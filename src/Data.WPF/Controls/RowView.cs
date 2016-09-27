using DevZest.Data.Windows.Primitives;
using DevZest.Data.Windows.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DevZest.Data.Windows.Controls
{
    [TemplatePart(Name = "PART_Panel", Type = typeof(RowElementPanel))]
    public class RowView : Control
    {
        private static readonly DependencyPropertyKey RowPresenterPropertyKey = DependencyProperty.RegisterReadOnly(nameof(RowPresenter),
            typeof(RowPresenter), typeof(RowView), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty RowPresenterProperty = RowPresenterPropertyKey.DependencyProperty;

        static RowView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowView), new FrameworkPropertyMetadata(typeof(RowView)));
        }

        public RowPresenter RowPresenter
        {
            get { return (RowPresenter)GetValue(RowPresenterProperty); }
            private set { SetValue(RowPresenterPropertyKey, value); }
        }

        internal RowItemCollection RowItems
        {
            get { return RowPresenter.RowItems; }
        }

        private IElementCollection ElementCollection { get; set; }
        internal IReadOnlyList<UIElement> Elements
        {
            get { return ElementCollection; }
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
            if (RowPresenter.Template.OnCleanupRowView != null)
                RowPresenter.Template.OnCleanupRowView(this);
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
            if (RowPresenter.Template.OnSetupRowView != null)
                RowPresenter.Template.OnSetupRowView(this);
        }

        private void AddElements()
        {
            var rowItems = RowItems;
            for (int i = 0; i < rowItems.Count; i++)
            {
                var rowItem = rowItems[i];
                var element = rowItem.Setup(RowPresenter);
                ElementCollection.Add(element);
            }
        }

        private void ClearElements()
        {
            var rowItems = RowItems;
            Debug.Assert(Elements.Count == rowItems.Count);
            for (int i = 0; i < rowItems.Count; i++)
            {
                var rowItem = rowItems[i];
                var element = Elements[i];
                rowItem.Cleanup(element);
            }
            ElementCollection.RemoveRange(0, Elements.Count);
        }

        internal void Refresh()
        {
            Debug.Assert(RowPresenter != null || Elements != null);

            var rowItems = RowItems;
            Debug.Assert(Elements.Count == rowItems.Count);
            for (int i = 0; i < rowItems.Count; i++)
            {
                var rowItem = rowItems[i];
                var element = Elements[i];
                rowItem.Refresh(element);
            }
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
    }
}
