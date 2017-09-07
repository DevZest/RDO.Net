using System;
using DevZest.Data.Views;
using System.Windows.Media;

namespace DevZest.Data.Presenters.Plugins
{
    public sealed class BlockViewAlternation : BlockViewPlugin
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

        protected override void Setup(BlockView blockView)
        {
            if (_backgroudnBrushes.Length == 0)
                return;

            blockView.Background = _backgroudnBrushes[blockView.Ordinal % AlternationCount];
        }

        protected override void Refresh(BlockView blockView)
        {
        }

        protected override void Cleanup(BlockView rowView)
        {
        }
    }
}
