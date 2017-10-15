namespace SciFi.Environment.State {
    /// Provides an immutable copy of the object's state.
    /// This contract is not enforceable with C#'s type
    /// system, but bad things might happen if part of a
    /// snapshot changes without synchronization at every
    /// access point.
    public interface IStateSnapshotProvider<S> where S: struct {
        /// Get an immutable snapshot of the object's state.
        /// If any part of the snapshot is can be changed
        /// through a reference, all accesses must be synchronized.
        void GetStateSnapshot(ref S snapshot);
    }
}