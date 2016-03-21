namespace DevZest.Data.Windows
{
    /// <summary>Specifies condition for generating EOF <see cref="RowPresenter"/>.</summary>
    public enum EofRowMapping
    {
        /// <summary>No EOF <see cref="RowPresenter"/> will be generated.</summary>
        Never,

        /// <summary>EOF <see cref="RowPresenter"/> will always be generated.</summary>
        Always,

        /// <summary>EOF <see cref="RowPresenter"/> will be generated only when there is no data.</summary>
        NoData
    }
}
