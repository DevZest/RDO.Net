using DevZest.Data.Presenters.Primitives;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections;
using DevZest.Data.Views.Primitives;
using System.Diagnostics;

namespace DevZest.Data.Views
{
    public class InPlaceEditor : ContentControl, ICompositeView
    {
        private sealed class BindingDispatcher : CompositeBindingDispatcher, IReadOnlyList<UIElement>
        {
            public BindingDispatcher(InPlaceEditor view)
            {
                Debug.Assert(view != null);
                _view = view;
            }

            private readonly InPlaceEditor _view;
            protected override ICompositeView View
            {
                get { return _view; }
            }

            private FrameworkElement Element
            {
                get { return _view.Element; }
            }

            private FrameworkElement EditingElement
            {
                get { return _view.EditingElement; }
            }

            private Binding ElementBinding
            {
                get { return Bindings[0]; }
            }

            private TwoWayBinding EditingElementBinding
            {
                get { return (TwoWayBinding)Bindings[1]; }
            }

            #region IReadOnlyList<UIElement>
            int IReadOnlyCollection<UIElement>.Count
            {
                get { return 2; }
            }

            UIElement IReadOnlyList<UIElement>.this[int index]
            {
                get
                {
                    if (index < 0 || index > 1)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    return index == 0 ? Element : EditingElement;
                }
            }

            IEnumerator<UIElement> IEnumerable<UIElement>.GetEnumerator()
            {
                yield return Element;
                yield return EditingElement;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                yield return Element;
                yield return EditingElement;
            }

            #endregion

            public override IReadOnlyList<UIElement> Children
            {
                get { return this; }
            }

            private bool IsVisible(int index)
            {
                return index == (_view.IsEditing ? 1 : 0);
            }

            public void OnIsEditingChanged()
            {
                var newValue = _view.IsEditing;
                var oldValue = !newValue;
            }

            protected override void Initialize(int index, Binding binding, string name)
            {
                Debug.Assert(index == 0 || index == 1);
                if (IsVisible(index))
                    base.Initialize(index, binding, name);
            }

            protected override void AddChild(UIElement child, string name)
            {
                _view.SetContent((FrameworkElement)child);
            }

            protected override void BeginSetup(int index, Binding binding, UIElement element)
            {
                if (IsVisible(index))
                    base.BeginSetup(index, binding, element);
            }

            protected override void EndSetup(int index, Binding binding)
            {
                if (IsVisible(index))
                    base.EndSetup(index, binding);
            }

            protected override void Refresh(int index, Binding binding, UIElement element)
            {
                if (IsVisible(index))
                    base.Refresh(index, binding, element);
            }

            protected override void Cleanup(int index, Binding binding, UIElement element)
            {
                if (IsVisible(index))
                    base.Cleanup(index, binding, element);
            }

            protected override void FlushInput(int index, TwoWayBinding binding, UIElement element)
            {
                if (IsVisible(index))
                    base.FlushInput(index, binding, element);
            }
        }

        private static readonly DependencyPropertyKey IsEditingPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsEditing), typeof(bool), typeof(InPlaceEditor),
            new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty IsEditingProperty = IsEditingPropertyKey.DependencyProperty;

        public InPlaceEditor()
        {
            _bindingDispatcher = new BindingDispatcher(this);
        }

        private readonly BindingDispatcher _bindingDispatcher;

        CompositeBindingDispatcher ICompositeView.BindingDispatcher
        {
            get { return _bindingDispatcher; }
        }

        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            private set { SetValue(IsEditingPropertyKey, BooleanBoxes.Box(value)); }
        }

        public FrameworkElement Element
        {
            get { return IsEditing ? null : Content as FrameworkElement; }
        }

        public FrameworkElement EditingElement
        {
            get { return IsEditing ? Content as FrameworkElement : null; }
        }

        private Style _elementStyle;
        public Style ElementStyle
        {
            get { return _elementStyle; }
            set
            {
                _elementStyle = value;
                var element = Element;
                if (element != null)
                    element.Style = value;
            }
        }

        private Style _editingElementStyle;
        public Style EditingElementStyle
        {
            get { return _editingElementStyle; }
            set
            {
                _editingElementStyle = value;
                var editingElement = EditingElement;
                if (editingElement != null)
                    editingElement.Style = value;
            }
        }

        private void SetContent(FrameworkElement element)
        {
            element.Style = IsEditing ? EditingElementStyle : ElementStyle;
            Content = element;
        }
    }
}
