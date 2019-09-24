namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Identifies the status of validation.
    /// </summary>
    public enum ValidationStatus
    {
        /// <summary>
        /// The validation failed when flushing from view to presenter.
        /// </summary>
        FailedFlushing,

        /// <summary>
        /// The validation is running.
        /// </summary>
        Validating,

        /// <summary>
        /// The validation failed with validation error.
        /// </summary>
        Failed,

        /// <summary>
        /// The validation succeeded without any error.
        /// </summary>
        Succeeded
    }
}
