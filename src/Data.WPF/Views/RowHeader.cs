using DevZest.Data.Presenters.Primitives;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DevZest.Data.Presenters;
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
    public class RowHeader : ButtonBase, IRowElement
    {
        public static readonly DependencyProperty SeparatorBrushProperty = DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush),
            typeof(RowHeader), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SeparatorVisibilityProperty = DependencyProperty.Register("SeparatorVisibility", typeof(Visibility),
            typeof(RowHeader), new FrameworkPropertyMetadata(Visibility.Visible));

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

        void IRowElement.Cleanup(RowPresenter rowPresenter)
        {
        }

        void IRowElement.Refresh(RowPresenter rowPresenter)
        {
            UpdateVisualStates(rowPresenter);
        }

        void IRowElement.Setup(RowPresenter rowPresenter)
        {
        }

        private void UpdateVisualStates(RowPresenter rowPresenter)
        {
            UpdateVisualStates(rowPresenter, true);
        }

        private void UpdateVisualStates(RowPresenter rowPresenter, bool useTransitions)
        {
            if (!IsLoaded)
                return;

            if (rowPresenter.IsSelected && rowPresenter.DataPresenter.SelectedRows.Count == 1)
                VisualStates.GoToState(this, useTransitions, VisualStates.StateSelected);
            else
                VisualStates.GoToState(this, useTransitions, VisualStates.StateUnselected);

            if (rowPresenter.IsVirtual)
            {
                if (rowPresenter.IsEditing)
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateNewEditingRow);
                else if (rowPresenter.IsCurrent)
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateNewCurrentRow);
                else
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateNewRow);
            }
            else if (rowPresenter.IsCurrent)
            {
                if (rowPresenter.IsEditing)
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateCurrentEditingRow);
                else
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateCurrentRow);
            }
            else
                VisualStates.GoToState(this, useTransitions, VisualStates.StateRegularRow);
        }
    }
}
