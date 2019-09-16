namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents the status of async validator.
    /// </summary>
    public enum AsyncValidatorStatus
    {
        /// <summary>
        /// The async validator is inactive.
        /// </summary>
        Inactive,

        /// <summary>
        /// The async validator is running.
        /// </summary>
        Running,

        /// <summary>
        /// The async validator runs completely without any validation error.
        /// </summary>
        Validated,

        /// <summary>
        /// The async validator runs completely with validation error.
        /// </summary>
        Error,

        /// <summary>
        /// There was an error running the async validator.
        /// </summary>
        Faulted
    }
}
