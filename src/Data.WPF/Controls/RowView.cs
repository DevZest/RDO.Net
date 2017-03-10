using DevZest.Windows.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using DevZest.Windows.Controls.Primitives;
using DevZest.Windows.Data;
using System.Windows.Controls;

namespace DevZest.Windows.Controls
{
    [TemplatePart(Name = "PART_Panel", Type = typeof(RowViewPanel))]
    public class RowView : ContainerView
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

        static RowView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowView), new FrameworkPropertyMetadata(typeof(RowView)));
        }

        public RowPresenter RowPresenter { get; private set; }

        public sealed override int ContainerOrdinal
        {
            get { return RowPresenter == null ? -1 : RowPresenter.Index / RowPresenter.ElementManager.FlowCount; }
        }

        internal sealed override ElementManager ElementManager
        {
            get { return RowPresenter == null ? null : RowPresenter.ElementManager; }
        }

        internal RowBindingCollection RowBindings
        {
            get { return RowPresenter.RowBindings; }
        }

        private IElementCollection ElementCollection { get; set; }
        internal IReadOnlyList<UIElement> Elements
        {
            get { return ElementCollection; }
        }

        internal sealed override bool AffectedOnRowsChanged
        {
            get { return false; }
        }

        internal sealed override void ReloadCurrentRow(RowPresenter oldValue)
        {
            Debug.Assert(oldValue != null && RowPresenter == oldValue);
            var newValue = ElementManager.CurrentRow;
            Debug.Assert(newValue != null);

            if (oldValue == newValue)
                return;

            CleanupElements(false);
            oldValue.View = null;

            RowPresenter = newValue;
            newValue.View = this;
            if (Elements != null)
            {
                foreach (var element in Elements)
                    element.SetRowPresenter(newValue);
            }

            SetupElements(false);
        }

        internal sealed override void Setup(ElementManager elementManager, int containerOrdinal)
        {
            Setup(elementManager.Rows[containerOrdinal]);
        }

        internal virtual void Setup(RowPresenter rowPresenter)
        {
            Debug.Assert(RowPresenter == null && rowPresenter != null);
            RowPresenter = rowPresenter;
            rowPresenter.View = this;
            SetupElements(true);
        }

        internal sealed override void Cleanup()
        {
            Debug.Assert(RowPresenter != null);
            Debug.Assert(ElementCollection != null);

            CleanupElements(true);
            RowPresenter.View = null;
            RowPresenter = null;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template == null)
                return;

            var panel = Template.FindName("PART_Panel", this) as RowViewPanel;
            if (panel != null)
                Setup(panel);
        }

        internal void Setup(FrameworkElement elementsPanel)
        {
            if (ElementCollection != null)
            {
                if (ElementCollection.Parent == elementsPanel)
                    return;

                CleanupElements(true);
            }

            ElementCollection = ElementCollectionFactory.Create(elementsPanel);
            SetupElements(true);
        }

        private void SetupElements(bool addToCollection)
        {
            if (RowPresenter == null || ElementCollection == null)
                return;

            var rowBindings = RowBindings;
            if (addToCollection)
                rowBindings.BeginSetup();
            else
            {
                for (int i = 0; i < rowBindings.Count; i++)
                    rowBindings[i].BeginSetup(Elements[i]);
            }
            for (int i = 0; i < rowBindings.Count; i++)
            {
                var rowBinding = rowBindings[i];
                var element = rowBinding.Setup(RowPresenter);
                if (addToCollection)
                    ElementCollection.Add(element);
            }
            rowBindings.EndSetup();
        }

        private void CleanupElements(bool removeFromCollection)
        {
            var rowBindings = RowBindings;
            Debug.Assert(Elements.Count == rowBindings.Count);
            for (int i = 0; i < rowBindings.Count; i++)
            {
                var rowBinding = rowBindings[i];
                var element = Elements[i];
                rowBinding.Cleanup(element);
            }
            if (removeFromCollection)
                ElementCollection.RemoveRange(0, Elements.Count);
        }

        internal sealed override void Refresh()
        {
            Debug.Assert(RowPresenter != null);

            if (Elements == null)
                return;

            var rowBindings = RowBindings;
            Debug.Assert(Elements.Count == rowBindings.Count);
            for (int i = 0; i < rowBindings.Count; i++)
            {
                var rowBinding = rowBindings[i];
                var element = Elements[i];
                rowBinding.Refresh(element);
            }
        }

        internal void Flush()
        {
            Debug.Assert(RowPresenter != null && RowPresenter == ElementManager.CurrentRow);

            if (Elements == null)
                return;

            var rowBindings = RowBindings;
            Debug.Assert(Elements.Count == rowBindings.Count);
            for (int i = 0; i < rowBindings.Count; i++)
            {
                var rowBinding = rowBindings[i];
                var element = Elements[i];
                rowBinding.FlushInput(element);
            }
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

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);
            if (ElementManager == null || RowPresenter == null)
                return;

            if ((bool)e.NewValue)
                ElementManager.OnFocused(this);
        }

        internal int FlowIndex
        {
            get { return RowPresenter.Index % RowPresenter.ElementManager.FlowCount; }
        }

        private DataPresenter DataPresenter
        {
            get { return RowPresenter == null ? null : RowPresenter.DataPresenter; }
        }

        internal void SetupCommandEntries(DataPresenter dataPresenter)
        {
            if (dataPresenter == null)
                return;

            this.SetupCommandEntries(dataPresenter.RowViewCommandEntries);
        }
    }
}
