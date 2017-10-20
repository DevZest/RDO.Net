using System;
using DevZest.Data.Views;
using System.Windows.Media;

namespace DevZest.Data.Presenters
{
    public sealed class RowViewAlternation : RowViewPlugin
    {
        public RowViewAlternation()
            : this(new Brush[] { Brushes.White, Brushes.LightGray })
        {
        }

        public RowViewAlternation(Brush[] backgroundBrushes)
        {
            if (backgroundBrushes == null)
                throw new ArgumentNullException(nameof(backgroundBrushes));

            _backgroudnBrushes = backgroundBrushes;
        }

        private Brush[] _backgroudnBrushes;

        public int AlternationCount
        {
            get { return _backgroudnBrushes.Length; }
        }

        protected internal override void Setup(RowView rowView)
        {
        }

        protected internal override void Refresh(RowView rowView)
        {
            if (_backgroudnBrushes.Length == 0)
                return;

            rowView.Background = _backgroudnBrushes[rowView.RowPresenter.Index % AlternationCount];
        }

        protected internal override void Cleanup(RowView rowView)
        {
        }
    }
}
