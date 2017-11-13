using System;
using DevZest.Data.Views;
using System.Windows.Media;

namespace DevZest.Data.Presenters
{
    public sealed class BlockViewAlternation : BlockViewBehavior
    {
        public BlockViewAlternation()
            : this(new Brush[] { Brushes.White, Brushes.LightGray })
        {
        }

        public BlockViewAlternation(Brush[] backgroundBrushes)
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

        protected internal override void Setup(BlockView blockView)
        {
        }

        protected internal override void Refresh(BlockView blockView)
        {
            if (_backgroudnBrushes.Length == 0)
                return;

            blockView.Background = _backgroudnBrushes[blockView.Ordinal % AlternationCount];
        }

        protected internal override void Cleanup(BlockView rowView)
        {
        }
    }
}
