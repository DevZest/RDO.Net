namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Specifies the validation mode.
    /// </summary>
    public enum ValidationMode
    {
        /// <summary>
        /// Validation is performed progressively.
        /// </summary>
        Progressive,

        /// <summary>
        /// Validaiton is performed implicitly.
        /// </summary>
        Implicit,

        /// <summary>
        /// Validation is performed explicitly.
        /// </summary>
        Explicit
    }
}
