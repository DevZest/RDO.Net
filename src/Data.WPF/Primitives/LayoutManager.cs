using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract partial class LayoutManager : ElementManager
    {
        internal static LayoutManager Create(DataPresenter dataPresenter)
        {
            var result = LayoutManager.Create(dataPresenter.Template, dataPresenter.DataSet);
            result.DataPresenter = dataPresenter;
            return result;
        }

        internal static LayoutManager Create(Template template, DataSet dataSet)
        {
            if (!template.Orientation.HasValue)
                return new LayoutZManager(template, dataSet);
            else if (template.Orientation.GetValueOrDefault() == Orientation.Horizontal)
                return new LayoutXManager(template, dataSet);
            else
                return new LayoutYManager(template, dataSet);
        }

        protected LayoutManager(Template template, DataSet dataSet)
            : base(template, dataSet)
        {
        }

        internal DataPresenter DataPresenter { get; private set; }

        private TemplateItemCollection<ScalarItem> ScalarItems
        {
            get { return Template.InternalScalarItems; }
        }

        private TemplateItemCollection<BlockItem> BlockItems
        {
            get { return Template.InternalBlockItems; }
        }

        private void UpdateAutoSize(BlockView blockView, TemplateItem templateItem, Size measuredSize)
        {
            Debug.Assert(templateItem.IsAutoSize);

            var gridRange = templateItem.GridRange;
            if (templateItem.AutoWidthGridColumns.Count > 0)
            {
                double totalAutoWidth = measuredSize.Width - gridRange.GetMeasuredWidth(x => !x.IsAutoLength);
                if (totalAutoWidth > 0)
                {
                    var changed = DistributeAutoLength(blockView, templateItem.AutoWidthGridColumns, totalAutoWidth);
                    if (changed)
                        Template.DistributeStarWidths();
                }
            }

            if (templateItem.AutoHeightGridRows.Count > 0)
            {
                double totalAutoHeight = measuredSize.Height - gridRange.GetMeasuredHeight(x => !x.IsAutoLength);
                if (totalAutoHeight > 0)
                {
                    var changed = DistributeAutoLength(blockView, templateItem.AutoHeightGridRows, totalAutoHeight);
                    if (changed)
                        Template.DistributeStarHeights();
                }
            }
        }

        private bool DistributeAutoLength<T>(BlockView blockView, IReadOnlyList<T> autoLengthTracks, double totalMeasuredLength)
            where T : GridTrack
        {
            Debug.Assert(autoLengthTracks.Count > 0);
            Debug.Assert(totalMeasuredLength > 0);

            if (autoLengthTracks.Count == 1)
            {
                var track = autoLengthTracks[0];
                if (totalMeasuredLength > GetMeasuredLength(blockView, track))
                    return SetMeasuredAutoLength(blockView, track, totalMeasuredLength);
            }

            return DistributeOrderedAutoLength(blockView, autoLengthTracks.OrderByDescending(x => x.MeasuredLength).ToArray(), totalMeasuredLength);
        }

        private bool DistributeOrderedAutoLength<T>(BlockView blockView, IReadOnlyList<T> orderedAutoLengthTracks, double totalMeasuredLength)
            where T : GridTrack
        {
            Debug.Assert(orderedAutoLengthTracks.Count > 0);
            Debug.Assert(totalMeasuredLength > 0);

            var count = orderedAutoLengthTracks.Count;
            double avgLength = totalMeasuredLength / count;
            var result = false;
            for (int i = 0; i < count; i++)
            {
                var track = orderedAutoLengthTracks[i];
                var trackMeasuredLength = GetMeasuredLength(blockView, track);
                if (trackMeasuredLength >= avgLength)
                {
                    totalMeasuredLength -= trackMeasuredLength;
                    avgLength = totalMeasuredLength / (count - i + 1);
                }
                else
                    result = SetMeasuredAutoLength(blockView, track, avgLength);
            }
            return result;
        }

        protected virtual double GetMeasuredLength(BlockView blockView, GridTrack gridTrack)
        {
            return gridTrack.MeasuredLength;
        }

        protected virtual bool SetMeasuredAutoLength(BlockView blockView, GridTrack gridTrack, double value)
        {
            var delta = value - gridTrack.MeasuredLength;
            Debug.Assert(delta > 0);
            gridTrack.MeasuredLength = value;
            gridTrack.Owner.TotalAutoLength += delta;
            return true;
        }

        protected abstract Size GetMeasuredSize(ScalarItem scalarItem);

        protected abstract Size GetMeasuredSize(BlockView blockView, GridRange gridRange, bool clipScrollCross);

        internal virtual Size Measure(Size availableSize)
        {
            Template.InitMeasure(availableSize);
            BlockDimensions = Template.CoerceBlockDimensions();
            PrepareMeasure();
            var result = FinalizeMeasure();
            return result;
        }

        private bool IsPreparingMeasure;

        private void PrepareMeasure()
        {
            PrepareMeasure(ScalarItems.AutoSizeItems);

            IsPreparingMeasure = true;
            PrepareMeasureBlocks();
            IsPreparingMeasure = false;
        }

        private void PrepareMeasure(IReadOnlyCollection<ScalarItem> scalarItems)
        {
            foreach (var scalarItem in scalarItems)
            {
                Debug.Assert(scalarItem.BlockDimensions == 1, "Auto size is not allowed with multidimensional ScalarItem.");
                var element = scalarItem[0];
                element.Measure(scalarItem.AvailableAutoSize);
                UpdateAutoSize(null, scalarItem, element.DesiredSize);
            }
        }

        protected abstract void PrepareMeasureBlocks();

        private Size FinalizeMeasure()
        {
            foreach (var scalarItem in ScalarItems)
            {
                for (int i = 0; i < scalarItem.BlockDimensions; i++)
                {
                    var element = scalarItem[i];
                    element.Measure(GetMeasuredSize(scalarItem));
                }
            }

            for (int i = 0; i < BlockViews.Count; i++)
            {
                var block = BlockViews[i];
                block.Measure(GetMeasuredSize(block));
            }

            return MeasuredSize;
        }

        protected abstract Size GetMeasuredSize(BlockView block);

        protected abstract Size MeasuredSize { get; }

        internal Size MeasureBlock(BlockView blockView, Size constraintSize)
        {
            if (IsPreparingMeasure)
                PrepareMeasureBlock(blockView);
            else
                FinalizeMeasureBlock(blockView);
            return constraintSize;
        }

        private void PrepareMeasureBlock(BlockView blockView)
        {
            Debug.Assert(IsPreparingMeasure);

            PrepareMeasureBlockItems(blockView, BlockItems.AutoSizeItems);

            for (int i = 0; i < blockView.Count; i++)
                blockView[i].View.Measure(Size.Empty);
        }

        private void PrepareMeasureBlockItems(BlockView blockView, IReadOnlyList<BlockItem> blockItems)
        {
            foreach (var blockItem in blockItems)
            {
                Debug.Assert(blockItem.IsAutoSize);
                var element = blockView[blockItem];
                element.Measure(blockItem.AvailableAutoSize);
                UpdateAutoSize(blockView, blockItem, element.DesiredSize);
            }
        }

        private void FinalizeMeasureBlock(BlockView blockView)
        {
            Debug.Assert(!IsPreparingMeasure);

            foreach (var blockItem in BlockItems)
            {
                var element = blockView[blockItem];
                element.Measure(GetMeasuredSize(blockView, blockItem.GridRange, false));
            }

            if (blockView.Count > 0)
            {
                var size = GetMeasuredSize(blockView, Template.RowRange, true);
                for (int i = 0; i < blockView.Count; i++)
                    blockView[i].View.Measure(size);
            }
        }

        internal Size MeasureRow(RowPresenter row, Size constraintSize)
        {
            if (IsPreparingMeasure)
                PrepareMeasureRow(row);
            else
                FinalizeMeasureRow(row);
            return constraintSize;
        }

        private void PrepareMeasureRow(RowPresenter row)
        {
            var rowItems = row.RowItems;
            if (rowItems.AutoSizeItems.Count == 0)
                return;

            var blockView = BlockViews[row];
            Debug.Assert(blockView != null);
            foreach (var rowItem in rowItems.AutoSizeItems)
            {
                var element = row.Elements[rowItem.Ordinal];
                element.Measure(rowItem.AvailableAutoSize);
                UpdateAutoSize(blockView, rowItem, element.DesiredSize);
            }
        }

        private void FinalizeMeasureRow(RowPresenter row)
        {
            var rowItems = row.RowItems;
            if (rowItems.Count == 0)
                return;

            var blockView = BlockViews[row];
            Debug.Assert(blockView != null);
            foreach (var rowItem in rowItems)
            {
                var element = row.Elements[rowItem.Ordinal];
                element.Measure(GetMeasuredSize(blockView, rowItem.GridRange, false));
            }
        }

        internal Rect GetScalarItemRect(ScalarItem scalarItem, int blockDimension)
        {
            var point = GetScalarItemLocation(scalarItem, blockDimension);
            var size = GetMeasuredSize(scalarItem);
            return new Rect(point, size);
        }

        protected abstract Point GetScalarItemLocation(ScalarItem scalarItem, int blockDimension);

        internal Size Arrange(Size finalSize)
        {
            foreach (var scalarItem in ScalarItems)
            {
                for (int i = 0; i < scalarItem.BlockDimensions; i++)
                {
                    var element = scalarItem[i];
                    ArrangeScalarItem(element, GetScalarItemRect(scalarItem, i));
                }
            }

            ArrangeBlocks();
            return finalSize;
        }

        protected virtual void ArrangeScalarItem(UIElement element, Rect rect)
        {
            element.Arrange(rect);
        }

        internal Rect GetBlockRect(BlockView blockView)
        {
            var offset = GetBlockLocation(blockView);
            var size = GetMeasuredSize(blockView);
            return new Rect(offset, size);
        }

        protected abstract Point GetBlockLocation(BlockView block);

        private void ArrangeBlocks()
        {
            for (int i = 0; i < BlockViews.Count; i++)
            {
                var block = BlockViews[i];
                ArrangeBlock(block, GetBlockRect(block));
            }
        }

        protected virtual void ArrangeBlock(BlockView blockView, Rect rect)
        {
            blockView.Arrange(rect);
        }

        internal Rect GetBlockItemRect(BlockView blockView, BlockItem blockItem)
        {
            var location = GetBlockItemLocation(blockView, blockItem);
            var size = GetMeasuredSize(blockView, blockItem.GridRange, false);
            return new Rect(location, size);
        }

        protected abstract Point GetBlockItemLocation(BlockView block, BlockItem blockItem);

        internal Rect GetRowRect(BlockView block, int blockDimension)
        {
            var location = GetRowLocation(block, blockDimension);
            var size = GetMeasuredSize(block, Template.RowRange, true);
            return new Rect(location, size);
        }

        protected abstract Point GetRowLocation(BlockView block, int blockDimension);

        internal void ArrangeBlock(BlockView block)
        {
            foreach (var blockItem in BlockItems)
            {
                var element = block[blockItem];
                element.Arrange(GetBlockItemRect(block, blockItem));
            }

            if (block.Count > 0)
            {
                var size = GetMeasuredSize(block, Template.RowRange, true);
                for (int i = 0; i < block.Count; i++)
                    block[i].View.Arrange(GetRowRect(block, i));
            }
        }

        internal Rect GetRowItemRect(BlockView blockView, RowItem rowItem)
        {
            var location = GetRowItemLocation(blockView, rowItem);
            var size = GetMeasuredSize(blockView, rowItem.GridRange, false);
            return new Rect(location, size);
        }

        protected abstract Point GetRowItemLocation(BlockView blockView, RowItem rowItem);

        internal void ArrangeRow(RowPresenter row)
        {
            var rowItems = row.RowItems;
            if (rowItems.Count == 0)
                return;

            var block = BlockViews[row];
            Debug.Assert(block != null);
            foreach (var rowItem in rowItems)
            {
                var element = row.Elements[rowItem.Ordinal];
                element.Arrange(GetRowItemRect(block, rowItem));
            }
        }
    }
}
