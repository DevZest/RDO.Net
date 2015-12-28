using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public class DataSetView : Control
    {
        private static readonly DependencyPropertyKey PresenterPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Presenter),
            typeof(DataSetPresenter), typeof(DataSetView), new FrameworkPropertyMetadata(null, OnPresenterChanged));

        public static readonly DependencyProperty PresenterProperty = PresenterPropertyKey.DependencyProperty;

        public static readonly DependencyProperty ScrollableProperty = DependencyProperty.Register(nameof(Scrollable),
            typeof(bool), typeof(DataSetView), new FrameworkPropertyMetadata(BooleanBoxes.True));

        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(HorizontalScrollBarVisibility),
            typeof(ScrollBarVisibility), typeof(DataSetView), new PropertyMetadata(ScrollBarVisibility.Auto));

        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(VerticalScrollBarVisibility),
            typeof(ScrollBarVisibility), typeof(DataSetView), new FrameworkPropertyMetadata(ScrollBarVisibility.Auto));

        public static readonly DependencyProperty ScrollLineHeightProperty = DependencyProperty.Register(nameof(ScrollLineHeight),
            typeof(double), typeof(DataSetView), new FrameworkPropertyMetadata(20.0d));

        public static readonly DependencyProperty ScrollLineWidthProperty = DependencyProperty.Register(nameof(ScrollLineWidth),
            typeof(double), typeof(DataSetView), new FrameworkPropertyMetadata(20.0d));

        static DataSetView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataSetView), new FrameworkPropertyMetadata(typeof(DataSetView)));
        }


        private static void OnPresenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (DataSetPresenter)e.OldValue;
            var newValue = (DataSetPresenter)e.NewValue;
            ((DataSetView)d).OnPresenterChanged(oldValue, newValue);
        }

        private void OnPresenterChanged(DataSetPresenter oldValue, DataSetPresenter newValue)
        {
            if (oldValue != null)
                oldValue.View = null;

            Debug.Assert(newValue != null);
            newValue.View = this;
        }

        public DataSetPresenter Presenter
        {
            get { return (DataSetPresenter)GetValue(PresenterProperty); }
            internal set { SetValue(PresenterPropertyKey, value); }
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

        public void Show(DataSetPresenter presenter)
        {
            if (presenter == null)
                throw new ArgumentNullException(nameof(presenter));

            Presenter = presenter;
        }
    }
}
