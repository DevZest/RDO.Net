using DevZest.Data.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class LayoutZManager : LayoutManager
    {
        internal LayoutZManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy)
            : base(template, dataSet, where, orderBy, true)
        {
        }

        private void RefreshBlock()
        {
            if (CurrentRow != null && BlockViewList.Count == 0)
                BlockViewList.RealizeFirst(CurrentRow.Index);
        }

        protected override void OnCurrentRowChanged()
        {
            BlockViewList.VirtualizeAll();
            RefreshBlock();
            base.OnCurrentRowChanged();
        }

        protected override void PrepareMeasureBlocks()
        {
            RefreshBlock();
            if (BlockViewList.Count == 1)
                BlockViewList[0].Measure(Size.Empty);  // Available size is ignored when preparing blocks
        }

        protected override Point GetScalarItemLocation(ScalarItem scalarItem, int blockDimension)
        {
            Debug.Assert(blockDimension == 0);
            return scalarItem.GridRange.MeasuredLocation;
        }

        protected override Size GetScalarItemSize(ScalarItem scalarItem)
        {
            return scalarItem.GridRange.MeasuredSize;
        }

        internal override Thickness GetScalarItemClip(ScalarItem scalarItem, int blockDimension)
        {
            return new Thickness();
        }

        protected override Point GetBlockLocation(BlockView block)
        {
            return Template.Range().GetLocation(Template.BlockRange);
        }

        protected override Size GetBlockSize(BlockView block)
        {
            return Template.BlockRange.MeasuredSize;
        }

        internal override Thickness GetBlockClip(BlockView block)
        {
            return new Thickness();
        }

        protected override Point GetBlockItemLocation(BlockView blockView, BlockItem blockItem)
        {
            return Template.BlockRange.GetLocation(blockItem.GridRange);
        }

        protected override Size GetBlockItemSize(BlockView block, BlockItem blockItem)
        {
            return blockItem.GridRange.MeasuredSize;
        }

        internal override Thickness GetBlockItemClip(BlockView block, BlockItem blockItem)
        {
            return new Thickness();
        }

        protected override Point GetRowLocation(BlockView blockView, int blockDimension)
        {
            Debug.Assert(blockDimension == 0);
            return Template.BlockRange.GetLocation(Template.RowRange);
        }

        protected override Size GetRowSize(BlockView block, int blockDimension)
        {
            return Template.RowRange.MeasuredSize;
        }

        internal override Thickness GetRowClip(int blockDimension)
        {
            return new Thickness();
        }

        protected override Point GetRowItemLocation(RowPresenter row, RowItem rowItem)
        {
            return Template.RowRange.GetLocation(rowItem.GridRange);
        }

        protected override Size GetRowItemSize(RowPresenter row, RowItem rowItem)
        {
            return rowItem.GridRange.MeasuredSize;
        }

        internal override Thickness GetRowItemClip(RowPresenter row, RowItem rowItem)
        {
            return new Thickness();
        }

        protected override Size MeasuredSize
        {
            get { return Template.Range().MeasuredSize; }
        }

        internal override IEnumerable<GridLineFigure> GridLineFigures
        {
            get
            {
                foreach (var gridLine in Template.GridLines)
                {
                    var startPoint = gridLine.StartGridPoint.ToPoint(Template);
                    var endPoint = gridLine.EndGridPoint.ToPoint(Template);
                    yield return new GridLineFigure(gridLine, startPoint, endPoint);
                }
            }
        }
    }
}
