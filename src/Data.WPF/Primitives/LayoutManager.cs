using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        private RecapItemCollection<ScalarItem> ScalarItems
        {
            get { return Template.InternalScalarItems; }
        }

        private RecapItemCollection<BlockItem> BlockItems
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
                    DistributeAutoLength(blockView, templateItem.AutoWidthGridColumns, totalAutoWidth);
                    Template.DistributeStarWidths();
                }
            }

            if (templateItem.AutoHeightGridRows.Count > 0)
            {
                double totalAutoHeight = measuredSize.Height - gridRange.GetMeasuredHeight(x => !x.IsAutoLength);
                if (totalAutoHeight > 0)
                {
                    DistributeAutoLength(blockView, templateItem.AutoHeightGridRows, totalAutoHeight);
                    Template.DistributeStarHeights();
                }
            }
        }

        private void DistributeAutoLength<T>(BlockView blockView, IReadOnlyList<T> autoLengthTracks, double totalMeasuredLength)
            where T : GridTrack
        {
            Debug.Assert(autoLengthTracks.Count > 0);
            Debug.Assert(totalMeasuredLength > 0);

            if (autoLengthTracks.Count == 1)
            {
                var track = autoLengthTracks[0];
                if (totalMeasuredLength > GetMeasuredLength(blockView, track))
                    SetMeasuredAutoLength(blockView, track, totalMeasuredLength);
            }
            else
                DistributeOrderedAutoLength(blockView, autoLengthTracks.OrderByDescending(x => x.MeasuredLength).ToArray(), totalMeasuredLength);
        }

        private void DistributeOrderedAutoLength<T>(BlockView blockView, IReadOnlyList<T> orderedAutoLengthTracks, double totalMeasuredLength)
            where T : GridTrack
        {
            Debug.Assert(orderedAutoLengthTracks.Count > 0);
            Debug.Assert(totalMeasuredLength > 0);

            var count = orderedAutoLengthTracks.Count;
            double avgLength = totalMeasuredLength / count;
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
                    SetMeasuredAutoLength(blockView, track, avgLength);
            }
        }

        private bool IsVariantLength(BlockView block, GridTrack gridTrack)
        {
            return block != null && gridTrack.VariantByBlock;
        }

        protected double GetMeasuredLength(BlockView block, GridTrack gridTrack)
        {
            return IsVariantLength(block, gridTrack) ? block.GetMeasuredLength(gridTrack) : gridTrack.MeasuredLength;
        }

        private void SetMeasuredAutoLength(BlockView block, GridTrack gridTrack, double value)
        {
            if (IsVariantLength(block, gridTrack))
                block.SetMeasuredLength(gridTrack, value);
            else
            {
                var delta = value - gridTrack.MeasuredLength;
                Debug.Assert(delta > 0);
                gridTrack.MeasuredLength = value;
                gridTrack.Owner.TotalAutoLength += delta;
            }
        }

        protected abstract Size GetScalarItemSize(ScalarItem scalarItem);

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
            IsPreparingMeasure = true;
            PrepareMeasure(ScalarItems.PreAutoSizeItems);
            PrepareMeasureBlocks();
            PrepareMeasure(ScalarItems.PostAutoSizeItems);
            IsPreparingMeasure = false;
        }

        private void PrepareMeasure(IEnumerable<ScalarItem> scalarItems)
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
                    element.Measure(GetScalarItemSize(scalarItem));
                }
            }

            for (int i = 0; i < BlockViews.Count; i++)
            {
                var block = BlockViews[i];
                block.Measure(GetBlockSize(block));
            }

            return MeasuredSize;
        }

        protected abstract Size GetBlockSize(BlockView block);

        protected abstract Size MeasuredSize { get; }

        internal Size MeasureBlock(BlockView blockView, Size constraintSize)
        {
            if (IsPreparingMeasure)
                PrepareMeasureBlock(blockView);
            else
                FinalizeMeasureBlock(blockView);
            return constraintSize;
        }

        private void PrepareMeasureBlock(BlockView block)
        {
            Debug.Assert(IsPreparingMeasure);

            PrepareMeasureBlockItems(block, BlockItems.PreAutoSizeItems);

            for (int i = 0; i < block.Count; i++)
                block[i].View.Measure(Size.Empty);

            PrepareMeasureBlockItems(block, BlockItems.PostAutoSizeItems);
        }

        private void PrepareMeasureBlockItems(BlockView blockView, IEnumerable<BlockItem> blockItems)
        {
            foreach (var blockItem in blockItems)
            {
                Debug.Assert(blockItem.IsAutoSize);
                var element = blockView[blockItem];
                element.Measure(blockItem.AvailableAutoSize);
                UpdateAutoSize(blockView, blockItem, element.DesiredSize);
            }
        }

        private void FinalizeMeasureBlock(BlockView block)
        {
            Debug.Assert(!IsPreparingMeasure);

            foreach (var blockItem in BlockItems)
            {
                var element = block[blockItem];
                element.Measure(GetBlockItemSize(block, blockItem));
            }

            if (block.Count > 0)
            {
                var size = GetBlockSize(block);
                for (int i = 0; i < block.Count; i++)
                    block[i].View.Measure(size);
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

            foreach (var rowItem in rowItems)
            {
                var element = row.Elements[rowItem.Ordinal];
                element.Measure(GetRowItemSize(row, rowItem));
            }
        }

        internal Rect GetScalarItemRect(ScalarItem scalarItem, int blockDimension)
        {
            var point = GetScalarItemLocation(scalarItem, blockDimension);
            var size = GetScalarItemSize(scalarItem);
            return new Rect(point, size);
        }

        internal abstract Thickness GetScalarItemClip(ScalarItem scalarItem, int blockDimension);

        protected abstract Point GetScalarItemLocation(ScalarItem scalarItem, int blockDimension);

        internal Size Arrange(Size finalSize)
        {
            foreach (var scalarItem in ScalarItems)
            {
                for (int i = 0; i < scalarItem.BlockDimensions; i++)
                {
                    var element = scalarItem[i];
                    var rect = GetScalarItemRect(scalarItem, i);
                    var clip = GetScalarItemClip(scalarItem, i);
                    Arrange(element, rect, clip);
                }
            }

            ArrangeBlocks();
            return finalSize;
        }

        private void Arrange(UIElement element, Rect rect, Thickness clip)
        {
            element.Arrange(rect);
            if (clip.Left == 0 && clip.Top == 0 && clip.Right == 0 && clip.Top == 0)
                element.Clip = null;
            else if (double.IsPositiveInfinity(clip.Left) || double.IsPositiveInfinity(clip.Top)
                || double.IsPositiveInfinity(clip.Right) || double.IsPositiveInfinity(clip.Bottom))
                element.Clip = Geometry.Empty;
            else
                element.Clip = new RectangleGeometry(new Rect(clip.Left, clip.Top, rect.Width - clip.Left - clip.Right, rect.Height - clip.Top - clip.Bottom));
        }

        internal Rect GetBlockRect(BlockView blockView)
        {
            var offset = GetBlockLocation(blockView);
            var size = GetBlockSize(blockView);
            return new Rect(offset, size);
        }

        protected abstract Point GetBlockLocation(BlockView block);

        internal abstract Thickness GetBlockClip(BlockView block);

        private void ArrangeBlocks()
        {
            for (int i = 0; i < BlockViews.Count; i++)
            {
                var block = BlockViews[i];
                var rect = GetBlockRect(block);
                var clip = GetBlockClip(block);
                Arrange(block, rect, clip);
            }
        }

        internal Rect GetBlockItemRect(BlockView block, BlockItem blockItem)
        {
            var location = GetBlockItemLocation(block, blockItem);
            var size = GetBlockItemSize(block, blockItem);
            return new Rect(location, size);
        }

        protected abstract Point GetBlockItemLocation(BlockView block, BlockItem blockItem);

        protected abstract Size GetBlockItemSize(BlockView block, BlockItem blockItem);

        internal abstract Thickness GetBlockItemClip(BlockView block, BlockItem blockItem);

        internal Rect GetRowRect(BlockView block, int blockDimension)
        {
            var location = GetRowLocation(block, blockDimension);
            var size = GetRowSize(block, blockDimension);
            return new Rect(location, size);
        }

        protected abstract Point GetRowLocation(BlockView block, int blockDimension);

        protected abstract Size GetRowSize(BlockView block, int blockDimension);

        internal abstract Thickness GetRowClip(int blockDimension);

        internal void ArrangeBlock(BlockView block)
        {
            foreach (var blockItem in BlockItems)
            {
                var element = block[blockItem];
                var rect = GetBlockItemRect(block, blockItem);
                var clip = GetBlockItemClip(block, blockItem);
                Arrange(element, rect, clip);
            }

            for (int i = 0; i < block.Count; i++)
            {
                var row = block[i];
                var rect = GetRowRect(block, i);
                var clip = GetRowClip(i);
                Arrange(row.View, rect, clip);
            }
        }

        internal Rect GetRowItemRect(RowPresenter row, RowItem rowItem)
        {
            var location = GetRowItemLocation(row, rowItem);
            var size = GetRowItemSize(row, rowItem);
            return new Rect(location, size);
        }

        protected abstract Point GetRowItemLocation(RowPresenter row, RowItem rowItem);

        protected abstract Size GetRowItemSize(RowPresenter row, RowItem rowItem);

        internal abstract Thickness GetRowItemClip(RowPresenter row, RowItem rowItem);

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
                var rect = GetRowItemRect(row, rowItem);
                var clip = GetRowItemClip(row, rowItem);
                Arrange(element, rect, clip);
            }
        }

        internal abstract IEnumerable<GridLineFigure> GridLineFigures { get; }
    }
}
