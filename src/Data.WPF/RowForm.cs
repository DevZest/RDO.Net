using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Windows
{
    [TemplatePart(Name = PART_RowPanel, Type = typeof(RowPanel))]
    public class RowForm : Control
    {
        private const string PART_RowPanel = "PART_RowPanel";

        public RowView View { get; private set; }

        private bool _elementsCreated;
        private IElementCollection _elements;
        internal IReadOnlyList<UIElement> Elements
        {
            get { return _elements; }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Debug.Assert(View == null);
            var rowPanel = GetTemplateChild(PART_RowPanel) as RowPanel;
            _elements = rowPanel == null ? IElementCollectionFactory.Create(null) : rowPanel.ElementCollection;
            _elementsCreated = false;
        }

        internal void Initialize(RowView view)
        {
            Debug.Assert(view != null && View == null);

            EnsureElementsCreated(view);

            View = view;

            var repeatItems = view.Owner.Template.RepeatItems;
            Debug.Assert(Elements.Count == repeatItems.Count);
            for (int i = 0; i < Elements.Count; i++)
            {
                var element = Elements[i];
                element.SetRowView(view);
                element.SetDataView(view.Owner);
                repeatItems[i].Initialize(element);
            }

            view.BindingsReset += OnBindingsReset;
        }

        private void EnsureElementsCreated(RowView view)
        {
            if (_elementsCreated)
                return;

            ApplyTemplate();
            var repeatItems = view.Owner.Template.RepeatItems;
            for (int i = 0; i < repeatItems.Count; i++)
                _elements.Add(repeatItems[i].Generate());
            _elementsCreated = true;
        }

        private void OnBindingsReset(object sender, System.EventArgs e)
        {
            var repeatItems = View.Owner.Template.RepeatItems;
            for (int i = 0; i < repeatItems.Count; i++)
                repeatItems[i].UpdateTarget(Elements[i]);
        }

        internal void Cleanup()
        {
            Debug.Assert(View != null);

            View.BindingsReset -= OnBindingsReset;

            var repeatItems = View.Owner.Template.RepeatItems;
            Debug.Assert(Elements.Count == repeatItems.Count);
            for (int i = 0; i < Elements.Count; i++)
            {
                var element = Elements[i];
                repeatItems[i].Cleanup(element);
                element.SetRowView(null);
                element.SetDataView(null);
            }

            View = null;
        }

        //protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        //{
        //    base.OnPreviewLostKeyboardFocus(e);
        //}

        //protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        //{
        //    base.OnLostKeyboardFocus(e);
        //    if (View != null)
        //        View.IsFocused = false;
        //}

        //protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        //{
        //    base.OnPreviewGotKeyboardFocus(e);
        //    if (View != null)
        //        View.IsFocused = true;
        //}
    }
}
