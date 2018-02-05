using DevZest.Data.Presenters.Primitives;
using System.Windows;
using System.Windows.Controls;
using System;
using DevZest.Data.Presenters;
using System.Diagnostics;

namespace DevZest.Data.Views
{
    public class InPlaceEditor : ContentControl
    {
        private static readonly DependencyPropertyKey InertElementPropertyKey = DependencyProperty.RegisterReadOnly(nameof(InertElement), typeof(UIElement), typeof(InPlaceEditor),
            new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty InertElementProperty = InertElementPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey InertElementVisibilityPropertyKey = DependencyProperty.RegisterReadOnly(nameof(InertElementVisibility), typeof(Visibility),
            typeof(InPlaceEditor), new FrameworkPropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty InertElementVisibilityProperty = InertElementVisibilityPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey EditorElementPropertyKey = DependencyProperty.RegisterReadOnly(nameof(EditorElement), typeof(UIElement), typeof(InPlaceEditor),
            new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty EditorElementProperty = EditorElementPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey EditorElementVisibilityPropertyKey = DependencyProperty.RegisterReadOnly(nameof(EditorElementVisibility), typeof(Visibility),
            typeof(InPlaceEditor), new FrameworkPropertyMetadata(Visibility.Collapsed));
        public static readonly DependencyProperty EditorElementVisibilityProperty = EditorElementVisibilityPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey IsEditingPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsEditing), typeof(bool), typeof(InPlaceEditor),
            new FrameworkPropertyMetadata(BooleanBoxes.False, new PropertyChangedCallback(_OnIsEditingChanged)));
        public static readonly DependencyProperty IsEditingProperty = IsEditingPropertyKey.DependencyProperty;

        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            private set { SetValue(IsEditingPropertyKey, BooleanBoxes.Box(value)); }
        }

        private static void _OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InPlaceEditor)d).OnIsEditingChanged();
        }

        private void OnIsEditingChanged()
        {
            var rowPresenter = this.GetRowPresenter();
            if (rowPresenter != null)
                rowPresenter.DataPresenter.InvalidateView();
        }

        public UIElement InertElement
        {
            get { return (UIElement)GetValue(InertElementProperty);  }
            private set { SetValue(InertElementPropertyKey, value); }
        }

        public Visibility InertElementVisibility
        {
            get { return (Visibility)GetValue(InertElementVisibilityProperty); }
            private set { SetValue(InertElementVisibilityPropertyKey, value); }
        }

        public UIElement EditorElement
        {
            get { return (UIElement)GetValue(EditorElementProperty); }
            private set { SetValue(EditorElementPropertyKey, value); }
        }

        public Visibility EditorElementVisibility
        {
            get { return (Visibility)GetValue(EditorElementVisibilityProperty); }
            private set { SetValue(EditorElementPropertyKey, value); }
        }

        internal void Setup(UIElement inertElement, UIElement editorElement)
        {
            Debug.Assert(inertElement != null);
            Debug.Assert(editorElement != null);
            InertElement = inertElement;
            EditorElement = editorElement;
            System.Windows.Controls.Validation.SetValidationAdornerSiteFor(EditorElement, this);
        }

        internal void Cleanup()
        {
            System.Windows.Controls.Validation.SetValidationAdornerSiteFor(EditorElement, null);
            InertElement = EditorElement = null;
        }

        public void BeginEdit()
        {
            if (IsEditing)
                return;

            throw new NotImplementedException();
        }

        public bool CanEndEdit
        {
            get
            {
                //return IsEditing ? _bindingDispatcher.CanEndEdit : false;
                throw new NotImplementedException();
            }
        }

        public bool EndEdit()
        {
            if (!CanEndEdit)
                return false;

            //_bindingDispatcher.EndEdit();
            //return true;
            throw new NotImplementedException();
        }

        public void CancelEdit()
        {
            if (!IsEditing)
                return;

            //_bindingDispatcher.CancelEdit();
            throw new NotImplementedException();
        }
    }
}
