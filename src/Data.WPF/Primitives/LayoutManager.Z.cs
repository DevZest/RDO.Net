using System;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    partial class LayoutManager
    {
        private sealed class Z : LayoutManager
        {
            public Z(Template template, DataSet dataSet)
                : base(template, dataSet)
            {
                RefreshBlock();
            }

            private void RefreshBlock()
            {
                if (CurrentRow != null && Blocks.Count == 0)
                    BlockViews.RealizeFirstUnpinned(CurrentRow.Ordinal);
            }

            protected override void OnSetState(DataPresenterState dataPresenterState)
            {
                base.OnSetState(dataPresenterState);
                if (dataPresenterState == DataPresenterState.CurrentRow)
                {
                    BlockViews.VirtualizeAll();
                    RefreshBlock();
                }
            }

            protected override void PrepareMeasure()
            {
                RefreshBlock();
                if (BlockViews.Count == 1)
                    BlockViews[0].Measure(Size.Empty);  // Available size is ignored when preparing blocks
            }

            protected override Size MeasuredSize
            {
                get
                {
                    var range = Template.Range();
                    return range.IsEmpty ? new Size() : range.GetMeasuredSize(null);
                }
            }
        }
    }
}
