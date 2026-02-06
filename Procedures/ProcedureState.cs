namespace BGC.Procedures
{
    /// <summary>
    /// Base record for all procedure states. Using a record enforces:
    /// - Immutability (init-only properties)
    /// - Value equality (for testing/comparison)
    /// - Built-in with-expressions for clean state transitions
    /// </summary>
    public abstract record ProcedureState
    {
        /// <summary>
        /// Monotonically increasing step counter. Managed by the framework.
        /// </summary>
        public int StepNumber { get; init; } = 0;
    }
}
