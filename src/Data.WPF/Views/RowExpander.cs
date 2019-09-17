using DevZest.Data.Presenters;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Views
{
    /// <summary>
    /// Represents the control that displays a header that can expand/collapse child rows.
    /// </summary>
    public class RowExpander : Control
    {
        /// <summary>
        /// Contains commands implemented by <see cref="RowExpander"/> class.
        /// </summary>
        public abstract class Commands
        {
            /// <summary>
            /// Command to toggle expand state.
            /// </summary>
            public static readonly RoutedUICommand ToggleExpand = new RoutedUICommand();
        }

        private static readonly DependencyPropertyKey IsExpandedPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsExpanded), typeof(bool), typeof(RowExpander),
            new FrameworkPropertyMetadata(BooleanBoxes.False));

        /// <summary>
        /// Identifies the <see cref="IsExpanded"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty = IsExpandedPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey HasChildrenPropertyKey = DependencyProperty.RegisterReadOnly(nameof(HasChildren), typeof(bool), typeof(RowExpander),
            new FrameworkPropertyMetadata(BooleanBoxes.False));

        /// <summary>
        /// Identifies the <see cref="HasChildren"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HasChildrenProperty = HasChildrenPropertyKey.DependencyProperty;

        static RowExpander()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowExpander), new FrameworkPropertyMetadata(typeof(RowExpander)));
            FocusableProperty.OverrideMetadata(typeof(RowExpander), new FrameworkPropertyMetadata(BooleanBoxes.False));
            KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(RowExpander), new FrameworkPropertyMetadata(BooleanBoxes.False));
        }

        /// <summary>
        ///  Initializes a new instance of <see cref="RowExpander"/> class.
        /// </summary>
        public RowExpander()
        {
            CommandBindings.Add(new CommandBinding(Commands.ToggleExpand, ExecToggleExpand, CanToggleExpand));
        }

        private void CanToggleExpand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = RowPresenter != null && RowPresenter.HasChildren;
        }

        private void ExecToggleExpand(object sender, ExecutedRoutedEventArgs e)
        {
            RowPresenter.ToggleExpandState();
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// Gets a value indicates whether current row is expanded. This is a dependency property.
        /// </summary>
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            private set { SetValue(IsExpandedPropertyKey, BooleanBoxes.Box(value)); }
        }

        /// <summary>
        /// Gets a value indicates whether current row has children. This is a dependency property.
        /// </summary>
        public bool HasChildren
        {
            get { return (bool)GetValue(HasChildrenProperty); }
            private set { SetValue(HasChildrenPropertyKey, BooleanBoxes.Box(value)); }
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
        }
    }
}
