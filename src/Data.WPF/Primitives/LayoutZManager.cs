using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class LayoutZManager : LayoutManager
    {
        internal LayoutZManager(Template template, DataSet dataSet)
            : base(template, dataSet)
        {
        }

        private void RefreshBlock()
        {
            if (CurrentRow != null && Blocks.Count == 0)
                BlockViews.RealizeFirst(CurrentRow.Ordinal);
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

        protected override void PrepareMeasureBlocks()
        {
            RefreshBlock();
            if (BlockViews.Count == 1)
                BlockViews[0].Measure(Size.Empty);  // Available size is ignored when preparing blocks
        }

        protected override Size GetMeasuredSize(ScalarItem scalarItem)
        {
            return scalarItem.GridRange.MeasuredSize;
        }

        protected override Size GetMeasuredSize(BlockView blockView, GridRange gridRange)
        {
            Debug.Assert(Template.BlockRange.Contains(gridRange));
            return gridRange.MeasuredSize;
        }

        protected override Point Offset(Point point, int blockDimension)
        {
            Debug.Assert(blockDimension == 0);
            return point;
        }

        protected override Point GetOffset(BlockView blockView, GridRange baseGridRange, GridRange gridRange)
        {
            Debug.Assert(baseGridRange.Contains(gridRange));

            var basePoint = baseGridRange.MeasuredPoint;
            var point = gridRange.MeasuredPoint;
            return new Point(point.X - basePoint.X, point.Y - basePoint.Y);
        }

        protected override void FinalizeMeasureBlocks()
        {
            if (BlockViews.Count == 1)
                BlockViews[0].Measure(Template.BlockRange.MeasuredSize);
        }

        protected override Size MeasuredSize
        {
            get { return Template.Range().MeasuredSize; }
        }
    }
}
