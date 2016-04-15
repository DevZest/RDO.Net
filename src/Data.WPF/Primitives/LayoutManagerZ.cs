using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class LayoutManagerZ : LayoutManager
    {
        internal LayoutManagerZ(Template template, DataSet dataSet)
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

        protected override void PrepareMeasureBlocks()
        {
            RefreshBlock();
            if (BlockViews.Count == 1)
                BlockViews[0].Measure(Size.Empty);  // Available size is ignored when preparing blocks
        }

        internal override Size GetMeasuredSize(DataItem dataItem, int blockDimension)
        {
            Debug.Assert(blockDimension == 0);
            return dataItem.GridRange.GetMeasuredSize(null);
        }

        internal override Rect GetArrangeRect(DataItem dataItem, int blockDimension)
        {
            var size = GetMeasuredSize(dataItem, blockDimension);
            var point = new Point(dataItem.GridRange.Left.MeasuredStartOffset, dataItem.GridRange.Top.MeasuredStartOffset);
            return new Rect(point, size);
        }

        protected override Size MeasuredSize
        {
            get
            {
                var lastGridColumn = Template.GridColumns.Last;
                double width = lastGridColumn == null ? 0 : lastGridColumn.MeasuredEndOffset;
                var lastGridRow = Template.GridRows.Last;
                double height = lastGridRow == null ? 0 : lastGridRow.MeasuredEndOffset;
                return new Size(width, height);
            }
        }
    }
}
