using DevZest.Data.Presenters.Primitives;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DevZest.Data.Presenters;
using System.Windows.Input;
using DevZest.Windows;
using System.Diagnostics;
using System;

namespace DevZest.Data.Views
{
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateNormal)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateMouseOver)]
    [TemplateVisualState(GroupName = VisualStates.GroupSelection, Name = VisualStates.StateSelected)]
    [TemplateVisualState(GroupName = VisualStates.GroupSelection, Name = VisualStates.StateUnselected)]
    [TemplateVisualState(GroupName = VisualStates.GroupRowIndicator, Name = VisualStates.StateRegularRow)]
    [TemplateVisualState(GroupName = VisualStates.GroupRowIndicator, Name = VisualStates.StateCurrentRow)]
    [TemplateVisualState(GroupName = VisualStates.GroupRowIndicator, Name = VisualStates.StateCurrentEditingRow)]
    [TemplateVisualState(GroupName = VisualStates.GroupRowIndicator, Name = VisualStates.StateNewRow)]
    [TemplateVisualState(GroupName = VisualStates.GroupRowIndicator, Name = VisualStates.StateNewCurrentRow)]
    [TemplateVisualState(GroupName = VisualStates.GroupRowIndicator, Name = VisualStates.StateNewEditingRow)]
    public class RowHeader : ButtonBase, IRowElement, RowSelectionWiper.ISelector
    {
        public abstract class Styles
        {
            public static readonly StyleId Flat = new StyleId(typeof(RowHeader));
        }

        private static readonly DependencyPropertyKey IsSelectedPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsSelected), typeof(bool),
            typeof(RowHeader), new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;

        public static readonly DependencyProperty SeparatorBrushProperty = DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush),
            typeof(RowHeader), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SeparatorVisibilityProperty = DependencyProperty.Register(nameof(SeparatorVisibility), typeof(Visibility),
            typeof(RowHeader), new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty ResizeGripperVisibilityProperty = DependencyProperty.Register(nameof(ResizeGripperVisibility), typeof(Visibility),
            typeof(RowHeader), new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty IsResizeGripperProperty = DependencyProperty.RegisterAttached("IsResizeGripper", typeof(bool),
            typeof(RowHeader), new FrameworkPropertyMetadata(BooleanBoxes.False));

        static RowHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowHeader), new FrameworkPropertyMetadata(typeof(RowHeader)));
        }

        public RowHeader()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var rowPresenter = RowView.GetCurrent(this).RowPresenter;
            UpdateVisualStates(rowPresenter);
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            private set { SetValue(IsSelectedPropertyKey, BooleanBoxes.Box(value)); }
        }

        public Brush SeparatorBrush
        {
            get { return (Brush)GetValue(SeparatorBrushProperty); }
            set { SetValue(SeparatorBrushProperty, value); }
        }

        public Visibility SeparatorVisibility
        {
            get { return (Visibility)GetValue(SeparatorVisibilityProperty); }
            set { SetValue(SeparatorVisibilityProperty, value); }
        }

        public Visibility ResizeGripperVisibility
        {
            get { return (Visibility)GetValue(ResizeGripperVisibilityProperty); }
            set { SetValue(ResizeGripperVisibilityProperty, value); }
        }

        public static bool GetIsResizeGripper(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsResizeGripperProperty);
        }

        public static void SetIsResizeGripper(DependencyObject obj, bool value)
        {
            obj.SetValue(IsResizeGripperProperty, BooleanBoxes.Box(value));
        }

        void IRowElement.Setup(RowPresenter p)
        {
            var dataPresenter = p.DataPresenter;
            RowSelectionWiper.EnsureSetup(dataPresenter);
        }

        void IRowElement.Refresh(RowPresenter p)
        {
            UpdateVisualStates(p);
        }

        void IRowElement.Cleanup(RowPresenter p)
        {
        }

        private void UpdateVisualStates(RowPresenter p)
        {
            UpdateVisualStates(p, true);
        }

        private void UpdateVisualStates(RowPresenter p, bool useTransitions)
        {
            if (!IsLoaded)
                return;

            if (p.IsSelected)
            {
                IsSelected = true;
                VisualStates.GoToState(this, useTransitions, VisualStates.StateSelected);
            }
            else
            {
                IsSelected = false;
                VisualStates.GoToState(this, useTransitions, VisualStates.StateUnselected);
            }

            if (p.IsVirtual)
            {
                if (p.IsEditing)
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateNewEditingRow);
                else if (p.IsCurrent)
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateNewCurrentRow);
                else
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateNewRow);
            }
            else if (p.IsCurrent)
            {
                if (p.IsEditing)
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateCurrentEditingRow);
                else
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateCurrentRow);
            }
            else
                VisualStates.GoToState(this, useTransitions, VisualStates.StateRegularRow);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled)
                HandleMouseButtonDown(MouseButton.Left);
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled)
                HandleMouseButtonDown(MouseButton.Right);
            base.OnMouseRightButtonDown(e);
        }

        private RowPresenter RowPresenter
        {
            get { return this.GetRowPresenter(); }
        }

        private DataPresenter DataPresenter
        {
            get { return RowPresenter?.DataPresenter; }
        }

        private void HandleMouseButtonDown(MouseButton mouseButton)
        {
            var dataPresenter = DataPresenter;
            if (dataPresenter == null)
                return;

            dataPresenter.Select(RowPresenter, mouseButton, () =>
            {
                if (!IsKeyboardFocusWithin)
                    Focus();
            });
        }

        private GridTrack GridTrackToResize
        {
            get { return this.GetBinding()?.GridRange.RowSpan.EndTrack; }
        }

        private sealed class DragHandler : DragHandlerBase
        {
            private GridTrack _gridTrack;
            private GridLength _oldValue;
            private RowPresenter _rowPresenter;
            private double _resized;

            public bool BeginDrag(RowHeader rowHeader, UIElement resizeGripper, MouseEventArgs e)
            {
                Debug.Assert(rowHeader != null);
                Debug.Assert(resizeGripper != null);
                _gridTrack = rowHeader.GridTrackToResize;
                if (_gridTrack == null)
                    return false;
                _oldValue = _gridTrack.Length;
                _rowPresenter = rowHeader.RowPresenter;
                DragDetect(resizeGripper, e);
                return true;
            }

            protected override void OnBeginDrag()
            {
                _resized = _rowPresenter.GetMeasuredLength(_gridTrack).Value;
            }

            private double MinLength
            {
                get { return _gridTrack.MinLength; }
            }

            private double MaxLength
            {
                get { return _gridTrack.MaxLength; }
            }

            protected override void OnDragDelta()
            {
                var newValue = _resized = _resized + MouseDeltaY;
                newValue = Math.Max(newValue, MinLength);
                newValue = Math.Min(newValue, MaxLength);
                _resized += newValue - _resized;
                var length = new GridLength(newValue, GridUnitType.Pixel);
                ResizeTo(length);
            }

            private void ResizeTo(GridLength length)
            {
                _rowPresenter.Resize(_gridTrack, length);
            }

            protected override void OnEndDrag(UIElement dragElement, bool abort)
            {
                if (abort)
                    ResizeTo(_oldValue);

                if (IsShiftKeyDown)
                    _gridTrack.Resize(_rowPresenter.GetLength(_gridTrack));
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (e.Handled)
                return;

            var resizeGripper = e.OriginalSource as UIElement;
            if (resizeGripper == null || !GetIsResizeGripper(resizeGripper))
                return;

            if (e.ClickCount == 1)
                e.Handled = new DragHandler().BeginDrag(this, resizeGripper, e);
            else if (e.ClickCount == 2)
                e.Handled = AutoResize();
        }

        private static bool IsShiftKeyDown
        {
            get { return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift); }
        }

        private bool AutoResize()
        {
            var isShiftDown = IsShiftKeyDown;

            if (isShiftDown)
                GridTrackToResize.Resize(GridLength.Auto);
            else
                RowPresenter.Resize(GridTrackToResize, GridLength.Auto);
            return true;
        }
    }
}
