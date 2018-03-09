using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    public class GridCell : Control
    {
        private static readonly DependencyPropertyKey ChildPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Child), typeof(UIElement), typeof(GridCell),
            new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ChildProperty = ChildPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ModePropertyKey = DependencyProperty.RegisterAttachedReadOnly(nameof(Mode), typeof(GridCellMode?), typeof(GridCell),
            new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ModeProperty = ModePropertyKey.DependencyProperty;

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

        public GridCellMode? Mode
        {
            get { return (GridCellMode?)GetValue(ModeProperty); }
            private set { SetValue(ModePropertyKey, value); }
        }

        public static GridCellMode? GetMode(UIElement element)
        {
            return (GridCellMode?)element.GetValue(ModeProperty);
        }
    }
}
