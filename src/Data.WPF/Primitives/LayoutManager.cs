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
                double totalAutoWidth = measuredSize.Width - gridRange.GetMeasuredWidth(x => !x.IsAutoLength(Template.SizeToContentX));
                if (totalAutoWidth > 0)
                {
                    var changed = DistributeAutoLength(blockView, templateItem.AutoWidthGridColumns, totalAutoWidth);
                    if (changed)
                        Template.DistributeStarWidths();
                }
            }

            if (templateItem.AutoHeightGridRows.Count > 0)
            {
                double totalAutoHeight = measuredSize.Height - gridRange.GetMeasuredHeight(x => !x.IsAutoLength(Template.SizeToContentY));
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
            gridTrack.SetMeasuredLength(value);
            gridTrack.Owner.TotalAutoLength += delta;
            return true;
        }

        protected abstract Size GetMeasuredSize(ScalarItem scalarItem);

        protected abstract Size GetMeasuredSize(BlockView blockView, GridRange gridRange);

        protected abstract Point GetOffset(BlockView blockView, GridRange baseGridRange, GridRange gridRange);

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
            PrepareMeasure(ScalarItems.AutoSizeItemsBefore);

            IsPreparingMeasure = true;
            PrepareMeasureBlocks();
            IsPreparingMeasure = false;

            PrepareMeasure(ScalarItems.AutoSizeItemsAfter);
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

            FinalizeMeasureBlocks();
            return MeasuredSize;
        }

        protected abstract void FinalizeMeasureBlocks();

        protected abstract Size MeasuredSize { get; }

        internal Size Measure(BlockView blockView, Size constraintSize)
        {
            if (IsPreparingMeasure)
                PrepareMeasure(blockView);
            else
                FinalizeMeasure(blockView);
            return constraintSize;
        }

        private void PrepareMeasure(BlockView blockView)
        {
            Debug.Assert(IsPreparingMeasure);

            PrepareMeasure(blockView, BlockItems.AutoSizeItemsBefore);

            for (int i = 0; i < blockView.Count; i++)
                blockView[i].View.Measure(Size.Empty);

            PrepareMeasure(blockView, BlockItems.AutoSizeItemsAfter);
        }

        private void PrepareMeasure(BlockView blockView, IReadOnlyList<BlockItem> blockItems)
        {
            foreach (var blockItem in blockItems)
            {
                var element = blockView[blockItem];
                element.Measure(blockItem.AvailableAutoSize);
                UpdateAutoSize(blockView, blockItem, element.DesiredSize);
            }
        }

        private void FinalizeMeasure(BlockView blockView)
        {
            Debug.Assert(!IsPreparingMeasure);

            foreach (var blockItem in BlockItems)
            {
                var element = blockView[blockItem];
                element.Measure(GetMeasuredSize(blockView, blockItem.GridRange));
            }

            if (blockView.Count > 0)
            {
                var size = GetMeasuredSize(blockView, Template.RowRange);
                for (int i = 0; i < blockView.Count; i++)
                    blockView[i].View.Measure(size);
            }
        }

        internal Size Measure(RowPresenter row, Size constraintSize)
        {
            if (IsPreparingMeasure)
                PrepareMeasure(row);
            else
                FinalizeMeasure(row);
            return constraintSize;
        }

        private void PrepareMeasure(RowPresenter row)
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

        private void FinalizeMeasure(RowPresenter row)
        {
            var rowItems = row.RowItems;
            if (rowItems.Count == 0)
                return;

            var blockView = BlockViews[row];
            Debug.Assert(blockView != null);
            foreach (var rowItem in rowItems)
            {
                var element = row.Elements[rowItem.Ordinal];
                element.Measure(GetMeasuredSize(blockView, rowItem.GridRange));
            }
        }

        protected abstract Point Offset(Point point, int blockDimension);

        internal Rect GetArrangeRect(ScalarItem scalarItem, int blockDimension)
        {
            var size = GetMeasuredSize(scalarItem);
            var point = Offset(scalarItem.GridRange.MeasuredPoint, blockDimension);
            return new Rect(point, size);
        }

        internal Size Arrange(Size finalSize)
        {
            foreach (var scalarItem in ScalarItems)
            {
                for (int i = 0; i < scalarItem.BlockDimensions; i++)
                {
                    var element = scalarItem[i];
                    element.Arrange(GetArrangeRect(scalarItem, i));
                }
            }

            ArrangeBlocks();
            return finalSize;
        }

        internal Rect GetArrangeRect(BlockView blockView)
        {
            var size = GetMeasuredSize(blockView, Template.BlockRange);
            var offset = GetOffset(blockView, Template.Range(), Template.BlockRange);
            return new Rect(offset, size);
        }

        private void ArrangeBlocks()
        {
            for (int i = 0; i < BlockViews.Count; i++)
            {
                var blockView = BlockViews[i];
                blockView.Arrange(GetArrangeRect(blockView));
            }
        }

        internal virtual Rect GetArrangeRect(BlockView blockView, BlockItem blockItem)
        {
            var size = GetMeasuredSize(blockView, blockItem.GridRange);
            var offset = GetOffset(blockView, Template.BlockRange, blockItem.GridRange);
            return new Rect(offset, size);
        }

        internal Rect GetArrangeRect(BlockView blockView, int blockDimension)
        {
            var size = GetMeasuredSize(blockView, Template.RowRange);
            var offset = Offset(GetOffset(blockView, Template.BlockRange, Template.RowRange), blockDimension);
            return new Rect(offset, size);
        }

        internal void Arrange(BlockView blockView)
        {
            foreach (var blockItem in BlockItems)
            {
                var element = blockView[blockItem];
                element.Arrange(GetArrangeRect(blockView, blockItem));
            }

            if (blockView.Count > 0)
            {
                var size = GetMeasuredSize(blockView, Template.RowRange);
                for (int i = 0; i < blockView.Count; i++)
                    blockView[i].View.Arrange(GetArrangeRect(blockView, i));
            }
        }

        internal Rect GetArrangeRect(BlockView blockView, RowItem rowItem)
        {
            var size = GetMeasuredSize(blockView, rowItem.GridRange);
            var offset = GetOffset(blockView, Template.RowRange, rowItem.GridRange);
            return new Rect(offset, size);
        }

        internal void Arrange(RowPresenter row)
        {
            var rowItems = row.RowItems;
            if (rowItems.Count == 0)
                return;

            var blockView = BlockViews[row];
            Debug.Assert(blockView != null);
            foreach (var rowItem in rowItems)
            {
                var element = row.Elements[rowItem.Ordinal];
                element.Arrange(GetArrangeRect(blockView, rowItem));
            }
        }
    }
}
