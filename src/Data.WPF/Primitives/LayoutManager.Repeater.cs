using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    partial class LayoutManager
    {
        private abstract class Repeater
        {
            private sealed class RepeaterZ : Repeater
            {
                public RepeaterZ(LayoutManager layoutManager)
                    : base(layoutManager)
                {
                }
            }

            private class RepeaterY : Repeater
            {
                public RepeaterY(LayoutManager layoutManager)
                    : base(layoutManager)
                {
                }
            }

            private class RepeaterX : Repeater
            {
                public RepeaterX(LayoutManager layoutManager)
                    : base(layoutManager)
                {
                }
            }

            protected Repeater(LayoutManager layoutManager)
            {
                Debug.Assert(layoutManager != null);
                LayoutManager = layoutManager;
            }

            public LayoutManager LayoutManager { get; private set; }

            protected Template Template
            {
                get { return LayoutManager.Template; }
            }

            protected virtual int CalculateCrossRepeats()
            {
                return 1;
            }

            protected Size AvailableSize { get; private set; }

            protected double AvailableWidth
            {
                get { return AvailableSize.Width; }
            }

            protected double AvailableHeight
            {
                get { return AvailableSize.Height; }
            }

            protected bool SizeToContentX
            {
                get { return double.IsPositiveInfinity(AvailableWidth); }
            }

            protected bool SizeToContentY
            {
                get { return double.IsPositiveInfinity(AvailableHeight); }
            }

            public Size Measure(Size availableSize)
            {
                AvailableSize = availableSize;
                throw new NotImplementedException();
            }

            private void RealizeElements()
            {
                LayoutManager.CrossRepeats = CalculateCrossRepeats();

            }

            private void MeasureAuto(ScalarItem scalarItem)
            {
                throw new NotImplementedException();
            }
        }
    }
}
