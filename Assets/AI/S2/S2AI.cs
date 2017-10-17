using UnityEngine;
using System;
using Random = System.Random;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ThreadPriority = System.Threading.ThreadPriority;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace SciFi.AI.S2 {
    /// A multithreaded utility (strategy) AI controller.
    /// When signaled, a thread will start a loop where it
    /// grabs a few indexes from a shared list and runs their
    /// evaluation routine. The results are stored per-thread,
    /// and when all processing is finished, the individual results
    /// are compared against each other and the final outcomes
    /// are chosen. If it turns out that not all data can be processed
    /// in a single step, it may help to maintain two lists and sort
    /// the unused one by use frequency, and then swap the lists
    /// every few cycles.
    public class S2AI : IDisposable {
        // -- Game & main thread environment --
        int aiCount;
        AIInputManager[] inputManagers;
        AIEnvironment env, env2;
        /// Dimensions = [AI ID, action group ID].
        int[,] strategyIdsRead;
        /// To avoid thread conflicts, we will write and read from two different
        /// arrays, and do an atomic swap when writing is done.
        int[,] strategyIdsWrite;
        /// Dimensions = [AI ID, action group ID].
        int[,] prevStrategyIds;
        Random mainThreadRandom;

        // -- Evaluator thread environment --
        bool running;
        int activeThreads;
        EventWaitHandle startEvent;
        List<Strategy> strategies;
        /// How many indexes should a thread grab at once?
        int blockSize;
        /// First index for the next block of evaluators. If this is
        /// greater or equal to the length of `evaluators`, we're done.
        int nextBlockStart;
        /// Note: To truly pick the best strategy when allowing
        /// multiple groups, I'd probably need to sort all the
        /// strategies and run through until all slots are filled.
        /// However, I don't think this case will be very common
        /// here, so I'll just keep a second place with a requirement
        /// that it can only apply to one actionGroup, and use it
        /// in case an item covering two groups gets replaced by
        /// one only applying to one.
        /// Dimensions = [thread ID, AI ID, action group ID * 2].
        Decision[][,] threadResults;
        Thread[] evalThreads;
        Random[] evalThreadRandom;

        // -- Controller state --
        Thread controlThread;
        int threadCtl;
        EventWaitHandle ctlEvent;
        //IEnumerable<Strategy> strategiesToAdd;
        //int aiIndexToRemove;
        //int frameSkip;

        private static class ThreadControl {
            public const int Shutdown       = 0;
            public const int AddStrategies  = 1;
            public const int AIDestroyed    = 2;
            public const int LateUpdate     = 3;
        }

        private struct Decision {
            /// A value from 0 to 1 indicating how good or bad
            /// this action would be.
            public readonly float utility;
            /// A bitmask of which action groups this decision occupies.
            public readonly uint actionGroupMask;
            /// The AI controller's strategy index.
            public readonly int strategyId;

            /// `utility` should be between 0 and 1.
            public Decision(float utility, uint actionGroupMask, int strategyId) {
                this.utility = utility;
                this.actionGroupMask = actionGroupMask;
                this.strategyId = strategyId;
            }
        }

        public S2AI(int threads, int blockSize) {
            this.running = false;
            this.activeThreads = 0;
            this.blockSize = blockSize;
            this.nextBlockStart = 0;
            this.threadResults = new Decision[threads][,];
            this.mainThreadRandom = new Random(DateTime.Now.Millisecond);
            this.evalThreadRandom = new Random[threads];
            for (int i = 0; i < threads; i++) {
                this.evalThreadRandom[i] = new Random(DateTime.Now.Millisecond * (i + 1));
            }
        }

        public void Ready(
            AIEnvironment env,
            IEnumerable<Strategy> strategies,
            IEnumerable<AIInputManager> inputManagers
        ) {
            this.env = env;
            this.env2 = new AIEnvironment(env);
            this.strategies = Shuffle(strategies.ToList());
            this.inputManagers = inputManagers.ToArray();
            this.aiCount = this.inputManagers.Length;

            int threads = threadResults.Length;
            for (int i = 0; i < threads; i++) {
                threadResults[i] = new Decision[aiCount, ActionGroup.Count];
            }
            this.strategyIdsRead = new int[aiCount, ActionGroup.Count];
            this.strategyIdsWrite = new int[aiCount, ActionGroup.Count];
            this.prevStrategyIds = new int[aiCount, ActionGroup.Count];

            this.startEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            this.ctlEvent = new EventWaitHandle(false, EventResetMode.ManualReset);

            Thread.MemoryBarrier();

            evalThreads
              = Enumerable.Range(0, threads)
                .Select(id => {
                    var t = new Thread(EvalThreadMain);
                    t.IsBackground = true;
                    t.Start(id);
                    return t;
                })
                .ToArray();

            controlThread = new Thread(ControlThreadMain);
            controlThread.IsBackground = true;
            controlThread.Priority = ThreadPriority.BelowNormal;
            controlThread.Start();
        }

        ~S2AI() {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing) {
            if (ctlEvent != null) {
                Threadctl(ThreadControl.Shutdown);
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// Rearrange the list so that each AI gets roughly
        /// equal processing time.
        private static List<Strategy> Shuffle(List<Strategy> strategies) {
            return strategies;
        }

        private static void ResetDecisionArray(Decision[,] decisions) {
            int aiCount = decisions.GetLength(0);
            int actionGroups = decisions.GetLength(1);
            for (int i = 0; i < aiCount; i++) {
                for (int j = 0; j < actionGroups; j++) {
                    decisions[i, j] = new Decision(0f, 0, -1);
                }
            }
        }

        private static void ResetStrategyIdArray(int[,] sids) {
            int aiCount = sids.GetLength(0);
            int actionGroups = sids.GetLength(1);
            for (int i = 0; i < aiCount; i++) {
                for (int j = 0; j < actionGroups; j++) {
                    sids[i, j] = -1;
                }
            }
        }

        /// Set non-thread-safe variables to thread-local copies before passing
        /// the environment to a strategy.
        private void SetupEnvForThread(AIEnvironment env, Random threadRandom) {
            env.threadRandom = threadRandom;
        }

        /// Start evaluating the first step, but don't take any action yet.
        public void BeginEvaluate() {
            ResetStrategyIdArray(prevStrategyIds);
            ResetStrategyIdArray(strategyIdsRead);
            for (int i = 0; i < threadResults.Length; i++) {
                ResetDecisionArray(threadResults[i]);
            }
            env.Update(Time.time);
            Thread.MemoryBarrier();
            Volatile.Write(ref running, true);
            Volatile.Write(ref nextBlockStart, 0);
            startEvent.Set();
        }

        /// Execute the current step and move into the next step immediately
        /// after.
        public void ExecAndMoveNext() {
            Volatile.Write(ref nextBlockStart, strategies.Count);
            // If processing finished, we'll just take the results right now.
            // Otherwise, we'll use the previous results and tell the control
            // thread to collect the new ones when the processing threads pause.
            Execute();
            if (Volatile.Read(ref activeThreads) == 0) {
                env.Update(Time.time);
                Thread.MemoryBarrier();
                Volatile.Write(ref nextBlockStart, 0);
                startEvent.Set();
            } else {
                env2.Update(Time.time);
                Thread.MemoryBarrier();
                Threadctl(ThreadControl.LateUpdate);
            }
        }

        private void MergeDecisions() {
            var strategyIdsWrite = Volatile.Read(ref this.strategyIdsWrite);
            // This should be ok as long as all the numbers stay low.
            for (int j = 0; j < aiCount; j++) {
                for (int k = 0; k < ActionGroup.Count; k++) {
                    // Inner loop is swapped for better distribution in case
                    // not all strategies are processed at the time we're
                    // picking which ones to use.
                    float bestUtility = 0f;
                    for (int i = 0; i < threadResults.Length; i++) {
                        var r = threadResults[i];
                        if (i == 0 || r[j, k].utility > bestUtility) {
                            bestUtility = r[j, k].utility;
                            strategyIdsWrite[j, k] = r[j, k].strategyId;
                        }
                    }
                }
            }
            Volatile.Write(ref this.strategyIdsWrite, this.strategyIdsRead);
            Volatile.Write(ref this.strategyIdsRead, strategyIdsWrite);
        }

        /// Returns true if there is a strategy to execute, false if not.
        private bool ActivateStrategy(
            int newSid,
            ref int oldSid,
            AIEnvironment env,
            AIInputManager inputManager
        )
        {
            bool hasNewStrategy = newSid >= 0;
            if (newSid == oldSid) { return hasNewStrategy; }
            if (oldSid > -1) {
                this.strategies[oldSid].Deactivate(env, inputManager);
            }
            if (newSid > -1) {
                this.strategies[newSid].Activate(env);
            }
            oldSid = newSid;
            return hasNewStrategy;
        }

        private void Execute() {
            var env = Volatile.Read(ref this.env);
            SetupEnvForThread(env, mainThreadRandom);
            var strategyIdsRead = Volatile.Read(ref this.strategyIdsRead);
            for (int i = 0; i < strategyIdsRead.GetLength(0); i++) {
                for (int j = 0; j < strategyIdsRead.GetLength(1); j++) {
                    int s = strategyIdsRead[i, j];
                    var im = inputManagers[i];
                    if (!ActivateStrategy(s, ref prevStrategyIds[i, j], env, im)) {
                        continue;
                    }
                    this.strategies[s].Execute(env, im);
                }
            }
        }

        /// To avoid races, _must_ call this from the main thread only.
        private void Threadctl(int ctl) {
            Volatile.Write(ref threadCtl, ctl);
            ctlEvent.Set();
        }

        private void ControlThreadMain() {
            var ctlEvent = Volatile.Read(ref this.ctlEvent);
            while (true) {
                ctlEvent.WaitOne();
                switch (Volatile.Read(ref threadCtl)) {
                case ThreadControl.Shutdown:
                    CtlShutdown();
                    return;
                case ThreadControl.AddStrategies:
                    CtlAddStrategies();
                    break;
                case ThreadControl.AIDestroyed:
                    CtlAIDestroyed();
                    break;
                case ThreadControl.LateUpdate:
                    CtlLateUpdate();
                    break;
                default:
                    throw new ArgumentException("Bad threadctl value", nameof(threadCtl));
                }
                ctlEvent.Reset();
            }
        }

        private void CtlShutdown() {
            // Set state to signal threads to terminate.
            // Order of events here is important so that no
            // thread can get stuck waiting for a dead event.
            // The thread resets the event before it checks
            // `running`, and sets it again after if an exit
            // was signaled. If all threads are waiting,
            // they will be freed after the `Set` call here
            // and exit; if any calls `Reset` after this `Set`
            // call, it will see the signaled exit and free
            // any other stuck threads.
            Volatile.Write(ref nextBlockStart, blockSize);
            Volatile.Write(ref running, false);
            startEvent.Set();
            SpinWait.SpinUntil(() => evalThreads.All(t => !t.IsAlive), 25);
            startEvent.Close();
            startEvent = null;
            ctlEvent.Close();
            ctlEvent = null;
        }

        private void CtlAddStrategies() {
            throw new NotImplementedException();
        }

        private void CtlAIDestroyed() {
            throw new NotImplementedException();
        }

        private void CtlLateUpdate() {
            SpinWait.SpinUntil(() => Volatile.Read(ref activeThreads) == 0);
            // We want to always update the env from the main thread,
            // but if we're not ready, we'll use the backup and then swap
            // them here when no other thread is using them.
            var tmp = env;
            Volatile.Write(ref env, env2);
            env2 = tmp;
            Volatile.Write(ref nextBlockStart, 0);
            startEvent.Set();
        }

        /// The basic idea is this: the threads all block on `startEvent`.
        /// When the event is set, they start processing slices of the array,
        /// guaranteed not to overlap via interlocked operations, and evenly
        /// distributed over all computer players by shuffling the array
        /// at the start.
        private void EvalThreadMain(object threadIdObj) {
            Volatile.Read(ref startEvent).WaitOne();
            int threadId = (int)threadIdObj;
            Interlocked.Increment(ref activeThreads);
            AIEnvironment env = Volatile.Read(ref this.env);
            // These don't change (for now).
            var strategyCount = Volatile.Read(ref this.strategies).Count;
            var threadCount = Volatile.Read(ref this.evalThreads).Length;
            var blockSize = Volatile.Read(ref this.blockSize);

            try {
                while (true) {
                    var blockEnd = Interlocked.Add(ref nextBlockStart, blockSize);
                    var blockStart = blockEnd - blockSize;
                    if (blockStart >= strategyCount) {
                        startEvent.Reset();
                        bool last = Interlocked.Decrement(ref activeThreads) == 0;
                        try {
                            if (!Volatile.Read(ref running)) {
                                // Consider: a thread reaches this point before
                                // `running` has been set to false, but
                                // another thread calls `Reset` before it can
                                // wait and after the controller has called
                                // `Set`. This thread would get stuck waiting
                                // forever unless the second thread undid its
                                // reset here.
                                startEvent.Set();
                                return;
                            } else if (last) {
                                MergeDecisions();
                            }
                            startEvent.WaitOne();
                        } catch {
                            // If we let an exception go between the
                            // decrement and increment, the counter
                            // will stop being accurate. This way, it will
                            // always be set to zero whether a thread
                            // terminates or they all are just paused.
                            return;
                        }
                        ResetDecisionArray(threadResults[threadId]);
                        Interlocked.Increment(ref activeThreads);
                        env = Volatile.Read(ref this.env);
                        continue;
                    }

                    if (blockEnd > strategyCount) {
                        blockEnd = strategyCount;
                    }
                    SetupEnvForThread(env, evalThreadRandom[threadId]);
                    for (int i = blockStart; i < blockEnd; i++) {
                        TakeBestStrategy(i, threadResults[threadId], env);
                    }
                }
            } catch {
                Interlocked.Decrement(ref activeThreads);
                return;
            }
        }

        private static readonly int[] deBruijnMultiplyTable = {
            0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
            31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
        };
        /// Find the index of the least significant bit set.
        /// <see href="http://graphics.stanford.edu/~seander/bithacks.html"/>
        private static int LeastBitIndex(uint value) {
            uint and = value & unchecked((uint)-value);
            uint mul = unchecked(and * 0x077CB531u);
            uint shift = mul >> 27;
            return deBruijnMultiplyTable[(int)shift];
        }

        /// For each item in `agMask`, add the difference between the new
        /// and old utility values. If the net utility is better, we want
        /// to replace all matches with the new value.
        private void TakeBestStrategy(
            int strategyIndex,
            Decision[,] existingDecisions,
            AIEnvironment env
        )
        {
            var strategy = strategies[strategyIndex];
            var utility = strategy.Evaluate(env);
            if (utility < .001f) {
                return;
            }
            uint agMask = strategy.actionGroupMask & ActionGroup.All;
            var decision = new Decision(utility, agMask, strategyIndex);

            // If I end up needing to support multiple groups per strategy:
            // In a loop for each bit of the new decision's mask, look
            // at the current best decision for that spot. For each spot
            // the current decision occupies but the new one doesn't, find
            // the net utility loss by replacing those with the second place
            // values and the net utility gain from placing the new decision
            // in the slots it needs. If the gain is more than the loss,
            // replace it. Don't erase the second place values when moving
            // them into first - they may need to replace a future value.
            // If the current decision only has one bit set and its utility
            // is greater than the second place's utility, even if it is
            // also greater than first place's utility, replace second.

            int agIndex = LeastBitIndex(agMask);
            int aiIndex = strategy.aiIndex;
            // It's ok if this is being written to - if it is, that means
            // the result from this thread will not be used and we will get
            // reset by `CtlLateUpdate`.
            var oldStrategyIndex = prevStrategyIds[aiIndex, agIndex];
            if (
                oldStrategyIndex != -1
                  && !strategies[oldStrategyIndex]
                    .CanTransitionTo(strategy.GetType())
            )
            {
                return;
            }

            var oldUtility = existingDecisions[aiIndex, agIndex].utility;
            if (utility > oldUtility) {
                existingDecisions[aiIndex, agIndex] = decision;
            }
        }
    }
}