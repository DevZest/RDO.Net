using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DevZest.Data.Views
{
    public class ValidationPlaceholder : Control, IRowElement, IScalarElement
    {
        private sealed class MouseTracker
        {
            public MouseTracker(BasePresenter presenter)
            {
                _presenter = presenter;
                View = (UIElement)presenter.View;
                _presenter.ViewChanged += OnViewChanged;
            }

            private void OnViewChanged(object sender, EventArgs e)
            {
                View = (UIElement)Presenter.View;
            }

            private readonly BasePresenter _presenter;
            public BasePresenter Presenter
            {
                get { return _presenter; }
            }

            private UIElement _view;

            public event EventHandler<EventArgs> EnterMouseTrack;
            public event EventHandler<EventArgs> ExitMouseTrack;

            private UIElement View
            {
                get { return _view; }
                set
                {
                    if (_view == value)
                        return;

                    if (_view != null)
                    {
                        _view.MouseEnter -= TrackMouse;
                        _view.PreviewMouseMove -= TrackMouse;
                        _view.MouseLeave -= OnMouseLeave;
                    }
                    _view = value;
                    if (_view != null)
                    {
                        _view.MouseEnter += TrackMouse;
                        _view.PreviewMouseMove += TrackMouse;
                        _view.MouseLeave += OnMouseLeave;
                    }
                }
            }

            private void TrackMouse(object sender, MouseEventArgs e)
            {
                TrackMouse(sender, e, true);
            }

            private void OnMouseLeave(object sender, MouseEventArgs e)
            {
                TrackMouse(sender, e, false);
            }

            private void TrackMouse(object sender, MouseEventArgs e, bool hitTest)
            {
                var enterMouseTrack = EnterMouseTrack;
                var exitMouseTrack = ExitMouseTrack;
                if (enterMouseTrack == null || exitMouseTrack == null)
                    return;

                enterMouseTrack(this, EventArgs.Empty);
                if (hitTest)
                    HitTest(sender, e);
                exitMouseTrack(this, EventArgs.Empty);
            }

            private void HitTest(object sender, MouseEventArgs e)
            {
                var view = (UIElement)sender;
                var parameters = new PointHitTestParameters(e.GetPosition(view));
                VisualTreeHelper.HitTest(view, element =>
                {
                    var validationPlaceholder = element as ValidationPlaceholder;
                    if (validationPlaceholder != null)
                    {
                        validationPlaceholder.IsMouseIndirectlyOver = true;
                        return HitTestFilterBehavior.ContinueSkipChildren;
                    }
                    return HitTestFilterBehavior.Continue;
                }, result => HitTestResultBehavior.Continue, parameters);
            }
        }


        private static readonly DependencyPropertyKey IsActivePropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsActive", typeof(bool), typeof(ValidationPlaceholder),
            new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty IsActiveProperty = IsActivePropertyKey.DependencyProperty;

        static ValidationPlaceholder()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ValidationPlaceholder), new FrameworkPropertyMetadata(typeof(ValidationPlaceholder)));
        }

        public static bool GetIsActive(DependencyObject element)
        {
            return (bool)element.GetValue(IsActiveProperty);
        }

        private static void SetIsActive(DependencyObject element, bool value)
        {
            element.SetValue(IsActivePropertyKey, BooleanBoxes.Box(value));
        }

        private static readonly ConditionalWeakTable<BasePresenter, MouseTracker> s_mouseTrackers = new ConditionalWeakTable<BasePresenter, MouseTracker>();

        private static MouseTracker GetMouseTracker(BasePresenter presenter)
        {
            return s_mouseTrackers.GetValue(presenter, CreateMouseTracker);
        }

        private static MouseTracker CreateMouseTracker(BasePresenter presenter)
        {
            return new MouseTracker(presenter);
        }

        private void CoerceIsActive()
        {
            SetIsActive(this, GetIsActive());
        }

        private bool GetIsActive()
        {
            if (IsMouseIndirectlyOver)
                return true;
            if (_containingElements == null)
                return false;
            for (int i = 0; i < _containingElements.Count; i++)
            {
                if (_containingElements[i].IsKeyboardFocusWithin)
                    return true;
            }
            return false;
        }

        void IRowElement.Setup(RowPresenter rowPresenter)
        {
            WireMouseTrack(rowPresenter.DataPresenter);
        }

        void IRowElement.Refresh(RowPresenter rowPresenter)
        {
        }

        void IRowElement.Cleanup(RowPresenter rowPresenter)
        {
            UnwireMouseTrack(rowPresenter.DataPresenter);
        }

        void IScalarElement.Setup(ScalarPresenter scalarPresenter)
        {
            WireMouseTrack(scalarPresenter.Presenter);
        }

        void IScalarElement.Refresh(ScalarPresenter scalarPresenter)
        {
        }

        void IScalarElement.Cleanup(ScalarPresenter scalarPresenter)
        {
            UnwireMouseTrack(scalarPresenter.Presenter);
        }

        private void WireMouseTrack(BasePresenter presenter)
        {
            var mouseTracker = GetMouseTracker(presenter);
            mouseTracker.EnterMouseTrack += OnEnterMouseTrack;
            mouseTracker.ExitMouseTrack += OnExitMouseTrack;
        }

        private void UnwireMouseTrack(BasePresenter presenter)
        {
            var mouseTracker = GetMouseTracker(presenter);
            mouseTracker.EnterMouseTrack -= OnEnterMouseTrack;
            mouseTracker.ExitMouseTrack -= OnExitMouseTrack;
        }

        private bool IsMouseIndirectlyOver { get; set; }

        private void OnEnterMouseTrack(object sender, EventArgs e)
        {
            IsMouseIndirectlyOver = false;
        }

        private void OnExitMouseTrack(object sender, EventArgs e)
        {
            CoerceIsActive();
        }

        private IReadOnlyList<UIElement> _containingElements;
        internal void Setup(IReadOnlyList<UIElement> containingElements)
        {
            Debug.Assert(_containingElements == null && containingElements != null);

            _containingElements = containingElements;
            for (int i = 0; i < _containingElements.Count; i++)
                _containingElements[i].IsKeyboardFocusWithinChanged += OnContainingElementIsKeyboardFocusWithinChanged;

            CoerceIsActive();
        }

        private void OnContainingElementIsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CoerceIsActive();
        }

        internal void Cleanup()
        {
            Debug.Assert(_containingElements != null);

            for (int i = 0; i < _containingElements.Count; i++)
                _containingElements[i].IsKeyboardFocusWithinChanged += OnContainingElementIsKeyboardFocusWithinChanged;
            _containingElements = null;
        }
    }
}
