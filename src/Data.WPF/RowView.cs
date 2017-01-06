using DevZest.Data.Windows.Primitives;
using DevZest.Data.Windows.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;

namespace DevZest.Data.Windows
{
    [TemplatePart(Name = "PART_Panel", Type = typeof(RowViewPanel))]
    public class RowView : ContainerView
    {
        static RowView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowView), new FrameworkPropertyMetadata(typeof(RowView)));
        }

        public RowPresenter RowPresenter { get; private set; }

        public sealed override int ContainerOrdinal
        {
            get { return RowPresenter == null ? -1 : BlockOrdinal; }
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

        internal sealed override void OnCurrentRowChanged(RowPresenter oldCurrentRow)
        {
            var newCurrentRow = ElementManager.CurrentRow;
            if (oldCurrentRow != newCurrentRow)
                Reload(newCurrentRow);
        }

        internal void Reload(RowPresenter rowPresenter)
        {
            Debug.Assert(RowPresenter != null && rowPresenter != null && RowPresenter != rowPresenter);

            InternalCleanup();
            RowPresenter.View = null;

            RowPresenter = rowPresenter;
            rowPresenter.View = this;
            if (Elements != null)
            {
                foreach (var element in Elements)
                    element.SetRowPresenter(rowPresenter);
            }

            OnSetup();
        }

        internal sealed override void ReloadIfInvalid()
        {
        }

        internal virtual void Setup(RowPresenter rowPresenter)
        {
            Debug.Assert(RowPresenter == null && rowPresenter != null);
            RowPresenter = rowPresenter;
            rowPresenter.View = this;
            SetupElements();
        }

        internal sealed override void Cleanup()
        {
            Debug.Assert(RowPresenter != null);
            Debug.Assert(ElementCollection != null);

            CleanupElements();
            RowPresenter.View = null;
            RowPresenter = null;
        }

        private void CleanupElements()
        {
            InternalCleanup();
            ClearElements();
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

                CleanupElements();
            }

            ElementCollection = ElementCollectionFactory.Create(elementsPanel);
            SetupElements();
        }

        private void SetupElements()
        {
            if (RowPresenter == null || ElementCollection == null)
                return;

            AddElements();
            OnSetup();
        }

        private void AddElements()
        {
            var rowBindings = RowBindings;
            rowBindings.BeginSetup();
            for (int i = 0; i < rowBindings.Count; i++)
            {
                var rowBinding = rowBindings[i];
                var element = rowBinding.Setup(RowPresenter);
                ElementCollection.Add(element);
            }
            rowBindings.EndSetup();
        }

        private void ClearElements()
        {
            var rowBindings = RowBindings;
            Debug.Assert(Elements.Count == rowBindings.Count);
            for (int i = 0; i < rowBindings.Count; i++)
            {
                var rowBinding = rowBindings[i];
                var element = Elements[i];
                rowBinding.Cleanup(element);
            }
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
                ElementManager.SetCurrentRow(RowPresenter, false);
            ElementManager.InvalidateElements();
        }

        internal int BlockOrdinal
        {
            get { return RowPresenter.Index / RowPresenter.ElementManager.BlockDimensions; }
        }

        internal int BlockDimension
        {
            get { return RowPresenter.Index % RowPresenter.ElementManager.BlockDimensions; }
        }

        private DataPresenter DataPresenter
        {
            get { return RowPresenter == null ? null : RowPresenter.DataPresenter; }
        }

        internal void SetupCommandBindings()
        {
            var dataPresenter = DataPresenter;
            if (dataPresenter == null)
                return;

            var commandBindings = dataPresenter.RowViewCommandBindings;
            if (commandBindings == null)
                return;

            foreach (var commandBinding in commandBindings)
                CommandBindings.Add(commandBinding);
        }

        internal void SetupInputBindings()
        {
            var dataPresenter = DataPresenter;
            if (dataPresenter == null)
                return;

            var inputBindings = dataPresenter.RowViewInputBindings;
            if (inputBindings == null)
                return;

            foreach (var inputBinding in inputBindings)
                InputBindings.Add(inputBinding);
        }
    }
}
