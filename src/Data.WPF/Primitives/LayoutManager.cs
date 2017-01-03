using System;
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
        internal static LayoutManager Create(DataPresenter dataPresenter, Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy, Func<IEnumerable<ValidationMessage<Scalar>>> validateScalars)
        {
            var result = LayoutManager.Create(template, dataSet, where, orderBy, validateScalars);
            result.DataPresenter = dataPresenter;
            return result;
        }

        internal static LayoutManager Create(Template template, DataSet dataSet, _Boolean where = null, ColumnSort[] orderBy = null, Func<IEnumerable<ValidationMessage<Scalar>>> validateScalars = null)
        {
            if (!template.Orientation.HasValue)
                return new LayoutZManager(template, dataSet, where, orderBy, validateScalars);
            else if (template.Orientation.GetValueOrDefault() == Orientation.Horizontal)
                return new LayoutXManager(template, dataSet, where, orderBy, validateScalars);
            else
                return new LayoutYManager(template, dataSet, where, orderBy, validateScalars);
        }

        protected LayoutManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy, Func<IEnumerable<ValidationMessage<Scalar>>> validateScalars, bool emptyBlockViewList)
            : base(template, dataSet, where, orderBy, validateScalars, emptyBlockViewList)
        {
        }

        internal DataPresenter DataPresenter { get; private set; }

        private RecapBindingCollection<ScalarBinding> ScalarBindings
        {
            get { return Template.InternalScalarBindings; }
        }

        private RecapBindingCollection<BlockBinding> BlockBindings
        {
            get { return Template.InternalBlockBindings; }
        }

        private void UpdateAutoSize(BlockView blockView, Binding binding, Size measuredSize)
        {
            Debug.Assert(binding.IsAutoSize);

            var gridRange = binding.GridRange;
            if (binding.AutoWidthGridColumns.Count > 0)
            {
                double totalAutoWidth = measuredSize.Width - gridRange.GetMeasuredWidth(x => !x.IsAutoLength);
                if (totalAutoWidth > 0)
                {
                    DistributeAutoLength(blockView, binding.AutoWidthGridColumns, totalAutoWidth);
                    Template.DistributeStarWidths();
                }
            }

            if (binding.AutoHeightGridRows.Count > 0)
            {
                double totalAutoHeight = measuredSize.Height - gridRange.GetMeasuredHeight(x => !x.IsAutoLength);
                if (totalAutoHeight > 0)
                {
                    DistributeAutoLength(blockView, binding.AutoHeightGridRows, totalAutoHeight);
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

        private bool IsVariantLength(BlockView blockView, GridTrack gridTrack)
        {
            return blockView != null && gridTrack.VariantByBlock;
        }

        protected double GetMeasuredLength(BlockView blockView, GridTrack gridTrack)
        {
            return IsVariantLength(blockView, gridTrack) ? blockView.GetMeasuredLength(gridTrack) : gridTrack.MeasuredLength;
        }

        private void SetMeasuredAutoLength(BlockView blockView, GridTrack gridTrack, double value)
        {
            if (IsVariantLength(blockView, gridTrack))
                blockView.SetMeasuredLength(gridTrack, value);
            else
            {
                var delta = value - gridTrack.MeasuredLength;
                Debug.Assert(delta > 0);
                gridTrack.MeasuredLength = value;
                gridTrack.Owner.TotalAutoLength += delta;
            }
        }

        protected abstract Size GetSize(ScalarBinding scalarBinding);

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
            PrepareMeasure(ScalarBindings.PreAutoSizeBindings);
            PrepareMeasureBlocks();
            PrepareMeasure(ScalarBindings.PostAutoSizeBindings);
            IsPreparingMeasure = false;
        }

        private void PrepareMeasure(IEnumerable<ScalarBinding> scalarBindings)
        {
            foreach (var scalarBinding in scalarBindings)
            {
                Debug.Assert(scalarBinding.BlockDimensions == 1, "Auto size is not allowed with multidimensional ScalarBinding.");
                var element = scalarBinding[0];
                element.Measure(scalarBinding.AvailableAutoSize);
                UpdateAutoSize(null, scalarBinding, element.DesiredSize);
            }
        }

        protected abstract void PrepareMeasureBlocks();

        private Size FinalizeMeasure()
        {
            foreach (var scalarBinding in ScalarBindings)
            {
                for (int i = 0; i < scalarBinding.BlockDimensions; i++)
                {
                    var element = scalarBinding[i];
                    element.Measure(GetSize(scalarBinding));
                }
            }

            if (IsCurrentBlockViewIsolated)
                CurrentBlockView.Measure(GetSize(CurrentBlockView));

            for (int i = 0; i < BlockViewList.Count; i++)
            {
                var block = BlockViewList[i];
                block.Measure(GetSize(block));
            }

            return MeasuredSize;
        }

        protected abstract Size GetSize(BlockView blockView);

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

            PrepareMeasure(blockView, BlockBindings.PreAutoSizeBindings);

            for (int i = 0; i < blockView.Count; i++)
                blockView[i].View.Measure(Size.Empty);

            PrepareMeasure(blockView, BlockBindings.PostAutoSizeBindings);
        }

        private void PrepareMeasure(BlockView blockView, IEnumerable<BlockBinding> blockBindings)
        {
            foreach (var blockBinding in blockBindings)
            {
                Debug.Assert(blockBinding.IsAutoSize);
                var element = blockView[blockBinding];
                element.Measure(blockBinding.AvailableAutoSize);
                UpdateAutoSize(blockView, blockBinding, element.DesiredSize);
            }
        }

        private void FinalizeMeasure(BlockView blockView)
        {
            Debug.Assert(!IsPreparingMeasure);

            foreach (var blockBinding in BlockBindings)
            {
                var element = blockView[blockBinding];
                element.Measure(GetSize(blockView, blockBinding));
            }

            if (blockView.Count > 0)
            {
                for (int i = 0; i < blockView.Count; i++)
                {
                    var size = GetSize(blockView, i);
                    blockView[i].View.Measure(size);
                }
            }
        }

        internal Size Measure(RowView rowView, Size constraintSize)
        {
            if (IsPreparingMeasure)
                PrepareMeasure(rowView);
            else
                FinalizeMeasure(rowView);
            return constraintSize;
        }

        private void PrepareMeasure(RowView rowView)
        {
            var rowBindings = rowView.RowBindings;
            if (rowBindings.AutoSizeItems.Count == 0)
                return;

            var blockView = this[rowView];
            foreach (var rowBinding in rowBindings.AutoSizeItems)
            {
                var element = rowView.Elements[rowBinding.Ordinal];
                element.Measure(rowBinding.AvailableAutoSize);
                UpdateAutoSize(blockView, rowBinding, element.DesiredSize);
            }
        }

        private void FinalizeMeasure(RowView rowView)
        {
            var rowBindings = rowView.RowBindings;
            if (rowBindings.Count == 0)
                return;

            foreach (var rowBinding in rowBindings)
            {
                var element = rowView.Elements[rowBinding.Ordinal];
                element.Measure(GetSize(rowView, rowBinding));
            }
        }

        internal Rect GetRect(ScalarBinding scalarBinding, int blockDimension)
        {
            var point = GetLocation(scalarBinding, blockDimension);
            var size = GetSize(scalarBinding);
            return new Rect(point, size);
        }

        internal abstract Thickness GetClip(ScalarBinding scalarBinding, int blockDimension);

        protected abstract Point GetLocation(ScalarBinding scalarBinding, int blockDimension);

        internal Size Arrange(Size finalSize)
        {
            foreach (var scalarBinding in ScalarBindings)
            {
                for (int i = 0; i < scalarBinding.BlockDimensions; i++)
                {
                    var element = scalarBinding[i];
                    var rect = GetRect(scalarBinding, i);
                    var clip = GetClip(scalarBinding, i);
                    Arrange(element, rect, clip);
                }
            }

            ArrangeBlocks();
            return finalSize;
        }

        private void Arrange(UIElement element, Rect rect, Thickness clip)
        {
            element.Arrange(rect);
            if (clip.Left == 0 && clip.Top == 0 && clip.Right == 0 && clip.Bottom == 0)
                element.Clip = null;
            else if (double.IsPositiveInfinity(clip.Left) || double.IsPositiveInfinity(clip.Top)
                || double.IsPositiveInfinity(clip.Right) || double.IsPositiveInfinity(clip.Bottom))
                element.Clip = Geometry.Empty;
            else
                element.Clip = new RectangleGeometry(new Rect(clip.Left, clip.Top, rect.Width - clip.Left - clip.Right, rect.Height - clip.Top - clip.Bottom));
        }

        internal Rect GetRect(BlockView blockView)
        {
            var offset = GetLocation(blockView);
            var size = GetSize(blockView);
            return new Rect(offset, size);
        }

        protected abstract Point GetLocation(BlockView blockView);

        internal abstract Thickness GetClip(BlockView blockView);

        private void ArrangeBlocks()
        {
            if (IsCurrentBlockViewIsolated)
                Arrange(CurrentBlockView);

            for (int i = 0; i < BlockViewList.Count; i++)
                Arrange(BlockViewList[i]);
        }

        private void Arrange(BlockView blockView)
        {
            var rect = GetRect(blockView);
            var clip = GetClip(blockView);
            Arrange(blockView, rect, clip);
        }

        internal Rect GetRect(BlockView blockView, BlockBinding blockBinding)
        {
            var location = GetLocation(blockView, blockBinding);
            var size = GetSize(blockView, blockBinding);
            return new Rect(location, size);
        }

        protected abstract Point GetLocation(BlockView blockView, BlockBinding blockBinding);

        protected abstract Size GetSize(BlockView blockView, BlockBinding blockBinding);

        internal abstract Thickness GetClip(BlockView blockView, BlockBinding blockBinding);

        internal Rect GetRect(BlockView block, int blockDimension)
        {
            var location = GetLocation(block, blockDimension);
            var size = GetSize(block, blockDimension);
            return new Rect(location, size);
        }

        protected abstract Point GetLocation(BlockView block, int blockDimension);

        protected abstract Size GetSize(BlockView blockView, int blockDimension);

        internal abstract Thickness GetClip(int blockDimension);

        internal void ArrangeChildren(BlockView blockView)
        {
            foreach (var blockBinding in BlockBindings)
            {
                var element = blockView[blockBinding];
                var rect = GetRect(blockView, blockBinding);
                var clip = GetClip(blockView, blockBinding);
                Arrange(element, rect, clip);
            }

            for (int i = 0; i < blockView.Count; i++)
            {
                var row = blockView[i];
                var rect = GetRect(blockView, i);
                var clip = GetClip(i);
                Arrange(row.View, rect, clip);
            }
        }

        internal Rect GetRect(RowView rowView, RowBinding rowBinding)
        {
            var location = GetLocation(rowView, rowBinding);
            var size = GetSize(rowView, rowBinding);
            return new Rect(location, size);
        }

        protected abstract Point GetLocation(RowView rowView, RowBinding rowBinding);

        protected abstract Size GetSize(RowView rowView, RowBinding rowBinding);

        internal abstract Thickness GetClip(RowView rowView, RowBinding rowBinding);

        internal void ArrangeChildren(RowView rowView)
        {
            var rowBindings = rowView.RowBindings;
            if (rowBindings.Count == 0)
                return;

            foreach (var rowBinding in rowBindings)
            {
                var element = rowView.Elements[rowBinding.Ordinal];
                var rect = GetRect(rowView, rowBinding);
                var clip = GetClip(rowView, rowBinding);
                Arrange(element, rect, clip);
            }
        }

        internal abstract IEnumerable<GridLineFigure> GridLineFigures { get; }
    }
}
