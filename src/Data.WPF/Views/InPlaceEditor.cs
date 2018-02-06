using System.Windows;
using System;
using System.Windows.Media;
using System.Collections;
using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;

namespace DevZest.Data.Views
{
    public class InPlaceEditor : FrameworkElement, IScalarElement, IRowElement
    {
        public interface ISwitcher : IService
        {
            bool AffectsIsEditing(InPlaceEditor inPlaceEditor, DependencyProperty dp);
            bool GetIsEditing(InPlaceEditor inPlaceEditor);
        }

        private sealed class Switcher : ISwitcher
        {
            public DataPresenter DataPresenter { get; private set; }

            public void Initialize(DataPresenter dataPresenter)
            {
                DataPresenter = dataPresenter;
            }

            public bool AffectsIsEditing(InPlaceEditor inPlaceEditor, DependencyProperty dp)
            {
                return dp == IsMouseOverProperty || dp == IsKeyboardFocusWithinProperty;
            }

            public bool GetIsEditing(InPlaceEditor inPlaceEditor)
            {
                return inPlaceEditor.IsMouseOver || inPlaceEditor.IsKeyboardFocusWithin;
            }
        }

        private static readonly DependencyPropertyKey IsRowEditingPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsRowEditing), typeof(bool), typeof(InPlaceEditor),
            new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty IsRowEditingProperty = IsRowEditingPropertyKey.DependencyProperty;

        static InPlaceEditor()
        {
            ServiceManager.Register<ISwitcher, Switcher>();
        }

        public bool IsRowEditing
        {
            get { return (bool)GetValue(IsRowEditingProperty); }
            private set { SetValue(IsRowEditingProperty, BooleanBoxes.Box(value)); }
        }

        public bool IsEditing { get; private set; }

        private UIElement _inertElement;
        public UIElement InertElement
        {
            get { return _inertElement;  }
            private set
            {
                if (_inertElement == value)
                    return;

                var oldValue = _inertElement;
                _inertElement = value;
                OnChildChanged(oldValue, value);
            }
        }

        private void OnChildChanged(UIElement oldValue, UIElement newValue)
        {
            if (oldValue != null)
            {
                RemoveLogicalChild(oldValue);
                RemoveVisualChild(oldValue);
            }
            if (newValue != null)
            {
                AddLogicalChild(newValue);
                AddVisualChild(newValue);
            }
        }

        private UIElement _editorElement;
        public UIElement EditorElement
        {
            get { return _editorElement; }
            private set
            {
                if (_editorElement == value)
                    return;

                var oldValue = _editorElement;
                _editorElement = value;
                OnChildChanged(oldValue, value);
            }
        }

        public UIElement Child
        {
            get { return IsEditing ? EditorElement : InertElement; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= VisualChildrenCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            return Child;
        }

        protected override IEnumerator LogicalChildren
        {
            get
            {
                var child = Child;
                return child == null ? EmptyEnumerator.Singleton : new SingleChildEnumerator(child);
            }
        }

        protected override int VisualChildrenCount
        {
            get { return Child == null ? 0 : 1; }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            UIElement child = Child;
            if (child != null)
            {
                child.Measure(constraint);
                return child.DesiredSize;
            }
            return default(Size);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            UIElement child = Child;
            if (child != null)
                child.Arrange(new Rect(arrangeSize));
            return arrangeSize;
        }

        private DataView DataView
        {
            get { return DataView.GetCurrent(this); }
        }

        private DataPresenter DataPresenter
        {
            get { return DataView?.DataPresenter; }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            var switcher = DataPresenter?.GetService<ISwitcher>();
            if (switcher != null)
            {
                if (switcher.AffectsIsEditing(this, e.Property))
                    IsEditing = switcher.GetIsEditing(this);
            }
        }

        void IScalarElement.Setup(ScalarPresenter scalarPresenter)
        {
            throw new NotImplementedException();
        }

        void IScalarElement.Refresh(ScalarPresenter scalarPresenter)
        {
        }

        void IScalarElement.Cleanup(ScalarPresenter scalarPresenter)
        {
            throw new NotImplementedException();
        }

        void IRowElement.Setup(RowPresenter rowPresenter)
        {
            throw new NotImplementedException();
        }

        void IRowElement.Refresh(RowPresenter rowPresenter)
        {
            IsRowEditing = rowPresenter.IsEditing;
        }

        void IRowElement.Cleanup(RowPresenter rowPresenter)
        {
            throw new NotImplementedException();
        }

    }
}
