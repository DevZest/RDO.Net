using System;
using DevZest.Data.Views;
using System.Windows.Media;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Displays different background for <see cref="RowView"/> elements.
    /// </summary>
    public sealed class RowViewAlternation : RowViewBehavior
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RowViewAlternation"/>, with white for even and light gray for odd <see cref="RowView"/> elements.
        /// </summary>
        public RowViewAlternation()
            : this(new Brush[] { Brushes.White, Brushes.LightGray })
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RowViewAlternation"/>, with specified background brushes.
        /// </summary>
        /// <param name="backgroundBrushes">The background brushes.</param>
        public RowViewAlternation(Brush[] backgroundBrushes)
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
        protected internal override void Setup(RowView rowView)
        {
        }

        /// <inheritdoc/>
        protected internal override void Refresh(RowView rowView)
        {
            if (_backgroudnBrushes.Length == 0)
                return;

            rowView.Background = _backgroudnBrushes[rowView.RowPresenter.Index % AlternationCount];
        }

        /// <inheritdoc/>
        protected internal override void Cleanup(RowView rowView)
        {
        }
    }
}
