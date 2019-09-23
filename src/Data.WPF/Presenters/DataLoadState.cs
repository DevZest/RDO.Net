namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Describes the state of data loading.
    /// </summary>
    public enum DataLoadState
    {
        /// <summary>
        /// The data loading is not running.
        /// </summary>
        Idle,

        /// <summary>
        /// The data loading is running.
        /// </summary>
        Loading,

        /// <summary>
        /// The data has been loaded successfully.
        /// </summary>
        Succeeded,

        /// <summary>
        /// The data loading failed.
        /// </summary>
        Failed,

        /// <summary>
        /// The data loading is being cancelled.
        /// </summary>
        Cancelling,

        /// <summary>
        /// The data loading has been cancelled.
        /// </summary>
        Cancelled
    }
}
