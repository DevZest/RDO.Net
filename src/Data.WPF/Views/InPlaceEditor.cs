using DevZest.Data.Presenters.Primitives;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections;
using DevZest.Data.Views.Primitives;
using System.Diagnostics;
using DevZest.Data.Presenters;

namespace DevZest.Data.Views
{
    public class InPlaceEditor : ContentControl, ICompositeView, IScalarElement, IRowElement
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

            private bool IsEditing
            {
                get { return _view.IsEditing; }
                set { _view.IsEditing = value; }
            }

            private int ActiveBindingIndex
            {
                get { return IsEditing ? 1 : 0; }
            }

            protected override void Initialize(int index, Binding binding, string name)
            {
                if (index < 0 || index > 1)
                    throw new ArgumentOutOfRangeException(nameof(index));
                if (index == ActiveBindingIndex)
                    base.Initialize(index, binding, name);
            }

            protected override void AddChild(UIElement child, string name)
            {
                _view.SetContent((FrameworkElement)child);
            }

            protected override void BeginSetup(int index, Binding binding, UIElement element)
            {
                if (index == ActiveBindingIndex)
                    base.BeginSetup(index, binding, element);
            }

            protected override void EndSetup(int index, Binding binding)
            {
                if (index == ActiveBindingIndex)
                    base.EndSetup(index, binding);
            }

            protected override void Refresh(int index, Binding binding, UIElement element)
            {
                if (index == ActiveBindingIndex)
                    base.Refresh(index, binding, element);
            }

            protected override void Cleanup(int index, Binding binding, UIElement element)
            {
                if (index == ActiveBindingIndex)
                    base.Cleanup(index, binding, element);
            }

            protected override void FlushInput(int index, TwoWayBinding binding, UIElement element)
            {
                if (index == ActiveBindingIndex)
                    base.FlushInput(index, binding, element);
            }

            public void BeginEdit()
            {
                Debug.Assert(!IsEditing);
                Debug.Assert(Element != null);

                ElementBinding.Cleanup(Element);
                base.Initialize(1, EditingElementBinding, Names[1]);
                IsEditing = true;
                Setup(_view);
                base.EndSetup(1, EditingElementBinding);
            }

            public void CancelEdit()
            {
                Debug.Assert(_view.IsEditing);
                EditingElementBinding.Cleanup(EditingElement);
                base.Initialize(0, ElementBinding, Names[0]);
                IsEditing = false;
                Setup(_view);
                base.EndSetup(0, ElementBinding);
            }

            public void EndEdit()
            {
                base.FlushInput(1, EditingElementBinding, EditingElement);
                CancelEdit();
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
            if (element != null)
                element.Style = IsEditing ? EditingElementStyle : ElementStyle;
            Content = element;
        }

        public void BeginEdit()
        {
            if (IsEditing)
                return;

            _bindingDispatcher.BeginEdit();
        }

        public bool CanEndEdit
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool EndEdit()
        {
            if (!CanEndEdit)
                return false;

            _bindingDispatcher.EndEdit();
            return true;
        }

        public void CancelEdit()
        {
            if (!IsEditing)
                return;

            _bindingDispatcher.CancelEdit();
        }

        void IScalarElement.Setup(ScalarPresenter scalarPresenter)
        {
        }

        void IScalarElement.Refresh(ScalarPresenter scalarPresenter)
        {
            if (Element != null)
            {
                throw new NotImplementedException();
            }
        }

        void IScalarElement.Cleanup(ScalarPresenter scalarPresenter)
        {
        }

        void IRowElement.Setup(RowPresenter rowPresenter)
        {
        }

        void IRowElement.Refresh(RowPresenter rowPresenter)
        {
            if (Element != null)
            {
                throw new NotImplementedException();
            }
        }

        void IRowElement.Cleanup(RowPresenter rowPresenter)
        {
        }
    }
}
