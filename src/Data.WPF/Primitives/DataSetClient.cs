using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class DataSetClient : Decorator, IScrollInfo
    {
        public static readonly DependencyProperty ExtentWidthProperty = DependencyProperty.Register(nameof(ExtentWidth),
            typeof(double), typeof(DataSetClient), new FrameworkPropertyMetadata(0.0d, new PropertyChangedCallback(OnScrollInfoChanged)));

        public static readonly DependencyProperty ExtentHeightProperty = DependencyProperty.Register(nameof(ExtentHeight),
            typeof(double), typeof(DataSetClient), new FrameworkPropertyMetadata(0.0d, new PropertyChangedCallback(OnScrollInfoChanged)));

        public static readonly DependencyProperty ViewportWidthProperty = DependencyProperty.Register(nameof(ViewportWidth),
            typeof(double), typeof(DataSetClient), new FrameworkPropertyMetadata(0.0d, new PropertyChangedCallback(OnScrollInfoChanged)));

        public static readonly DependencyProperty ViewportHeightProperty = DependencyProperty.Register(nameof(ViewportHeight),
            typeof(double), typeof(DataSetClient), new FrameworkPropertyMetadata(0.0d, new PropertyChangedCallback(OnScrollInfoChanged)));

        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register(nameof(HorizontalOffset),
            typeof(double), typeof(DataSetClient),
            new FrameworkPropertyMetadata(0.0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnScrollInfoChanged)));

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register(nameof(VerticalOffset),
            typeof(double), typeof(DataSetClient),
            new FrameworkPropertyMetadata(0.0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnScrollInfoChanged)));

        public static readonly DependencyProperty ScrollLineHeightProperty = DependencyProperty.Register(nameof(ScrollLineHeight),
            typeof(double), typeof(DataSetClient), new FrameworkPropertyMetadata(20.0d));

        public static readonly DependencyProperty ScrollLineWidthProperty = DependencyProperty.Register(nameof(ScrollLineWidth),
            typeof(double), typeof(DataSetClient), new FrameworkPropertyMetadata(20.0d));

        static void OnScrollInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DataSetClient)d).InvalidateScrollOwner();
        }

        static DataSetClient()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataSetClient), new FrameworkPropertyMetadata(typeof(DataSetClient)));
        }

        DataSetView DataSetView
        {
            get { return TemplatedParent as DataSetView; }
        }

        DataSetManager Manager
        {
            get { return DataSetView == null ? null : DataSetView.Manager; }
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

        void InvalidateScrollOwner()
        {
            if (_scrollOwner != null)
                _scrollOwner.InvalidateScrollInfo();
        }

        public double ExtentWidth
        {
            get { return (double)GetValue(ExtentWidthProperty); }
            set { SetValue(ExtentWidthProperty, value); }
        }

        public double ExtentHeight
        {
            get { return (double)GetValue(ExtentHeightProperty); }
            set { SetValue(ExtentHeightProperty, value); }
        }

        public double ViewportHeight
        {
            get { return (double)GetValue(ViewportHeightProperty); }
            set { SetValue(ViewportHeightProperty, value); }
        }

        public double ViewportWidth
        {
            get { return (double)GetValue(ViewportWidthProperty); }
            set { SetValue(ViewportWidthProperty, value); }
        }

        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }

        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        ScrollViewer _scrollOwner;
        ScrollViewer IScrollInfo.ScrollOwner
        {
            get { return _scrollOwner; }
            set { _scrollOwner = value; }
        }

        double IScrollInfo.ExtentHeight
        {
            get { return ExtentHeight; }
        }

        double IScrollInfo.ExtentWidth
        {
            get { return ExtentWidth; }
        }

        double IScrollInfo.ViewportHeight
        {
            get { return ViewportHeight; }
        }

        double IScrollInfo.ViewportWidth
        {
            get { return ViewportWidth; }
        }

        double IScrollInfo.VerticalOffset
        {
            get { return VerticalOffset; }
        }

        double IScrollInfo.HorizontalOffset
        {
            get { return HorizontalOffset; }
        }

        bool _canVerticallyScroll;
        bool IScrollInfo.CanVerticallyScroll
        {
            get { return _canVerticallyScroll; }
            set { _canVerticallyScroll = value; }
        }

        bool _canHorizontallyScroll;
        bool IScrollInfo.CanHorizontallyScroll
        {
            get { return _canHorizontallyScroll; }
            set { _canHorizontallyScroll = value; }
        }

        void IScrollInfo.LineUp()
        {
            VerticalOffset -= ScrollLineHeight;
        }

        void IScrollInfo.LineDown()
        {
            VerticalOffset += ScrollLineHeight;
        }

        void IScrollInfo.LineLeft()
        {
            HorizontalOffset -= ScrollLineWidth;
        }

        void IScrollInfo.LineRight()
        {
            HorizontalOffset += ScrollLineWidth;
        }

        void IScrollInfo.PageUp()
        {
            VerticalOffset -= ViewportHeight;
        }

        void IScrollInfo.PageDown()
        {
            VerticalOffset += ViewportHeight;
        }

        void IScrollInfo.PageLeft()
        {
            HorizontalOffset -= ViewportWidth;
        }

        void IScrollInfo.PageRight()
        {
            HorizontalOffset += ViewportWidth;
        }

        void IScrollInfo.MouseWheelUp()
        {
            VerticalOffset -= SystemParameters.WheelScrollLines * ScrollLineHeight;
        }

        void IScrollInfo.MouseWheelDown()
        {
            VerticalOffset += SystemParameters.WheelScrollLines * ScrollLineHeight;
        }

        void IScrollInfo.MouseWheelLeft()
        {
            HorizontalOffset -= SystemParameters.WheelScrollLines * ScrollLineWidth;
        }

        void IScrollInfo.MouseWheelRight()
        {
            HorizontalOffset += SystemParameters.WheelScrollLines * ScrollLineWidth;
        }

        void IScrollInfo.SetHorizontalOffset(double offset)
        {
            HorizontalOffset = offset;
        }

        void IScrollInfo.SetVerticalOffset(double offset)
        {
            VerticalOffset = offset;
        }

        Rect IScrollInfo.MakeVisible(Visual visual, Rect rectangle)
        {
            return Rect.Empty;
        }
    }
}
