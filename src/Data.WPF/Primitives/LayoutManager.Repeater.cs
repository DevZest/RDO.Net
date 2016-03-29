using System;
using System.Diagnostics;
using System.Linq;
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

            private sealed class RepeaterXY : RepeaterY
            {
                public RepeaterXY(LayoutManager layoutManager)
                    : base(layoutManager)
                {
                }

                protected override int CalculateFlowCount()
                {
                    return SizeToContentX ? 1 : (int)(AvailableWidth / (Template.GridColumns.Sum(x => x.Width.Value)));
                }
            }

            private class RepeaterX : Repeater
            {
                public RepeaterX(LayoutManager layoutManager)
                    : base(layoutManager)
                {
                }
            }

            private sealed class RepeaterYX : RepeaterX
            {
                public RepeaterYX(LayoutManager layoutManager)
                    : base(layoutManager)
                {
                }

                protected override int CalculateFlowCount()
                {
                    return SizeToContentY ? 1 : (int)(AvailableHeight / (Template.GridRows.Sum(x => x.Height.Value)));
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

            protected virtual int CalculateFlowCount()
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
                LayoutManager.FlowCount = CalculateFlowCount();

            }

            private void MeasureAuto(ScalarItem scalarItem)
            {
                throw new NotImplementedException();
            }
        }
    }
}
