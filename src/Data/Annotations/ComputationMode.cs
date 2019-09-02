namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Indicates how computation is performed.
    /// </summary>
    public enum ComputationMode
    {
        /// <summary>
        /// Computation is performed bottom-up.
        /// </summary>
        Aggregate,
        /// <summary>
        /// Computation is performed top-down.
        /// </summary>
        Inherit
    }
}
