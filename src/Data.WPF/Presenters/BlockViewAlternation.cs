using System;
using DevZest.Data.Views;
using System.Windows.Media;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Displays different background for <see cref="BlockView"/> elements.
    /// </summary>
    public sealed class BlockViewAlternation : BlockViewBehavior
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BlockViewAlternation"/>, with white for even and light gray for odd <see cref="BlockView"/> elements.
        /// </summary>
        public BlockViewAlternation()
            : this(new Brush[] { Brushes.White, Brushes.LightGray })
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BlockViewAlternation"/>, with specified background brushes.
        /// </summary>
        /// <param name="backgroundBrushes">The background brushes.</param>
        public BlockViewAlternation(Brush[] backgroundBrushes)
        {
            if (backgroundBrushes == null)
                throw new ArgumentNullException(nameof(backgroundBrushes));

            _backgroudnBrushes = backgroundBrushes;
        }

        private Brush[] _backgroudnBrushes;

        /// <summary>
        /// Gets the count of background brushes.
        /// </summary>
        public int AlternationCount
        {
            get { return _backgroudnBrushes.Length; }
        }

        /// <inheritdoc/>
        protected internal override void Setup(BlockView blockView)
        {
        }

        /// <inheritdoc/>
        protected internal override void Refresh(BlockView blockView)
        {
            if (_backgroudnBrushes.Length == 0)
                return;

            blockView.Background = _backgroudnBrushes[blockView.Ordinal % AlternationCount];
        }

        /// <inheritdoc/>
        protected internal override void Cleanup(BlockView rowView)
        {
        }
    }
}
