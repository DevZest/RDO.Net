﻿using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Views
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

        public interface ICommandService : IService
        {
            IEnumerable<CommandEntry> GetCommandEntries(RowView rowView);
        }

        private sealed class CommandService : ICommandService
        {
            public DataPresenter DataPresenter { get; private set; }

            public void Initialize(DataPresenter dataPresenter)
            {
                DataPresenter = dataPresenter;
            }

            public IEnumerable<CommandEntry> GetCommandEntries(RowView rowView)
            {
                if (DataPresenter.Scrollable != null)
                {
                    if (DataPresenter.LayoutOrientation.HasValue)
                    {
                        yield return ScrollUpCommand.CommandBinding(ExecScrollUp);
                        yield return ScrollDownCommand.CommandBinding(ExecScrollDown);
                        yield return ScrollLeftCommand.CommandBinding(ExecScrollLeft);
                        yield return ScrollRightCommand.CommandBinding(ExecScrollRight);
                        yield return ScrollPageUpCommand.CommandBinding(ExecScrollPageUp);
                        yield return ScrollPageDownCommand.CommandBinding(ExecScrollPageDown);
                        yield return MoveUpCommand.InputBinding(ExecMoveUp, CanExecMoveUp, new KeyGesture(Key.Up));
                        yield return MoveDownCommand.InputBinding(ExecMoveDown, CanExecMoveDown, new KeyGesture(Key.Down));
                        yield return MoveLeftCommand.InputBinding(ExecMoveLeft, CanExecMoveLeft, new KeyGesture(Key.Left));
                        yield return MoveRightCommand.InputBinding(ExecMoveRight, CanExecMoveRight, new KeyGesture(Key.Right));
                        yield return MoveToHomeCommand.InputBinding(ExecMoveToHome, CanExecuteByKeyGesture, new KeyGesture(Key.Home));
                        yield return MoveToEndCommand.InputBinding(ExecMoveToEnd, CanExecuteByKeyGesture, new KeyGesture(Key.End));
                        yield return MoveToPageUpCommand.InputBinding(ExecMoveToPageUp, CanExecuteByKeyGesture, new KeyGesture(Key.PageUp));
                        yield return MoveToPageDownCommand.InputBinding(ExecMoveToPageDown, CanExecuteByKeyGesture, new KeyGesture(Key.PageDown));
                        if (Template.SelectionMode == SelectionMode.Extended)
                        {
                            yield return SelectExtendedUpCommand.InputBinding(ExecSelectExtendUp, CanExecMoveUp, new KeyGesture(Key.Up, ModifierKeys.Shift));
                            yield return SelectExtendedDownCommand.InputBinding(ExecSelectExtendedDown, CanExecMoveDown, new KeyGesture(Key.Down, ModifierKeys.Shift));
                            yield return SelectiExtendedLeftCommand.InputBinding(ExecSelectiExtendedLeft, CanExecMoveLeft, new KeyGesture(Key.Left, ModifierKeys.Shift));
                            yield return SelectExtendedRightCommand.InputBinding(ExecSelectExtendedRight, CanExecMoveRight, new KeyGesture(Key.Right));
                            yield return SelectExtendedHomeCommand.InputBinding(ExecSelectExtendedHome, CanExecuteByKeyGesture, new KeyGesture(Key.Home, ModifierKeys.Shift));
                            yield return SelectExtendedEndCommand.InputBinding(ExecSelectExtendedEnd, CanExecuteByKeyGesture, new KeyGesture(Key.End, ModifierKeys.Shift));
                            yield return SelectExtendedPageUpCommand.InputBinding(ExecSelectExtendedPageUp, CanExecuteByKeyGesture, new KeyGesture(Key.PageUp, ModifierKeys.Shift));
                            yield return SelectExtendedPageDownCommand.InputBinding(ExecSelectExtendedPageDown, CanExecuteByKeyGesture, new KeyGesture(Key.PageDown, ModifierKeys.Shift));
                        }
                        if (Template.SelectionMode == SelectionMode.Multiple)
                            yield return ToggleSelectionCommand.InputBinding(ExecToggleSelection, CanExecuteByKeyGesture, new KeyGesture(Key.Space));
                    }
                }
            }

            private Template Template
            {
                get { return DataPresenter.Template; }
            }

            private IScrollable Scrollable
            {
                get { return DataPresenter.Scrollable; }
            }

            private DataView View
            {
                get { return DataPresenter.View; }
            }

            private RowPresenter CurrentRow
            {
                get { return DataPresenter.CurrentRow; }
                set { DataPresenter.CurrentRow = value; }
            }

            private Orientation? LayoutOrientation
            {
                get { return DataPresenter.LayoutOrientation; }
            }

            private int FlowRepeatCount
            {
                get { return DataPresenter.FlowRepeatCount; }
            }

            private IReadOnlyList<RowPresenter> Rows
            {
                get { return DataPresenter.Rows; }
            }

            private void Select(RowPresenter rowPresenter, SelectionMode selectionMode, bool ensureVisible = true)
            {
                DataPresenter.Select(rowPresenter, selectionMode, ensureVisible);
            }

            private void ExecScrollUp(object sender, ExecutedRoutedEventArgs e)
            {
                Scrollable.ScrollBy(0, -View.ScrollLineHeight);
            }

            private void ExecScrollDown(object sender, ExecutedRoutedEventArgs e)
            {
                Scrollable.ScrollBy(0, View.ScrollLineHeight);
            }

            private void ExecScrollLeft(object sender, ExecutedRoutedEventArgs e)
            {
                Scrollable.ScrollBy(-View.ScrollLineWidth, 0);
            }

            private void ExecScrollRight(object sender, ExecutedRoutedEventArgs e)
            {
                Scrollable.ScrollBy(View.ScrollLineWidth, 0);
            }

            private void ExecScrollPageUp(object sender, ExecutedRoutedEventArgs e)
            {
                Scrollable.ScrollPageUp();
            }

            private void ExecScrollPageDown(object sender, ExecutedRoutedEventArgs e)
            {
                Scrollable.ScrollPageDown();
            }

            private bool CanSelect(Orientation orientation)
            {
                if (CurrentRow == null || !LayoutOrientation.HasValue)
                    return false;

                if (LayoutOrientation != orientation && FlowRepeatCount == 1)
                    return false;

                return true;
            }

            private RowPresenter GetRowBackward(Orientation orientation)
            {
                if (!CanSelect(orientation))
                    return null;
                var index = CurrentRow.Index - (LayoutOrientation == orientation ? FlowRepeatCount : 1);
                return index >= 0 ? Rows[index] : null;
            }

            private RowPresenter GetRowForward(Orientation orientation)
            {
                if (!CanSelect(orientation))
                    return null;
                var index = CurrentRow.Index + (LayoutOrientation == orientation ? FlowRepeatCount : 1);
                return index < Rows.Count ? Rows[index] : null;
            }

            private RowPresenter MoveUpRow
            {
                get { return GetRowBackward(Orientation.Vertical); }
            }

            private RowPresenter MoveDownRow
            {
                get { return GetRowForward(Orientation.Vertical); }
            }

            private RowPresenter MoveLeftRow
            {
                get { return GetRowBackward(Orientation.Horizontal); }
            }

            private RowPresenter MoveRightRow
            {
                get { return GetRowForward(Orientation.Horizontal); }
            }

            private bool ShouldSelectByKey
            {
                get { return Template.SelectionMode == SelectionMode.Single || Template.SelectionMode == SelectionMode.Extended; }
            }

            private void MoveTo(RowPresenter rowPresenter, bool ensureSelectVisible = true)
            {
                if (ShouldSelectByKey)
                    Select(rowPresenter, SelectionMode.Single, ensureSelectVisible);
                else
                {
                    CurrentRow = rowPresenter;
                    Scrollable.EnsureCurrentRowVisible();
                }
            }

            private void CanExecMoveUp(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanExecuteByKeyGesture(e) && MoveUpRow != null;
            }

            private void ExecMoveUp(object sender, ExecutedRoutedEventArgs e)
            {
                MoveTo(MoveUpRow);
            }

            private void CanExecMoveDown(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanExecuteByKeyGesture(e) && MoveDownRow != null;
            }

            private void ExecMoveDown(object sender, ExecutedRoutedEventArgs e)
            {
                MoveTo(MoveDownRow);
            }

            private void CanExecMoveLeft(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanExecuteByKeyGesture(e) && MoveLeftRow != null;
            }

            private void ExecMoveLeft(object sender, ExecutedRoutedEventArgs e)
            {
                MoveTo(MoveLeftRow);
            }

            private void CanExecMoveRight(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanExecuteByKeyGesture(e) && MoveRightRow != null;
            }

            private void ExecMoveRight(object sender, ExecutedRoutedEventArgs e)
            {
                MoveTo(MoveRightRow);
            }

            private void ExecMoveToHome(object sender, ExecutedRoutedEventArgs e)
            {
                MoveTo(Rows[0]);
            }

            private void ExecMoveToEnd(object sender, ExecutedRoutedEventArgs e)
            {
                MoveTo(Rows[Rows.Count - 1]);
            }

            private void ExecMoveToPageUp(object sender, ExecutedRoutedEventArgs e)
            {
                MoveTo(Scrollable.ScrollToPageUp(), false);
            }

            private void ExecMoveToPageDown(object sender, ExecutedRoutedEventArgs e)
            {
                MoveTo(Scrollable.ScrollToPageDown(), false);
            }

            private void ExecSelectExtendUp(object sender, ExecutedRoutedEventArgs e)
            {
                Select(MoveUpRow, SelectionMode.Extended);
            }

            private void ExecSelectExtendedDown(object sender, ExecutedRoutedEventArgs e)
            {
                Select(MoveDownRow, SelectionMode.Extended);
            }

            private void ExecSelectiExtendedLeft(object sender, ExecutedRoutedEventArgs e)
            {
                Select(MoveLeftRow, SelectionMode.Extended);
            }

            private void ExecSelectExtendedRight(object sender, ExecutedRoutedEventArgs e)
            {
                Select(MoveRightRow, SelectionMode.Extended);
            }

            private void CanExecuteByKeyGesture(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanExecuteByKeyGesture(e);
            }

            private bool CanExecuteByKeyGesture(CanExecuteRoutedEventArgs e)
            {
                return e.OriginalSource is RowView && Rows.Count > 0;
            }

            private void ExecSelectExtendedHome(object sender, ExecutedRoutedEventArgs e)
            {
                Select(Rows[0], SelectionMode.Extended);
            }

            private void ExecSelectExtendedEnd(object sender, ExecutedRoutedEventArgs e)
            {
                Select(Rows[Rows.Count - 1], SelectionMode.Extended);
            }

            private void ExecSelectExtendedPageUp(object sender, ExecutedRoutedEventArgs e)
            {
                Select(Scrollable.ScrollToPageUp(), SelectionMode.Extended, false);
            }

            private void ExecSelectExtendedPageDown(object sender, ExecutedRoutedEventArgs e)
            {
                Select(Scrollable.ScrollToPageDown(), SelectionMode.Extended, false);
            }

            private void ExecToggleSelection(object sender, ExecutedRoutedEventArgs e)
            {
                var rowPresenter = ((RowView)e.OriginalSource).RowPresenter;
                rowPresenter.IsSelected = !rowPresenter.IsSelected;
            }
        }

        static RowSelector()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowSelector), new FrameworkPropertyMetadata(typeof(RowSelector)));
            ServiceManager.Register<ICommandService, CommandService>();
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
                HandleMouseButtonDown(MouseButton.Left);
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled)
                HandleMouseButtonDown(MouseButton.Right);
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
