using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public class DataForm : Control
    {
        private static readonly DependencyPropertyKey ViewPropertyKey = DependencyProperty.RegisterReadOnly(nameof(View),
            typeof(DataView), typeof(DataForm), new FrameworkPropertyMetadata(null, OnViewChanged));

        public static readonly DependencyProperty ViewProperty = ViewPropertyKey.DependencyProperty;

        public static readonly DependencyProperty ScrollableProperty = DependencyProperty.Register(nameof(Scrollable),
            typeof(bool), typeof(DataForm), new FrameworkPropertyMetadata(BooleanBoxes.True));

        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(HorizontalScrollBarVisibility),
            typeof(ScrollBarVisibility), typeof(DataForm), new PropertyMetadata(ScrollBarVisibility.Auto));

        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(VerticalScrollBarVisibility),
            typeof(ScrollBarVisibility), typeof(DataForm), new FrameworkPropertyMetadata(ScrollBarVisibility.Auto));

        public static readonly DependencyProperty ScrollLineHeightProperty = DependencyProperty.Register(nameof(ScrollLineHeight),
            typeof(double), typeof(DataForm), new FrameworkPropertyMetadata(20.0d));

        public static readonly DependencyProperty ScrollLineWidthProperty = DependencyProperty.Register(nameof(ScrollLineWidth),
            typeof(double), typeof(DataForm), new FrameworkPropertyMetadata(20.0d));

        static DataForm()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataForm), new FrameworkPropertyMetadata(typeof(DataForm)));
        }


        private static void OnViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (DataView)e.OldValue;
            var newValue = (DataView)e.NewValue;
            ((DataForm)d).OnViewChanged(oldValue, newValue);
        }

        private void OnViewChanged(DataView oldValue, DataView newValue)
        {
        }

        public DataView View
        {
            get { return (DataView)GetValue(ViewProperty); }
            private set { SetValue(ViewPropertyKey, value); }
        }

        public bool Scrollable
        {
            get { return (bool)GetValue(ScrollableProperty); }
            set { SetValue(ScrollableProperty, value); }
        }

        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
            set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
        }

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); }
            set { SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        public double ScrollLineHeight
        {
            get { return (double)GetValue(ScrollLineHeightProperty); }
            set { SetValue(ScrollLineHeightProperty, value); }
        }

        public double ScrollLineWidth
        {
            get { return (double)GetValue(ScrollLineWidthProperty); }
            set { SetValue(ScrollLineWidthProperty, value); }
        }

        public void Show(DataView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            View = view;
        }

        internal void Cleanup()
        {
            Debug.Assert(View != null);
            View = null;
        }
    }
}
