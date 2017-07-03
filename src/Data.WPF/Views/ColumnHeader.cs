using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Presenters.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace DevZest.Data.Views
{
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateNormal)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateMouseOver)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StatePressed)]
    [TemplateVisualState(GroupName = VisualStates.GroupSort, Name = VisualStates.StateUnsorted)]
    [TemplateVisualState(GroupName = VisualStates.GroupSort, Name = VisualStates.StateSortAscending)]
    [TemplateVisualState(GroupName = VisualStates.GroupSort, Name = VisualStates.StateSortDescending)]
    public class ColumnHeader : ButtonBase, IScalarElement
    {
        public static readonly DependencyProperty CanSortProperty = DependencyProperty.Register(nameof(CanSort), typeof(bool),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(BooleanBoxes.True));

        private static readonly DependencyPropertyKey SortDirectionPropertyKey = DependencyProperty.RegisterReadOnly(nameof(SortDirection), typeof(SortDirection),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(SortDirection.Unspecified));

        public static readonly DependencyProperty SortDirectionProperty = SortDirectionPropertyKey.DependencyProperty;

        public static readonly DependencyProperty SeparatorBrushProperty = DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SeparatorVisibilityProperty = DependencyProperty.Register("SeparatorVisibility", typeof(Visibility),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(Visibility.Visible));

        static ColumnHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColumnHeader), new FrameworkPropertyMetadata(typeof(ColumnHeader)));
        }

        public ColumnHeader()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualState();
        }

        public Column Column { get; set; }

        public bool CanSort
        {
            get { return (bool)GetValue(CanSortProperty); }
            set { SetValue(CanSortProperty, BooleanBoxes.Box(value)); }
        }

        public SortDirection SortDirection
        {
            get { return (SortDirection)GetValue(SortDirectionProperty); }
            private set { SetValue(SortDirectionPropertyKey, value); }
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

        void IScalarElement.Cleanup(ScalarPresenter scalarPresenter)
        {
        }

        void IScalarElement.Refresh(ScalarPresenter scalarPresenter)
        {
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            UpdateVisualState(true);
        }

        private void UpdateVisualState(bool useTransitions)
        {
            SortDirection = GetSortDirection();
            if (!IsLoaded)
                return;
            if (SortDirection == SortDirection.Ascending)
                VisualStates.GoToState(this, useTransitions, VisualStates.StateSortAscending);
            else if (SortDirection == SortDirection.Descending)
                VisualStates.GoToState(this, useTransitions, VisualStates.StateSortDescending);
            else
                VisualStates.GoToState(this, useTransitions, VisualStates.StateUnsorted);
        }

        private DataPresenter DataPresenter
        {
            get { return DataView.GetCurrent(this)?.DataPresenter; }
        }

        private SortDirection GetSortDirection()
        {
            if (!CanSort)
                return SortDirection.Unspecified;

            var dataPresenter = DataPresenter;
            var orderBy = dataPresenter?.GetService<ISortService>()?.OrderBy;
            if (orderBy == null || orderBy.Count == 0)
                return SortDirection.Unspecified;

            for (int i = 0; i < orderBy.Count; i++)
            {
                var columnComparer = orderBy[i];
                if (columnComparer.GetColumn(dataPresenter.DataSet.Model) == Column)
                    return columnComparer.Direction;
            }
            return SortDirection.Unspecified;
        }

        void IScalarElement.Setup(ScalarPresenter scalarPresenter)
        {
            var dataPresenter = scalarPresenter.DataPresenter;
            var commands = GetCommands(dataPresenter);
            EnsureCommandEntriesSetup(commands);
            commands.EnsureCommandEntriesSetup(dataPresenter.View);
        }

        private bool _commandEntriesSetup;
        private void EnsureCommandEntriesSetup(ColumnHeaderCommands commands)
        {
            if (_commandEntriesSetup)
                return;

            this.SetupCommandEntries(commands.GetCommandEntries(this));
            _commandEntriesSetup = true;
        }

        private static ColumnHeaderCommands GetCommands(DataPresenter dataPresenter)
        {
            Debug.Assert(dataPresenter != null);
            return dataPresenter.GetService<ColumnHeaderCommands>(() => new ColumnHeaderCommands());
        }
    }
}
