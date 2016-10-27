﻿using DevZest.Data.Windows.Primitives;
using DevZest.Data.Windows.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Windows
{
    [TemplatePart(Name = "PART_Panel", Type = typeof(RowElementPanel))]
    public class RowView : Control
    {
        static RowView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowView), new FrameworkPropertyMetadata(typeof(RowView)));
        }

        public RowPresenter RowPresenter { get; private set; }

        internal RowBindingCollection RowBindings
        {
            get { return RowPresenter.RowBindings; }
        }

        private IElementCollection ElementCollection { get; set; }
        internal IReadOnlyList<UIElement> Elements
        {
            get { return ElementCollection; }
        }

        protected virtual void OnSetup()
        {
        }

        private void InternalCleanup()
        {
            OnCleanup();
        }

        protected virtual void OnCleanup()
        {
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

        internal virtual void Setup(RowPresenter rowPresenter)
        {
            Debug.Assert(RowPresenter == null && rowPresenter != null);
            RowPresenter = rowPresenter;
            rowPresenter.View = this;
            SetupElements();
        }

        internal void Cleanup()
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

            var panel = Template.FindName("PART_Panel", this) as RowElementPanel;
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
            for (int i = 0; i < rowBindings.Count; i++)
            {
                var rowBinding = rowBindings[i];
                var element = rowBinding.Setup(RowPresenter);
                ElementCollection.Add(element);
            }
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

        internal void Refresh(bool isReload)
        {
            Debug.Assert(RowPresenter != null || Elements != null);

            var rowBindings = RowBindings;
            Debug.Assert(Elements.Count == rowBindings.Count);
            for (int i = 0; i < rowBindings.Count; i++)
            {
                var rowBinding = rowBindings[i];
                rowBinding.Refresh(Elements[i]);
            }
            OnRefresh();
        }

        protected virtual void OnRefresh()
        {
        }

        private static RowView Focused { get; set; }

        internal static RowPresenter FocusedRow
        {
            get { return Focused == null ? null : Focused.RowPresenter; }
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            Focused = this;
            base.OnGotKeyboardFocus(e);
        }

        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            Debug.Assert(Focused == this);
            if (ShouldCommitEditing(e.NewFocus))
            {
                RowPresenter.RowManager.CommitEdit();
            }
            base.OnPreviewLostKeyboardFocus(e);
        }

        private bool ShouldCommitEditing(IInputElement newFocus)
        {
            if (RowPresenter == null || !RowPresenter.IsEditing)
                return false;

            var element = newFocus as DependencyObject;
            if (element == null)
                return true;

            if (this.Contains(element))
                return false;

            var currentFocusScope = FocusManager.GetFocusScope(this);
            var newFocusScope = FocusManager.GetFocusScope((DependencyObject)element);
            return !currentFocusScope.Contains(newFocusScope);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            Focused = null;
            base.OnLostKeyboardFocus(e);
        }

        internal int BlockOrdinal
        {
            get { return RowPresenter.Index / RowPresenter.ElementManager.BlockDimensions; }
        }

        internal int BlockDimension
        {
            get { return RowPresenter.Index % RowPresenter.ElementManager.BlockDimensions; }
        }

        internal void FlushInput()
        {
            var rowBindings = RowBindings;
            Debug.Assert(Elements.Count == rowBindings.Count);
            for (int i = 0; i < rowBindings.Count; i++)
            {
                var rowBinding = rowBindings[i];
                rowBinding.FlushInput(Elements[i]);
            }
        }
    }
}
