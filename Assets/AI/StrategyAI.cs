using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

using SciFi.AI.Strategies;
using SciFi.Players;

namespace SciFi.AI {
    public class StrategyAI : AIBase {
        const float evaluateStrategyInteval = .25f;
        float evaluateNextStrategyTime;

        struct StrategyInfo {
            public StrategyPicker strategyPicker;
            public Strategy strategy;
            public int lastControl;
        }
        StrategyInfo[] strategyInfos;

        // Strategy params
        struct StrategyParams {
            public Player me;
            public Player opponent;
            public GameObject ground;
        }
        StrategyParams strategyParams;

        const int strategyListIndex = 0;

        void Start() {
            strategyParams = new StrategyParams {
                me = GetComponent<Player>(),
                opponent = GameController.Instance.GetPlayer(0),
                ground = GameObject.Find("FinalDest"),
            };

            var strategyList = GetStrategyList(strategyListIndex);
            Func<StrategyType, StrategyInfo> makeStrategyInfo = (type) => {
                return new StrategyInfo {
                    strategyPicker = new StrategyPicker(GetStrategiesOfType(strategyList, type)),
                };
            };

            // Important: These must be in the same order as the StrategyType enum.
            strategyInfos = new [] {
                makeStrategyInfo(StrategyType.Movement),
                makeStrategyInfo(StrategyType.Jump),
                makeStrategyInfo(StrategyType.Attack),
                makeStrategyInfo(StrategyType.Block),
            };
        }

        void Update() {
            if (Time.time > evaluateNextStrategyTime) {
                evaluateNextStrategyTime = Time.time + evaluateStrategyInteval;
                for (var i = 0; i < strategyInfos.Length; i++) {
                    PickStrategy(ref strategyInfos[i]);
                }
            }

            for (var i = 0; i < strategyInfos.Length; i++) {
                UseStrategy(ref strategyInfos[i]);
            }
        }

        void PickStrategy(ref StrategyInfo info) {
            var newStrategy = info.strategyPicker.Pick();
            if (ReferenceEquals(newStrategy, info.strategy)) {
                return;
            }
            if (info.strategy != null) {
                info.strategy.OnDeactivate();
            }
            if (newStrategy != null) {
                newStrategy.OnActivate();
            }
            info.strategy = newStrategy;
        }

        void UseStrategy(ref StrategyInfo info) {
            if (info.strategy == null) {
                if (info.lastControl != Control.None) {
                    inputManager.Release(info.lastControl);
                    info.lastControl = Control.None;
                }
                return;
            }
            int control = info.strategy.GetControl();
            if (control != info.lastControl) {
                inputManager.Release(info.lastControl);
                inputManager.Press(control);
            }
            info.lastControl = control;
        }

        Strategy GetActiveStrategy(StrategyType type) {
            return strategyInfos[(int)type].strategy;
        }

        Strategy[] GetStrategyList(int list) {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Strategy)))
                .Where(t => t.GetCustomAttributes(false).Any(a => (a is StrategyListAttribute) && (((StrategyListAttribute)a).list == list)))
                .Select(t => InitializeStrategy(t))
                .ToArray();
        }

        static Strategy[] GetStrategiesOfType(Strategy[] list, StrategyType type) {
            return list
                .Where(t => t.GetType().GetCustomAttributes(true).Any(a => (a is StrategyTypeAttribute) && (((StrategyTypeAttribute)a).type == type)))
                .ToArray();
        }

        Strategy InitializeStrategy(Type type) {
            object[] paramValues;
            var constructor = type.GetConstructors().FirstOrDefault(c =>
                c.GetParameters().All(p =>
                    p.GetCustomAttributes(false).Any(a => a is StrategyParamAttribute)
                )
            );
            if (constructor == null) {
                constructor = type.GetConstructor(Type.EmptyTypes);
                paramValues = null;
            } else {
                paramValues = constructor.GetParameters()
                    .Select(p => (StrategyParamAttribute)p.GetCustomAttributes(false).Single(a => a is StrategyParamAttribute))
                    .Select(a => {
                        object val;
                        switch (a.type) {
                        case StrategyParamType.Me:
                            val = strategyParams.me;
                            break;
                        case StrategyParamType.Opponent:
                            val = strategyParams.opponent;
                            break;
                        case StrategyParamType.Ground:
                            val = strategyParams.ground;
                            break;
                        case StrategyParamType.GetActiveStrategy:
                            val = (Func<StrategyType, Strategy>)GetActiveStrategy;
                            break;
                        default:
                            val = null;
                            break;
                        }
                        return val;
                    })
                    .ToArray();
            }

            return (Strategy)constructor.Invoke(paramValues);
        }
    }
}