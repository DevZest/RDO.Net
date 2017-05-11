using DevZest.Windows.Data;
using DevZest.Windows.Data.Primitives;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DevZest.Windows.Controls
{
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateNormal)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateDisabled)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateMouseOver)]
    [TemplateVisualState(GroupName = VisualStates.GroupFocus, Name = VisualStates.StateFocused)]
    [TemplateVisualState(GroupName = VisualStates.GroupFocus, Name = VisualStates.StateUnfocused)]
    [TemplateVisualState(GroupName = VisualStates.GroupSelection, Name = VisualStates.StateSelected)]
    [TemplateVisualState(GroupName = VisualStates.GroupSelection, Name = VisualStates.StateSelectedInactive)]
    [TemplateVisualState(GroupName = VisualStates.GroupSelection, Name = VisualStates.StateUnselected)]
    public class RowSelector : ContentControl
    {
        public static RoutedUICommand ScrollUpCommand { get { return ComponentCommands.MoveFocusUp; } }
        public static RoutedUICommand ScrollDownCommand { get { return ComponentCommands.MoveFocusDown; } }
        public static RoutedUICommand ScrollLeftCommand { get { return ComponentCommands.MoveFocusBack; } }
        public static RoutedUICommand ScrollRightCommand { get { return ComponentCommands.MoveFocusForward; } }
        public static RoutedUICommand ScrollPageUpCommand { get { return ComponentCommands.MoveFocusPageUp; } }
        public static RoutedUICommand ScrollPageDownCommand { get { return ComponentCommands.MoveFocusPageDown; } }
        public static RoutedUICommand MoveUpCommand { get { return ComponentCommands.MoveUp; } }
        public static RoutedUICommand MoveDownCommand { get { return ComponentCommands.MoveDown; } }
        public static RoutedUICommand MoveLeftCommand { get { return ComponentCommands.MoveLeft; } }
        public static RoutedUICommand MoveRightCommand { get { return ComponentCommands.MoveRight; } }
        public static RoutedUICommand MoveToPageUpCommand { get { return ComponentCommands.MoveToPageUp; } }
        public static RoutedUICommand MoveToPageDownCommand { get { return ComponentCommands.MoveToPageDown; } }
        public static RoutedUICommand MoveToHomeCommand { get { return ComponentCommands.MoveToHome; } }
        public static RoutedUICommand MoveToEndCommand { get { return ComponentCommands.MoveToEnd; } }
        public static RoutedUICommand SelectExtendedUpCommand { get { return ComponentCommands.ExtendSelectionUp; } }
        public static RoutedUICommand SelectExtendedDownCommand { get { return ComponentCommands.ExtendSelectionDown; } }
        public static RoutedUICommand SelectiExtendedLeftCommand { get { return ComponentCommands.ExtendSelectionLeft; } }
        public static RoutedUICommand SelectExtendedRightCommand { get { return ComponentCommands.ExtendSelectionRight; } }
        public static readonly RoutedUICommand SelectExtendedHomeCommand = new RoutedUICommand();
        public static readonly RoutedUICommand SelectExtendedEndCommand = new RoutedUICommand();
        public static readonly RoutedUICommand SelectExtendedPageUpCommand = new RoutedUICommand();
        public static readonly RoutedUICommand SelectExtendedPageDownCommand = new RoutedUICommand();
        public static readonly RoutedUICommand ToggleSelectionCommand = new RoutedUICommand();

        static RowSelector()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowSelector), new FrameworkPropertyMetadata(typeof(RowSelector)));
        }

        public RowSelector()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (RowView != null)
                UpdateVisualState();
        }

        private void OnRowViewChanged(RowView oldValue, RowView newValue)
        {
            if (oldValue != null)
                oldValue.Refreshing -= OnRefreshing;
            if (newValue != null)
            {
                newValue.EnsureRowSelectorCommandEntriesSetup();
                UpdateVisualState();
                newValue.Refreshing += OnRefreshing;
            }
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

        private void UpdateVisualState(bool useTransitions)
        {
            if (!IsEnabled)
                VisualStates.GoToState(this, useTransitions, VisualStates.StateDisabled, VisualStates.StateNormal);
            else if (IsMouseOver)
                VisualStates.GoToState(this, useTransitions, VisualStates.StateMouseOver, VisualStates.StateNormal);
            else
                VisualStates.GoToState(this, useTransitions, VisualStates.StateNormal);

            if (IsKeyboardFocused)
                VisualStates.GoToState(this, useTransitions, VisualStates.StateFocused, VisualStates.StateUnfocused);
            else
                VisualStates.GoToState(this, useTransitions, VisualStates.StateUnfocused);

            if (IsSelected)
            {
                if (IsKeyboardFocusWithin)
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateSelected);
                else
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateSelectedInactive, VisualStates.StateSelected);
            }
            else
                VisualStates.GoToState(this, useTransitions, VisualStates.StateUnselected);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == RowView.CurrentProperty)
                OnRowViewChanged((RowView)e.OldValue, (RowView)e.NewValue);
            else if (e.Property == IsMouseOverProperty || e.Property == IsEnabledProperty || e.Property == IsKeyboardFocusedProperty || e.Property == IsKeyboardFocusWithinProperty)
                UpdateVisualState();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled)
                e.Handled = HandleMouseButtonDown(MouseButton.Left);
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled)
                e.Handled = HandleMouseButtonDown(MouseButton.Right);
            base.OnMouseRightButtonDown(e);
        }

        private ElementManager ElementManager
        {
            get { return RowPresenter.ElementManager; }
        }

        private bool HandleMouseButtonDown(MouseButton mouseButton)
        {
            var oldCurrentRow = ElementManager.CurrentRow;
            var focusMoved = IsKeyboardFocusWithin ? true : MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            var selected = Select(mouseButton, oldCurrentRow);
            return focusMoved || selected;
        }

        private SelectionMode? TemplateSelectionMode
        {
            get { return ElementManager.Template.SelectionMode; }
        }

        private bool Select(MouseButton mouseButton, RowPresenter currentRow)
        {
            if (!TemplateSelectionMode.HasValue)
                return false;

            switch (TemplateSelectionMode.Value)
            {
                case SelectionMode.Single:
                    Select((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control ? SelectionMode.Multiple : SelectionMode.Single, currentRow);
                    return true;
                case SelectionMode.Multiple:
                    Select(SelectionMode.Multiple, currentRow);
                    return true;
                case SelectionMode.Extended:
                    if (mouseButton != MouseButton.Left)
                    {
                        if (mouseButton == MouseButton.Right && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == ModifierKeys.None)
                        {
                            if (RowPresenter.IsSelected)
                                return false;
                            Select(SelectionMode.Single, currentRow);
                            return true;
                        }
                        return false;
                    }

                    if (IsControlDown && IsShiftDown)
                        return false;

                    var selectionMode = IsShiftDown ? SelectionMode.Extended : (IsControlDown ? SelectionMode.Multiple : SelectionMode.Single);
                    Select(selectionMode, currentRow);
                    return true;
            }
            return false;
        }

        private bool IsControlDown
        {
            get { return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control; }
        }

        private bool IsShiftDown
        {
            get { return (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift; }
        }

        private void Select(SelectionMode selectionMode, RowPresenter oldCurrentRow)
        {
            ElementManager.Select(RowPresenter, selectionMode, oldCurrentRow);
        }
    }
}
