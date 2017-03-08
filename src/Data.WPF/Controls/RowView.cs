using DevZest.Windows.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using DevZest.Windows.Controls.Primitives;
using DevZest.Windows.Data;

namespace DevZest.Windows.Controls
{
    [TemplatePart(Name = "PART_Panel", Type = typeof(RowViewPanel))]
    public class RowView : ContainerView
    {
        public static RoutedUICommand SelectUpCommand { get { return ComponentCommands.MoveUp; } }
        public static RoutedUICommand SelectDownCommand { get { return ComponentCommands.MoveDown; } }
        public static RoutedUICommand SelectLeftCommand { get { return ComponentCommands.MoveLeft; } }
        public static RoutedUICommand SelectRightCommand { get { return ComponentCommands.MoveRight; } }
        public static RoutedUICommand SelectPageUpCommand { get { return ComponentCommands.MoveToPageUp; } }
        public static RoutedUICommand SelectPageDownCommand { get { return ComponentCommands.MoveToPageDown; } }
        public static RoutedUICommand SelectHomeCommand { get { return ComponentCommands.MoveToHome; } }
        public static RoutedUICommand SelectEndCommand { get { return ComponentCommands.MoveToEnd; } }
        public static RoutedUICommand ExtendSelectionUpCommand { get { return ComponentCommands.ExtendSelectionUp; } }
        public static RoutedUICommand ExtendSelectionDownCommand { get { return ComponentCommands.ExtendSelectionDown; } }
        public static RoutedUICommand ExtendSelectionLeftCommand { get { return ComponentCommands.ExtendSelectionLeft; } }
        public static RoutedUICommand ExtendSelectionRightCommand { get { return ComponentCommands.ExtendSelectionRight; } }
        public static readonly RoutedUICommand ExtendSelectionHomeCommand = new RoutedUICommand();
        public static readonly RoutedUICommand ExtendSelectionEndCommand = new RoutedUICommand();
        public static readonly RoutedUICommand ExtendSelectionPageUpCommand = new RoutedUICommand();
        public static readonly RoutedUICommand ExtendSelectionPageDownCommand = new RoutedUICommand();

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

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (!IsKeyboardFocusWithin)
                e.Handled = MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
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
