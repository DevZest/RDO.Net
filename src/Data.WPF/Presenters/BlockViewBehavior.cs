using DevZest.Data.Views;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents a behavior that can dynamically change the look-and-feel of <see cref="BlockView"/>.
    /// </summary>
    public abstract class BlockViewBehavior
    {
        /// <summary>
        /// Setup the <see cref="BlockView"/>.
        /// </summary>
        /// <param name="blockView">The <see cref="BlockView"/>.</param>
        protected internal abstract void Setup(BlockView blockView);

        /// <summary>
        /// Refresh the <see cref="BlockView"/>.
        /// </summary>
        /// <param name="blockView">The <see cref="BlockView"/>.</param>
        protected internal abstract void Refresh(BlockView blockView);

        /// <summary>
        /// Cleanup the <see cref="BlockView"/>.
        /// </summary>
        /// <param name="blockView">The <see cref="BlockView"/>.</param>
        protected internal abstract void Cleanup(BlockView blockView);
    }
}
