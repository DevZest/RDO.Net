using DevZest.Data.Windows.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Windows
{
    [TemplatePart(Name = PART_RowPanel, Type = typeof(RowPanel))]
    public class RowView : Control
    {
        private const string PART_RowPanel = "PART_RowPanel";

        public RowPresenter RowPresenter { get; private set; }

        private bool _elementsCreated;
        private IElementCollection _elements;
        internal IReadOnlyList<UIElement> Elements
        {
            get { return _elements; }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Debug.Assert(RowPresenter == null);
            var rowPanel = GetTemplateChild(PART_RowPanel) as RowPanel;
            _elements = rowPanel == null ? IElementCollectionFactory.Create(null) : rowPanel.ElementCollection;
            _elementsCreated = false;
        }

        internal void Initialize(RowPresenter rowPresenter)
        {
            Debug.Assert(rowPresenter != null && RowPresenter == null);

            EnsureElementsCreated(rowPresenter);

            RowPresenter = rowPresenter;

            var repeatItems = rowPresenter.DataPresenter.Template.RepeatItems;
            Debug.Assert(Elements.Count == repeatItems.Count);
            for (int i = 0; i < Elements.Count; i++)
            {
                var element = Elements[i];
                element.SetRowPresenter(rowPresenter);
                element.SetDataPresenter(rowPresenter.DataPresenter);
                repeatItems[i].Initialize(element);
            }

            rowPresenter.BindingsReset += OnBindingsReset;
        }

        private void EnsureElementsCreated(RowPresenter rowPresenter)
        {
            if (_elementsCreated)
                return;

            ApplyTemplate();
            var repeatItems = rowPresenter.DataPresenter.Template.RepeatItems;
            for (int i = 0; i < repeatItems.Count; i++)
                _elements.Add(repeatItems[i].Generate());
            _elementsCreated = true;
        }

        private void OnBindingsReset(object sender, System.EventArgs e)
        {
            var repeatItems = RowPresenter.DataPresenter.Template.RepeatItems;
            for (int i = 0; i < repeatItems.Count; i++)
                repeatItems[i].UpdateTarget(Elements[i]);
        }

        internal void Cleanup()
        {
            Debug.Assert(RowPresenter != null);

            RowPresenter.BindingsReset -= OnBindingsReset;

            var repeatItems = RowPresenter.DataPresenter.Template.RepeatItems;
            Debug.Assert(Elements.Count == repeatItems.Count);
            for (int i = 0; i < Elements.Count; i++)
            {
                var element = Elements[i];
                repeatItems[i].Cleanup(element);
                element.SetRowPresenter(null);
                element.SetDataPresenter(null);
            }

            RowPresenter = null;
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
