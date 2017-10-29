namespace SciFi.Players.Hooks {
    public abstract class Hook {
        public bool IsEnabled { get; private set; }

        public Hook() {
            IsEnabled = true;
        }

        public void Enable() {
            IsEnabled = true;
        }

        public void Disable() {
            IsEnabled = false;
        }

        public abstract void Install(HookCollection hooks);
        public abstract void Remove(HookCollection hooks);
    }
}