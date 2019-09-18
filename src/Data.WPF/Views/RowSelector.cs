using DevZest.Data.Presenters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Views
{
    /// <summary>
    /// Represents the control that can perform row selection operation.
    /// </summary>
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateNormal)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateDisabled)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateMouseOver)]
    [TemplateVisualState(GroupName = VisualStates.GroupFocus, Name = VisualStates.StateFocused)]
    [TemplateVisualState(GroupName = VisualStates.GroupFocus, Name = VisualStates.StateUnfocused)]
    [TemplateVisualState(GroupName = VisualStates.GroupSelection, Name = VisualStates.StateSelected)]
    [TemplateVisualState(GroupName = VisualStates.GroupSelection, Name = VisualStates.StateSelectedInactive)]
    [TemplateVisualState(GroupName = VisualStates.GroupSelection, Name = VisualStates.StateUnselected)]
    public partial class RowSelector : ContentControl
    {
        private sealed class CurrentRowSynchronizer : IService
        {
            public static void EnsureInitialized(DataPresenter dataPresenter)
            {
                var service = ServiceManager.GetService<CurrentRowSynchronizer>(dataPresenter);
                Debug.Assert(service != null);
            }

            public DataPresenter DataPresenter { get; private set; }

            public void Initialize(DataPresenter dataPresenter)
            {
                DataPresenter = dataPresenter;
                SynchronizeSelection();
                DataPresenter.ViewInvalidating += OnViewInvalidating;
            }

            private void OnViewInvalidating(object sender, EventArgs e)
            {
                SynchronizeSelection();
            }

            private void SynchronizeSelection()
            {
                if (ShouldSynchronizeSelection)
                    DataPresenter.Select(CurrentRow);
            }

            private RowPresenter CurrentRow
            {
                get { return DataPresenter.CurrentRow; }
            }

            private IReadOnlyCollection<RowPresenter> SelectedRows
            {
                get { return DataPresenter.SelectedRows; }
            }

            private bool ShouldSynchronizeSelection
            {
                get
                {
                    if (CurrentRow == null)
                        return false;

                    if (!CurrentRow.IsSelected)
                        return true;

                    if (CurrentRow.IsEditing)
                    {
                        var selectedRows = SelectedRows;
                        if (selectedRows.Count != 1)
                            return true;
                    }

                    return false;
                }
            }
        }

        private static readonly DependencyPropertyKey IsActivePropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsActive), typeof(bool?), typeof(RowSelector),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="IsActive"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsActiveProperty = IsActivePropertyKey.DependencyProperty;

        static RowSelector()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowSelector), new FrameworkPropertyMetadata(typeof(RowSelector)));
            ServiceManager.Register<ICommandService, CommandService>();
            ServiceManager.Register<CurrentRowSynchronizer, CurrentRowSynchronizer>();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RowSelector"/> class.
        /// </summary>
        public RowSelector()
        {
        }

        /// <summary>
        /// Gets a value indicates whether current row is active.
        /// </summary>
        public bool? IsActive
        {
            get { return (bool?)GetValue(IsActiveProperty); }
            private set { SetValue(IsActivePropertyKey, value.HasValue ? BooleanBoxes.Box(value.Value) : null); }
        }

        private void OnRowViewChanged(RowView oldValue, RowView newValue)
        {
            if (oldValue != null)
            {
                oldValue.SettingUp -= OnSettingUp;
                oldValue.Refreshing -= OnRefreshing;
                oldValue.CleaningUp -= OnCleaningUp;
            }
            if (newValue != null)
            {
                if (newValue.HasSetup)
                    OnSettingUp();
                newValue.SettingUp += OnSettingUp;
                newValue.Refreshing += OnRefreshing;
                newValue.CleaningUp += OnCleaningUp;
            }
        }

        private void OnSettingUp(object sender, EventArgs e)
        {
            OnSettingUp();
        }

        private void OnSettingUp()
        {
            var dataPresenter = RowPresenter?.DataPresenter;
            if (dataPresenter != null)
            {
                CurrentRowSynchronizer.EnsureInitialized(dataPresenter);
                this.SetupCommandEntries(dataPresenter.GetService<ICommandService>(), GetCommandEntries);
            }
            UpdateVisualState();
        }

        private static IEnumerable<CommandEntry> GetCommandEntries(ICommandService commandService, RowSelector rowSelector)
        {
            return commandService.GetCommandEntries(rowSelector);
        }

        private void OnCleaningUp(object sender, EventArgs e)
        {
            this.CleanupCommandEntries();
        }

        private void OnRefreshing(object sender, EventArgs e)
        {
            UpdateVisualState();
        }

        private RowView RowView
        {
            get { return RowView.GetCurrent(this); }
        }

        private RowPresenter RowPresenter
        {
            get { return RowView?.RowPresenter; }
        }

        private bool IsSelected
        {
            get { return RowPresenter == null ? false : RowPresenter.IsSelected; }
        }

        private void UpdateVisualState()
        {
            UpdateVisualState(true);
        }

        private bool IsDataPresenterEditing
        {
            get
            {
                var dataPresenter = DataPresenter;
                return dataPresenter == null ? false : dataPresenter.IsEditing;
            }
        }

        private void UpdateVisualState(bool useTransitions)
        {
            if (!IsEnabled)
                VisualStates.GoToState(this, useTransitions, VisualStates.StateDisabled, VisualStates.StateNormal);
            else if (IsMouseOver && !IsDataPresenterEditing)
                VisualStates.GoToState(this, useTransitions, VisualStates.StateMouseOver, VisualStates.StateNormal);
            else
                VisualStates.GoToState(this, useTransitions, VisualStates.StateNormal);

            if (IsKeyboardFocused)
                VisualStates.GoToState(this, useTransitions, VisualStates.StateFocused, VisualStates.StateUnfocused);
            else
                VisualStates.GoToState(this, useTransitions, VisualStates.StateUnfocused);

            var oldIsActive = IsActive;
            var newIsActive = IsActive = GetIsActive();
            if (!newIsActive.HasValue)
                VisualStates.GoToState(this, useTransitions, VisualStates.StateUnselected);
            else if (newIsActive.Value)
                VisualStates.GoToState(this, useTransitions, VisualStates.StateSelected);
            else
                VisualStates.GoToState(this, useTransitions, VisualStates.StateSelectedInactive, VisualStates.StateSelected);

            if (IsSelected && RowPresenter == DataPresenter?.CurrentRow && oldIsActive != newIsActive)
                DataPresenter.InvalidateView();
        }

        private bool? GetIsActive()
        {
            if (!IsSelected)
                return null;

            return DataPresenter != null && DataPresenter.CurrentRow != null && DataPresenter.CurrentRow.View != null && DataPresenter.CurrentRow.View.IsKeyboardFocusWithin;
        }

        /// <inheritdoc/>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == RowView.CurrentProperty)
                OnRowViewChanged((RowView)e.OldValue, (RowView)e.NewValue);
            else if (e.Property == IsMouseOverProperty || e.Property == IsEnabledProperty || e.Property == IsKeyboardFocusedProperty)
                UpdateVisualState();
        }

        /// <inheritdoc/>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled)
                HandleMouseButtonDown(MouseButton.Left);
            base.OnMouseLeftButtonDown(e);
        }

        /// <inheritdoc/>
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled)
                HandleMouseButtonDown(MouseButton.Right);
            base.OnMouseRightButtonDown(e);
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
    }
}
