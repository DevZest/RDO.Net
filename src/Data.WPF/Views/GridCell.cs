using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    public class GridCell : Control
    {
        public enum Mode
        {
            Select,
            Edit
        }

        private static readonly DependencyPropertyKey ChildPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Child), typeof(UIElement), typeof(GridCell),
            new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ChildProperty = ChildPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsSelectedPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsSelected), typeof(bool), typeof(GridCell),
            new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsEditingPropertyKey = DependencyProperty.RegisterAttachedReadOnly(nameof(IsEditing), typeof(bool), typeof(GridCell),
            new FrameworkPropertyMetadata(BooleanBoxes.False, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty IsEditingProperty = IsEditingPropertyKey.DependencyProperty;

        static GridCell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridCell), new FrameworkPropertyMetadata(typeof(GridCell)));
        }

        public UIElement Child
        {
            get { return (UIElement)GetValue(ChildProperty); }
            private set { SetValue(ChildPropertyKey, value); }
        }

        internal T GetChild<T>()
            where T : UIElement, new()
        {
            if (Child == null)
                Child = new T();
            return (T)Child;
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            private set { SetValue(IsSelectedPropertyKey, value); }
        }

        public static bool GetIsEditing(UIElement element)
        {
            return (bool)element.GetValue(IsEditingProperty);
        }

        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            private set { SetValue(IsEditingPropertyKey, value); }
        }
    }
}
