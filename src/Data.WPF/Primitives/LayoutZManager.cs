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

        protected override Size GetMeasuredSize(BlockView block)
        {
            return Template.BlockRange.MeasuredSize;
        }

        protected override Size GetMeasuredSize(BlockView blockView, GridRange gridRange)
        {
            Debug.Assert(Template.BlockRange.Contains(gridRange));
            return gridRange.MeasuredSize;
        }

        protected override Point GetScalarItemLocation(ScalarItem scalarItem, int blockDimension)
        {
            Debug.Assert(blockDimension == 0);
            return scalarItem.GridRange.MeasuredLocation;
        }

        protected override Point GetBlockViewLocation(BlockView blockView)
        {
            return Template.Range().GetLocation(Template.BlockRange);
        }

        protected override Point GetBlockItemLocation(BlockView blockView, BlockItem blockItem)
        {
            return Template.BlockRange.GetLocation(blockItem.GridRange);
        }

        protected override Point GetRowViewLocation(BlockView blockView, int blockDimension)
        {
            Debug.Assert(blockDimension == 0);
            return Template.BlockRange.GetLocation(Template.RowRange);
        }

        protected override Point GetRowItemLocation(BlockView blockView, RowItem rowItem)
        {
            return Template.RowRange.GetLocation(rowItem.GridRange);
        }

        protected override Size MeasuredSize
        {
            get { return Template.Range().MeasuredSize; }
        }
    }
}
