using DevZest.Data.Views;
using DevZest.Data.Views.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Presenters.Primitives
{
    internal abstract partial class LayoutManager : InputManager
    {
        internal static LayoutManager Create(LayoutManager inherit, DataPresenter dataPresenter, Template template, DataSet dataSet, IReadOnlyList<Column> rowMatchColumns, Predicate<DataRow> where, IComparer<DataRow> orderBy)
        {
            var result = Create(inherit, template, dataSet, rowMatchColumns, where, orderBy);
            result._dataPresenter = dataPresenter;
            return result;
        }

        internal static LayoutManager Create(LayoutManager inherit, Template template, DataSet dataSet, IReadOnlyList<Column> rowMatchColumns, Predicate<DataRow> where = null, IComparer<DataRow> orderBy = null)
        {
            if (!template.Orientation.HasValue)
                return new LayoutZManager(inherit, template, dataSet, rowMatchColumns, where, orderBy);
            else if (template.Orientation.GetValueOrDefault() == Orientation.Horizontal)
                return new LayoutXManager(inherit as ScrollableManager, template, dataSet, rowMatchColumns, where, orderBy);
            else
                return new LayoutYManager(inherit as ScrollableManager, template, dataSet, rowMatchColumns, where, orderBy);
        }

        protected LayoutManager(LayoutManager inherit, Template template, DataSet dataSet, IReadOnlyList<Column> rowMatchColumns, Predicate<DataRow> where, IComparer<DataRow> orderBy, bool emptyContainerViewList)
            : base(inherit, template, dataSet, rowMatchColumns, where, orderBy, emptyContainerViewList)
        {
        }

        private DataPresenter _dataPresenter;
        internal override DataPresenter DataPresenter
        {
            get { return _dataPresenter; }
        }

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
                DistributeOrderedAutoLength(containerView, autoLengthTracks.OrderBy(x => x.MaxLength - GetMeasuredLength(containerView, x)).ToArray(), totalMeasuredLength);
        }

        private void DistributeOrderedAutoLength<T>(ContainerView containerView, IReadOnlyList<T> orderedAutoLengthTracks, double totalMeasuredLength)
            where T : GridTrack
        {
            Debug.Assert(orderedAutoLengthTracks.Count > 0);
            Debug.Assert(totalMeasuredLength > 0);

            var count = orderedAutoLengthTracks.Count;
            for (int i = 0; i < count; i++)
            {
                double avgLength = totalMeasuredLength / (count - i);
                var track = orderedAutoLengthTracks[i];
                var trackMeasuredLength = GetMeasuredLength(containerView, track);
                if (trackMeasuredLength < avgLength)
                {
                    var delta = SetMeasuredAutoLength(containerView, track, avgLength);
                    totalMeasuredLength -= delta;
                }
            }
        }

        protected virtual double GetMeasuredLength(ContainerView containerView, GridTrack gridTrack)
        {
            return gridTrack.MeasuredLength;
        }

        protected virtual double SetMeasuredAutoLength(ContainerView containerView, GridTrack gridTrack, double value)
        {
            Debug.Assert(value - gridTrack.MeasuredLength > 0);
            var delta = gridTrack.SetMeasuredLength(value);
            gridTrack.Owner.TotalAutoLength += delta;
            return delta;
        }

        protected abstract Size GetSize(ScalarBinding scalarBinding);

        internal Size Measure(Size availableSize)
        {
            IsMeasuring = true;
            Template.InitMeasure(availableSize);
            FlowRepeatCount = Template.CoerceFlowRepeatCount();
            PrepareMeasure();
            var result = FinalizeMeasure();
            IsMeasuring = false;
            return result;
        }

        internal bool IsMeasuring { get; private set; }
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
                for (int i = 0; i < scalarBinding.FlowRepeatCount; i++)
                {
                    var element = scalarBinding[i];
                    element.Measure(scalarBinding.AvailableAutoSize);
                    UpdateAutoSize(null, scalarBinding, element.DesiredSize);
                }
            }
        }

        protected abstract void PrepareMeasureContainers();

        private Size FinalizeMeasure()
        {
            foreach (var scalarBinding in ScalarBindings)
            {
                if (scalarBinding.IsAutoSize)
                    continue;
                for (int i = 0; i < scalarBinding.FlowRepeatCount; i++)
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

            return MeasuredSize;
        }

        protected abstract Size GetSize(ContainerView containerView);

        protected abstract Size MeasuredSize { get; }

        internal Size Measure(BlockView blockView, Size constraintSize)
        {
            Debug.Assert(IsMeasuring);
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
                if (blockBinding.IsAutoSize)
                    continue;
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
            Debug.Assert(IsMeasuring);
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
                if (rowBinding.IsAutoSize)
                    continue;
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

        internal abstract Thickness GetFrozenClip(ScalarBinding scalarBinding, int flowIndex);

        protected abstract Point GetPosition(ScalarBinding scalarBinding, int flowIndex);

        internal Size Arrange(Size finalSize)
        {
            foreach (var scalarBinding in ScalarBindings)
            {
                for (int i = 0; i < scalarBinding.FlowRepeatCount; i++)
                {
                    var element = scalarBinding[i];
                    var rect = GetRect(scalarBinding, i);
                    var clip = GetFrozenClip(scalarBinding, i);
                    Arrange(element, scalarBinding, rect, clip);
                }
            }

            ArrangeContainerViews();

            Template.InitFocus();
            return finalSize;
        }

        private void Arrange(UIElement element, Rect rect, Geometry clip)
        {
            element.Arrange(rect);
            element.Clip = clip;
        }

        private void Arrange(UIElement element, Rect rect, Thickness frozenClip)
        {
            if (frozenClip.Left == 0 && frozenClip.Top == 0 && frozenClip.Right == 0 && frozenClip.Bottom == 0)
                Arrange(element, rect, null);
            else if (double.IsPositiveInfinity(frozenClip.Left) || double.IsPositiveInfinity(frozenClip.Top)
                || double.IsPositiveInfinity(frozenClip.Right) || double.IsPositiveInfinity(frozenClip.Bottom))
                Arrange(element, rect, Geometry.Empty);
            else
                ArrangeWithClip(element, rect, frozenClip);
        }

        private void ArrangeWithClip(UIElement element, Rect rect, Thickness frozenClip)
        {
            element.Arrange(rect);
            var renderSize = element.RenderSize;
            var clipWidth = Math.Max(0, renderSize.Width - frozenClip.Left - frozenClip.Right);
            var clipHeight = Math.Max(0, renderSize.Height - frozenClip.Top - frozenClip.Bottom);
            element.Clip = new RectangleGeometry(new Rect(frozenClip.Left, frozenClip.Top, clipWidth, clipHeight));
        }

        private void Arrange(UIElement element, Binding binding, Rect rect, Thickness frozenClip)
        {
            var left = frozenClip.Left;
            if (binding.FrozenLeftShrink && left > 0 && !double.IsPositiveInfinity(left))
            {
                rect = new Rect(rect.Left + frozenClip.Left, rect.Top, Math.Max(0, rect.Width - frozenClip.Left), rect.Height);
                frozenClip = new Thickness(0, frozenClip.Top, frozenClip.Right, frozenClip.Bottom);
            }

            var top = frozenClip.Top;
            if (binding.FrozenTopShrink && top > 0 && !double.IsPositiveInfinity(top))
            {
                rect = new Rect(rect.Left, rect.Top + frozenClip.Top, rect.Width, Math.Max(0, rect.Height - frozenClip.Top));
                frozenClip = new Thickness(frozenClip.Left, 0, frozenClip.Right, frozenClip.Bottom);
            }

            var right = frozenClip.Right;
            if (binding.FrozenRightShrink && right > 0 && !double.IsPositiveInfinity(right))
            {
                rect = new Rect(rect.Left, rect.Top + frozenClip.Top, Math.Max(0, rect.Width - frozenClip.Right), rect.Height);
                frozenClip = new Thickness(frozenClip.Left, frozenClip.Top, 0, frozenClip.Bottom);
            }

            var bottom = frozenClip.Bottom;
            if (binding.FrozenBottomShrink && bottom > 0 && !double.IsPositiveInfinity(bottom))
            {
                rect = new Rect(rect.Left, rect.Top + frozenClip.Top, rect.Width, Math.Max(0, rect.Height - frozenClip.Bottom));
                frozenClip = new Thickness(frozenClip.Left, frozenClip.Top, frozenClip.Right, 0);
            }

            Arrange(element, rect, frozenClip);
        }

        internal Rect GetRect(ContainerView containerView)
        {
            var offset = GetPosition(containerView);
            var size = GetSize(containerView);
            return new Rect(offset, size);
        }

        protected abstract Point GetPosition(ContainerView containerView);

        internal abstract Thickness GetFrozenClip(ContainerView containerView);

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
            var clip = GetFrozenClip(containerView);
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

        internal abstract Thickness GetFrozenClip(BlockView blockView, BlockBinding blockBinding);

        internal Rect GetRect(BlockView block, int flowIndex)
        {
            var position = GetPosition(block, flowIndex);
            var size = GetSize(block, flowIndex);
            return new Rect(position, size);
        }

        protected abstract Point GetPosition(BlockView block, int flowIndex);

        protected abstract Size GetSize(BlockView blockView, int flowIndex);

        internal abstract Thickness GetFrozenClip(int flowIndex);

        internal void ArrangeChildren(BlockView blockView)
        {
            foreach (var blockBinding in BlockBindings)
            {
                var element = blockView[blockBinding];
                var rect = GetRect(blockView, blockBinding);
                var clip = GetFrozenClip(blockView, blockBinding);
                Arrange(element, blockBinding, rect, clip);
            }

            for (int i = 0; i < blockView.Count; i++)
            {
                var row = blockView[i];
                var rect = GetRect(blockView, i);
                var clip = GetFrozenClip(i);
                row.View.Arrange(rect);
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

        internal abstract Thickness GetFrozenClip(RowView rowView, RowBinding rowBinding);

        internal void ArrangeChildren(RowView rowView)
        {
            var rowBindings = rowView.RowBindings;
            if (rowBindings.Count == 0)
                return;

            foreach (var rowBinding in rowBindings)
            {
                var element = rowView.Elements[rowBinding.Ordinal];
                var rect = GetRect(rowView, rowBinding);
                var clip = GetFrozenClip(rowView, rowBinding);
                Arrange(element, rowBinding, rect, clip);
            }
        }

        internal abstract IEnumerable<GridLineFigure> GridLineFigures { get; }

        public FrameworkElement Panel
        {
            get { return ElementCollection?.Parent; }
        }

        public void InvalidateMeasure()
        {
            if (Panel != null)
                Panel.InvalidateMeasure();
        }
    }
}
