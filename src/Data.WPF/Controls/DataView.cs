using DevZest.Windows.Controls.Primitives;
using DevZest.Windows.Data;
using DevZest.Windows.Data.Primitives;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Windows.Controls
{
    [TemplatePart(Name = "PART_Panel", Type = typeof(DataViewPanel))]
    public class DataView : Control
    {
        private static readonly DependencyPropertyKey DataPresenterPropertyKey = DependencyProperty.RegisterReadOnly(nameof(DataPresenter),
            typeof(DataPresenter), typeof(DataView), new FrameworkPropertyMetadata(null, OnDataPresenterChanged));

        public static readonly DependencyProperty DataPresenterProperty = DataPresenterPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ScrollablePropertyKey = DependencyProperty.RegisterReadOnly(nameof(Scrollable),
            typeof(bool), typeof(DataView), new FrameworkPropertyMetadata(BooleanBoxes.False));

        public static readonly DependencyProperty ScrollableProperty = ScrollablePropertyKey.DependencyProperty;

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
            FocusableProperty.OverrideMetadata(typeof(DataView), new FrameworkPropertyMetadata(BooleanBoxes.False));
            //KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(DataView), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(DataView), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
        }

        public DataPresenter DataPresenter
        {
            get { return (DataPresenter)GetValue(DataPresenterProperty); }
            internal set
            {
                Debug.Assert((value == null) != (DataPresenter == null));

                SetValue(DataPresenterPropertyKey, value);
                if (value != null)
                    Scrollable = value.Template.Orientation.HasValue;
                else
                    ClearValue(ScrollablePropertyKey);
            }
        }

        private LayoutManager LayoutManager
        {
            get { return DataPresenter == null ? null : DataPresenter.LayoutManager; }
        }

        private static void OnDataPresenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DataView)d).SetElementsPanel();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            SetElementsPanel();
        }

        private void SetElementsPanel()
        {
            if (Template == null)
                return;
            var panel = Template.FindName("PART_Panel", this) as DataViewPanel;
            if (panel == null)
                return;

            var layoutManager = LayoutManager;
            if (layoutManager != null)
                layoutManager.SetElementsPanel(panel);
        }

        public bool Scrollable
        {
            get { return (bool)GetValue(ScrollableProperty); }
            private set { SetValue(ScrollablePropertyKey, BooleanBoxes.Box(value)); }
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

        internal void SetupCommandEntries()
        {
            var dataPresenter = DataPresenter;
            if (dataPresenter == null)
                return;

            this.SetupCommandEntries(dataPresenter.DataViewCommandEntries);
        }
    }
}
