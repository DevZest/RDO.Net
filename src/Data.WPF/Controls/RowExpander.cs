using DevZest.Windows.Data;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Windows.Controls
{
    public class RowExpander : Control
    {
        private sealed class ToggleExpandCommand : ICommand
        {
            public ToggleExpandCommand(RowExpander rowExpander)
            {
                _rowExpander = rowExpander;
            }

            private readonly RowExpander _rowExpander;

            private RowPresenter RowPresenter
            {
                get { return _rowExpander.RowPresenter; }
            }

            public event EventHandler CanExecuteChanged = delegate { };

            public bool CanExecute(object parameter)
            {
                var rowPresenter = RowPresenter;
                return rowPresenter != null && rowPresenter.HasChildren;
            }

            public void Execute(object parameter)
            {
                RowPresenter.ToggleExpandState();
            }

            public void RaiseCanExecutedChangedEvent()
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        private static readonly DependencyPropertyKey IsExpandedPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsExpanded), typeof(bool), typeof(RowExpander),
            new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty IsExpandedProperty = IsExpandedPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey HasChildrenPropertyKey = DependencyProperty.RegisterReadOnly(nameof(HasChildren), typeof(bool), typeof(RowExpander),
            new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty HasChildrenProperty = HasChildrenPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey ToggleExpandStateCommandPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ToggleExpandStateCommand), typeof(ICommand),
            typeof(RowExpander), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ToggleExpandStateCommandProperty = ToggleExpandStateCommandPropertyKey.DependencyProperty;

        static RowExpander()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowExpander), new FrameworkPropertyMetadata(typeof(RowExpander)));
            FocusableProperty.OverrideMetadata(typeof(RowExpander), new FrameworkPropertyMetadata(BooleanBoxes.False));
            KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(RowExpander), new FrameworkPropertyMetadata(BooleanBoxes.False));
        }

        public RowExpander()
        {
            ToggleExpandStateCommand = new ToggleExpandCommand(this);
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (RowView != null)
                UpdateState();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == RowView.CurrentProperty)
                OnRowViewChanged((RowView)e.OldValue, (RowView)e.NewValue);
        }

        private void OnRowViewChanged(RowView oldValue, RowView newValue)
        {
            if (oldValue != null)
                oldValue.Refreshing -= OnRefreshing;
            if (newValue != null)
            {
                newValue.Refreshing += OnRefreshing;
                UpdateState();
            }
        }

        private void OnRefreshing(object sender, EventArgs e)
        {
            UpdateState();
        }

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            private set { SetValue(IsExpandedPropertyKey, BooleanBoxes.Box(value)); }
        }

        public bool HasChildren
        {
            get { return (bool)GetValue(HasChildrenProperty); }
            private set { SetValue(HasChildrenPropertyKey, BooleanBoxes.Box(value)); }
        }

        public ICommand ToggleExpandStateCommand
        {
            get { return (ICommand)GetValue(ToggleExpandStateCommandProperty); }
            private set { SetValue(ToggleExpandStateCommandPropertyKey, value); }
        }

        private RowView RowView
        {
            get { return RowView.GetCurrent(this); }
        }

        private RowPresenter RowPresenter
        {
            get { return RowView == null ? null : RowView.RowPresenter; }
        }

        private void UpdateState()
        {
            Debug.Assert(RowPresenter != null);
            var rowPresenter = RowPresenter;
            IsExpanded = rowPresenter.IsExpanded;
            HasChildren = rowPresenter.HasChildren;
            ((ToggleExpandCommand)ToggleExpandStateCommand).RaiseCanExecutedChangedEvent();
        }
    }
}
