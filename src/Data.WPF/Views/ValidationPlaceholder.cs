using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DevZest.Data.Views
{
    public class ValidationPlaceholder : Control, IRowElement, IScalarElement
    {
        private interface IMouseTracker : IService
        {
            event EventHandler<EventArgs> EnterMouseTrack;
            event EventHandler<EventArgs> ExitMouseTrack;
        }

        private sealed class MouseTracker : IMouseTracker
        {
            private DataPresenter _dataPresenter;
            public DataPresenter DataPresenter
            {
                get { return _dataPresenter; }
            }

            private DataView _dataView;

            public event EventHandler<EventArgs> EnterMouseTrack;
            public event EventHandler<EventArgs> ExitMouseTrack;

            private DataView DataView
            {
                get { return _dataView; }
                set
                {
                    if (_dataView == value)
                        return;

                    if (_dataView != null)
                    {
                        _dataView.MouseEnter -= TrackMouse;
                        _dataView.PreviewMouseMove -= TrackMouse;
                        _dataView.MouseLeave -= OnMouseLeave;
                    }
                    _dataView = value;
                    if (_dataView != null)
                    {
                        _dataView.MouseEnter += TrackMouse;
                        _dataView.PreviewMouseMove += TrackMouse;
                        _dataView.MouseLeave += OnMouseLeave;
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
                var dataView = (DataView)sender;
                var parameters = new PointHitTestParameters(e.GetPosition(dataView));
                VisualTreeHelper.HitTest(dataView, element =>
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

            public void Initialize(DataPresenter dataPresenter)
            {
                _dataPresenter = dataPresenter;
                DataView = dataPresenter.View;
                _dataPresenter.ViewChanged += OnViewChanged;
            }

            private void OnViewChanged(object sender, EventArgs e)
            {
                DataView = DataPresenter.View;
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

        private static IMouseTracker GetMouseTrackerService(DataPresenter dataPresenter)
        {
            if (!ServiceManager.IsRegistered<IMouseTracker>())
                ServiceManager.Register<IMouseTracker, MouseTracker>();
            var service = ServiceManager.GetService<IMouseTracker>(dataPresenter);
            Debug.Assert(service != null);
            return service;
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
            WireMouseTrack(scalarPresenter.DataPresenter);
        }

        void IScalarElement.Refresh(ScalarPresenter scalarPresenter)
        {
        }

        void IScalarElement.Cleanup(ScalarPresenter scalarPresenter)
        {
            UnwireMouseTrack(scalarPresenter.DataPresenter);
        }

        private void WireMouseTrack(DataPresenter dataPresenter)
        {
            var mouseTracker = GetMouseTrackerService(dataPresenter);
            mouseTracker.EnterMouseTrack += OnEnterMouseTrack;
            mouseTracker.ExitMouseTrack += OnExitMouseTrack;
        }

        private void UnwireMouseTrack(DataPresenter dataPresenter)
        {
            var mouseTracker = GetMouseTrackerService(dataPresenter);
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
