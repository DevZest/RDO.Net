using DevZest.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Windows.Data.Primitives
{
    internal abstract partial class LayoutManager : InputManager
    {
        internal static LayoutManager Create(DataPresenter dataPresenter, Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy)
        {
            var result = LayoutManager.Create(template, dataSet, where, orderBy);
            result.DataPresenter = dataPresenter;
            return result;
        }

        internal static LayoutManager Create(Template template, DataSet dataSet, _Boolean where = null, ColumnSort[] orderBy = null)
        {
            if (!template.Orientation.HasValue)
                return new LayoutZManager(template, dataSet, where, orderBy);
            else if (template.Orientation.GetValueOrDefault() == Orientation.Horizontal)
                return new LayoutXManager(template, dataSet, where, orderBy);
            else
                return new LayoutYManager(template, dataSet, where, orderBy);
        }

        protected LayoutManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy, bool emptyContainerViewList)
            : base(template, dataSet, where, orderBy, emptyContainerViewList)
        {
        }

        internal DataPresenter DataPresenter { get; private set; }

        protected RecapBindingCollection<ScalarBinding> ScalarBindings
        {
            get { return Template.InternalScalarBindings; }
        }

        private RecapBindingCollection<BlockBinding> BlockBindings
        {
            get { return Template.InternalBlockBindings; }
        }

        private void UpdateAutoSize(ContainerView containerView, Binding binding, Size measuredSize)
        {
            Debug.Assert(binding.IsAutoSize);

            var gridRange = binding.GridRange;
            if (binding.AutoWidthGridColumns.Count > 0)
            {
                double totalAutoWidth = measuredSize.Width - gridRange.GetMeasuredWidth(x => !x.IsAutoLength);
                if (totalAutoWidth > 0)
                {
                    DistributeAutoLength(containerView, binding.AutoWidthGridColumns, totalAutoWidth);
                    Template.DistributeStarWidths();
                }
            }

            if (binding.AutoHeightGridRows.Count > 0)
            {
                double totalAutoHeight = measuredSize.Height - gridRange.GetMeasuredHeight(x => !x.IsAutoLength);
                if (totalAutoHeight > 0)
                {
                    DistributeAutoLength(containerView, binding.AutoHeightGridRows, totalAutoHeight);
                    Template.DistributeStarHeights();
                }
            }
        }

        private void DistributeAutoLength<T>(ContainerView containerView, IReadOnlyList<T> autoLengthTracks, double totalMeasuredLength)
            where T : GridTrack
        {
            Debug.Assert(autoLengthTracks.Count > 0);
            Debug.Assert(totalMeasuredLength > 0);

            if (autoLengthTracks.Count == 1)
            {
                var track = autoLengthTracks[0];
                if (totalMeasuredLength > GetMeasuredLength(containerView, track))
                    SetMeasuredAutoLength(containerView, track, totalMeasuredLength);
            }
            else
                DistributeOrderedAutoLength(containerView, autoLengthTracks.OrderByDescending(x => x.MeasuredLength).ToArray(), totalMeasuredLength);
        }

        private void DistributeOrderedAutoLength<T>(ContainerView containerView, IReadOnlyList<T> orderedAutoLengthTracks, double totalMeasuredLength)
            where T : GridTrack
        {
            Debug.Assert(orderedAutoLengthTracks.Count > 0);
            Debug.Assert(totalMeasuredLength > 0);

            var count = orderedAutoLengthTracks.Count;
            double avgLength = totalMeasuredLength / count;
            for (int i = 0; i < count; i++)
            {
                var track = orderedAutoLengthTracks[i];
                var trackMeasuredLength = GetMeasuredLength(containerView, track);
                if (trackMeasuredLength >= avgLength)
                {
                    totalMeasuredLength -= trackMeasuredLength;
                    avgLength = totalMeasuredLength / (count - i + 1);
                }
                else
                    SetMeasuredAutoLength(containerView, track, avgLength);
            }
        }

        protected virtual double GetMeasuredLength(ContainerView containerView, GridTrack gridTrack)
        {
            return gridTrack.MeasuredLength;
        }

        protected virtual void SetMeasuredAutoLength(ContainerView containerView, GridTrack gridTrack, double value)
        {
            var delta = value - gridTrack.MeasuredLength;
            Debug.Assert(delta > 0);
            gridTrack.MeasuredLength = value;
            gridTrack.Owner.TotalAutoLength += delta;
        }

        protected abstract Size GetSize(ScalarBinding scalarBinding);

        internal Size Measure(Size availableSize)
        {
            Template.InitMeasure(availableSize);
            FlowCount = Template.CoerceFlowCount();
            PrepareMeasure();
            var result = FinalizeMeasure();
            return result;
        }

        private bool IsPreparingMeasure;

        protected virtual void PrepareMeasure()
        {
            IsPreparingMeasure = true;
            PrepareMeasure(ScalarBindings.PreAutoSizeBindings);
            PrepareMeasureContainers();
            PrepareMeasure(ScalarBindings.PostAutoSizeBindings);
            IsPreparingMeasure = false;
        }

        private void PrepareMeasure(IEnumerable<ScalarBinding> scalarBindings)
        {
            foreach (var scalarBinding in scalarBindings)
            {
                Debug.Assert(scalarBinding.FlowCount == 1, "Auto size is not allowed with flowable ScalarBinding.");
                var element = scalarBinding[0];
                element.Measure(scalarBinding.AvailableAutoSize);
                UpdateAutoSize(null, scalarBinding, element.DesiredSize);
            }
        }

        protected abstract void PrepareMeasureContainers();

        private Size FinalizeMeasure()
        {
            foreach (var scalarBinding in ScalarBindings)
            {
                for (int i = 0; i < scalarBinding.FlowCount; i++)
                {
                    var element = scalarBinding[i];
                    element.Measure(GetSize(scalarBinding));
                }
            }

            if (IsCurrentContainerViewIsolated)
                CurrentContainerView.Measure(GetSize(CurrentContainerView));

            for (int i = 0; i < ContainerViewList.Count; i++)
            {
                var containerView = ContainerViewList[i];
                containerView.Measure(GetSize(containerView));
            }

            ClearCachedElements();
            return MeasuredSize;
        }

        protected abstract Size GetSize(ContainerView containerView);

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

            var containerView = this[rowView];
            foreach (var rowBinding in rowBindings.AutoSizeItems)
            {
                var element = rowView.Elements[rowBinding.Ordinal];
                element.Measure(rowBinding.AvailableAutoSize);
                UpdateAutoSize(containerView, rowBinding, element.DesiredSize);
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

        internal Rect GetRect(ScalarBinding scalarBinding, int flowIndex)
        {
            var point = GetPosition(scalarBinding, flowIndex);
            var size = GetSize(scalarBinding);
            return new Rect(point, size);
        }

        internal abstract Thickness GetClip(ScalarBinding scalarBinding, int flowIndex);

        protected abstract Point GetPosition(ScalarBinding scalarBinding, int flowIndex);

        internal Size Arrange(Size finalSize)
        {
            foreach (var scalarBinding in ScalarBindings)
            {
                for (int i = 0; i < scalarBinding.FlowCount; i++)
                {
                    var element = scalarBinding[i];
                    var rect = GetRect(scalarBinding, i);
                    var clip = GetClip(scalarBinding, i);
                    Arrange(element, rect, clip);
                }
            }

            ArrangeContainerViews();
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

        internal Rect GetRect(ContainerView containerView)
        {
            var offset = GetPosition(containerView);
            var size = GetSize(containerView);
            return new Rect(offset, size);
        }

        protected abstract Point GetPosition(ContainerView containerView);

        internal abstract Thickness GetClip(ContainerView containerView);

        private void ArrangeContainerViews()
        {
            if (IsCurrentContainerViewIsolated)
                Arrange(CurrentContainerView);

            for (int i = 0; i < ContainerViewList.Count; i++)
                Arrange(ContainerViewList[i]);
        }

        private void Arrange(ContainerView containerView)
        {
            var rect = GetRect(containerView);
            var clip = GetClip(containerView);
            Arrange(containerView, rect, clip);
        }

        internal Rect GetRect(BlockView blockView, BlockBinding blockBinding)
        {
            var position = GetPosition(blockView, blockBinding);
            var size = GetSize(blockView, blockBinding);
            return new Rect(position, size);
        }

        protected abstract Point GetPosition(BlockView blockView, BlockBinding blockBinding);

        protected abstract Size GetSize(BlockView blockView, BlockBinding blockBinding);

        internal abstract Thickness GetClip(BlockView blockView, BlockBinding blockBinding);

        internal Rect GetRect(BlockView block, int flowIndex)
        {
            var position = GetPosition(block, flowIndex);
            var size = GetSize(block, flowIndex);
            return new Rect(position, size);
        }

        protected abstract Point GetPosition(BlockView block, int flowIndex);

        protected abstract Size GetSize(BlockView blockView, int flowIndex);

        internal abstract Thickness GetClip(int flowIndex);

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
            var position = GetPosition(rowView, rowBinding);
            var size = GetSize(rowView, rowBinding);
            return new Rect(position, size);
        }

        protected abstract Point GetPosition(RowView rowView, RowBinding rowBinding);

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

        public virtual void SetCurrentRow(RowPresenter value, SelectionMode? selectionMode, bool ensureVisible)
        {
            var oldValue = CurrentRow;
            if (oldValue == value)
                return;

            throw new NotImplementedException();
            //SetCurrentRow(value, selectionMode);
        }
    }
}
