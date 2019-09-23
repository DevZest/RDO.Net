using DevZest.Data.Views.Primitives;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Identifies the placement of current <see cref="ContainerView"/> in <see cref="ContainerViewList"/>.
    /// </summary>
    public enum CurrentContainerViewPlacement
    {
        /// <summary>
        /// The <see cref="ContainerViewList"/> is empty and there is no current <see cref="ContainerView"/>.
        /// </summary>
        None = 0,

        /// <summary>
        /// The current <see cref="ContainerView"/> is the only item in <see cref="ContainerViewList"/>.
        /// </summary>
        Alone,

        /// <summary>
        /// The current <see cref="ContainerView"/> is visible and surrounded by other visible <see cref="ContainerView"/> objects.
        /// </summary>
        WithinList,

        /// <summary>
        /// The current <see cref="ContainerView"/> is before the visible list of <see cref="ContainerView"/> objects.
        /// </summary>
        BeforeList,

        /// <summary>
        /// The current <see cref="ContainerView"/> is after the visible list of <see cref="ContainerView"/> objects.
        /// </summary>
        AfterList
    }
}
