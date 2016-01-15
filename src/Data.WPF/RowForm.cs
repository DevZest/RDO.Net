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
        private IReadOnlyList<UIElement> Elements
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

            var listUnits = view.Owner.Template.ListUnits;
            Debug.Assert(Elements.Count == listUnits.Count);
            for (int i = 0; i < Elements.Count; i++)
            {
                var element = Elements[i];
                element.SetRowView(view);
                element.SetDataView(view.Owner);
                listUnits[i].Initialize(element);
            }

            view.BindingsReset += OnBindingsReset;
        }

        private void EnsureElementsCreated(RowView view)
        {
            if (_elementsCreated)
                return;

            ApplyTemplate();
            var listUnits = view.Owner.Template.ListUnits;
            for (int i = 0; i < listUnits.Count; i++)
                _elements.Add(listUnits[i].Generate());
            _elementsCreated = true;
        }

        private void OnBindingsReset(object sender, System.EventArgs e)
        {
            var listUnits = View.Owner.Template.ListUnits;
            for (int i = 0; i < listUnits.Count; i++)
                listUnits[i].UpdateTarget(Elements[i]);
        }

        internal void Cleanup()
        {
            Debug.Assert(View != null);

            View.BindingsReset -= OnBindingsReset;

            var listUnits = View.Owner.Template.ListUnits;
            Debug.Assert(Elements.Count == listUnits.Count);
            for (int i = 0; i < Elements.Count; i++)
            {
                var element = Elements[i];
                listUnits[i].Cleanup(element);
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
