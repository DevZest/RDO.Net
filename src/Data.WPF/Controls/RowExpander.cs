using DevZest.Windows.Data;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DevZest.Windows.Controls
{
    public class RowExpander : Control
    {
        private static DependencyPropertyKey IsExpandedPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsExpanded), typeof(bool), typeof(RowExpander),
            new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty IsExpandedProperty = IsExpandedPropertyKey.DependencyProperty;
        private static DependencyPropertyKey HasChildrenPropertyKey = DependencyProperty.RegisterReadOnly(nameof(HasChildren), typeof(bool), typeof(RowExpander),
            new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty HasChildrenProperty = HasChildrenPropertyKey.DependencyProperty;

        static RowExpander()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowExpander), new FrameworkPropertyMetadata(typeof(RowExpander)));
            FocusableProperty.OverrideMetadata(typeof(RowExpander), new FrameworkPropertyMetadata(BooleanBoxes.False));
            KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(RowExpander), new FrameworkPropertyMetadata(BooleanBoxes.False));
        }

        private static readonly DependencyProperty RowViewProperty = DependencyProperty.Register(nameof(RowView), typeof(RowView), typeof(RowExpander),
            new FrameworkPropertyMetadata(null, OnRowViewChanged));

        private static void OnRowViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RowExpander)d).OnRowViewChanged((RowView)e.OldValue, (RowView)e.NewValue);
        }

        public RowExpander()
        {
            var binding = new System.Windows.Data.Binding();
            binding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(RowView), 1);
            SetBinding(RowViewProperty, binding);
        }

        private void OnRowViewChanged(RowView oldValue, RowView newValue)
        {
            if (oldValue != null)
                oldValue.Refreshing -= OnRefreshing;
            if (newValue != null)
                newValue.Refreshing += OnRefreshing;
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

        private RowView RowView
        {
            get { return this.FindAncestor<RowView>(); }
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
