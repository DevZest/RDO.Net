using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public class DataView : Control
    {
        private static readonly DependencyPropertyKey DataPresenterPropertyKey = DependencyProperty.RegisterReadOnly(nameof(DataPresenter),
            typeof(DataPresenter), typeof(DataView), new FrameworkPropertyMetadata(null, OnPresenterChanged));

        public static readonly DependencyProperty DataPresenterProperty = DataPresenterPropertyKey.DependencyProperty;

        public static readonly DependencyProperty ScrollableProperty = DependencyProperty.Register(nameof(Scrollable),
            typeof(bool), typeof(DataView), new FrameworkPropertyMetadata(BooleanBoxes.True));

        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(HorizontalScrollBarVisibility),
            typeof(ScrollBarVisibility), typeof(DataView), new PropertyMetadata(ScrollBarVisibility.Auto));

        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(VerticalScrollBarVisibility),
            typeof(ScrollBarVisibility), typeof(DataView), new FrameworkPropertyMetadata(ScrollBarVisibility.Auto));

        public static readonly DependencyProperty ScrollLineHeightProperty = DependencyProperty.Register(nameof(ScrollLineHeight),
            typeof(double), typeof(DataView), new FrameworkPropertyMetadata(20.0d));

        public static readonly DependencyProperty ScrollLineWidthProperty = DependencyProperty.Register(nameof(ScrollLineWidth),
            typeof(double), typeof(DataView), new FrameworkPropertyMetadata(20.0d));

        static DataView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataView), new FrameworkPropertyMetadata(typeof(DataView)));
        }


        private static void OnPresenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (DataPresenter)e.OldValue;
            var newValue = (DataPresenter)e.NewValue;
            ((DataView)d).OnDataPresenterChanged(oldValue, newValue);
        }

        private void OnDataPresenterChanged(DataPresenter oldValue, DataPresenter newValue)
        {
        }

        public DataPresenter DataPresenter
        {
            get { return (DataPresenter)GetValue(DataPresenterProperty); }
            private set { SetValue(DataPresenterPropertyKey, value); }
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

        internal void Initialize(DataPresenter dataPresenter)
        {
            Debug.Assert(dataPresenter != null && DataPresenter == null);
            DataPresenter = dataPresenter;
        }

        internal void Cleanup()
        {
            Debug.Assert(DataPresenter != null);
            DataPresenter = null;
        }
    }
}
