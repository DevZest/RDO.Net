using System;
using DevZest.Data.Views;
using System.Windows.Media;

namespace DevZest.Data.Presenters.Plugins
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

        protected override void Setup(RowView rowView)
        {
        }

        protected override void Refresh(RowView rowView)
        {
            if (_backgroudnBrushes.Length == 0)
                return;

            rowView.Background = _backgroudnBrushes[rowView.RowPresenter.Index % AlternationCount];
        }

        protected override void Cleanup(RowView rowView)
        {
        }
    }
}
