using System;

using SciFi.Players.Hooks;

namespace SciFi.Players.Modifiers {
    public class Modifier {
        public ModId Id { get; private set; }

        /// This returns the expected new state -
        /// the state should not be updated if the values don't match.
        Func<ModId, bool, bool> stateWillChange;

        uint count;
        public uint Count {
            get {
                return count;
            }
            set {
                if (count == 0 && value > 0) {
                    if (!stateWillChange(Id, true)) {
                        return;
                    }
                    if (hook != null) {
                        hook.Enable();
                    }
                } else if (count > 0 && value == 0) {
                    if (stateWillChange(Id, false)) {
                        return;
                    }
                    if (hook != null) {
                        hook.Disable();
                    }
                }
                count = value;
            }
        }

        Hook hook;
        public Hook Hook {
            set {
                if (hook != null) {
                    throw new InvalidOperationException("Hook is already assigned.");
                }
                hook = value;
                if (Count == 0) {
                    hook.Disable();
                } else {
                    hook.Enable();
                }
            }
        }

        public Modifier(ModId id, Func<ModId, bool, bool> stateWillChange) {
            this.Id = id;
            this.stateWillChange = stateWillChange;
        }

        public void Add() {
            ++Count;
        }

        public void Remove() {
            if (Count == 0) {
                return;
            }
            --Count;
        }

        public bool IsEnabled() {
            return Count > 0;
        }
    }
}