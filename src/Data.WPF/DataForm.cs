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
        private static readonly DependencyPropertyKey PresenterPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Presenter),
            typeof(DataPresenter), typeof(DataForm), new FrameworkPropertyMetadata(null, OnPresenterChanged));

        public static readonly DependencyProperty PresenterProperty = PresenterPropertyKey.DependencyProperty;

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


        private static void OnPresenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (DataPresenter)e.OldValue;
            var newValue = (DataPresenter)e.NewValue;
            ((DataForm)d).OnPresenterChanged(oldValue, newValue);
        }

        private void OnPresenterChanged(DataPresenter oldValue, DataPresenter newValue)
        {
        }

        public DataPresenter Presenter
        {
            get { return (DataPresenter)GetValue(PresenterProperty); }
            private set { SetValue(PresenterPropertyKey, value); }
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

        public void Show(DataPresenter presenter)
        {
            if (presenter == null)
                throw new ArgumentNullException(nameof(presenter));

            Presenter = presenter;
        }

        internal void Cleanup()
        {
            Debug.Assert(Presenter != null);
            Presenter = null;
        }
    }
}
