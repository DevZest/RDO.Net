using DevZest.Data.Views.Primitives;
using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using DevZest.Windows;

namespace DevZest.Data.Views
{
    /// <summary>
    /// Represents a control that displays scalar and DataSet data.
    /// </summary>
    /// <seealso cref="DataView.Commands"/>
    /// <seealso cref="DataView.ICommandService"/>
    /// <seealso cref="DataView.IPasteAppendService"/>
    /// <seealso cref="BindingFactory.BindToDataView{T}(T, Func{DataPresenter{T}})"/>
    [TemplatePart(Name = "PART_Panel", Type = typeof(DataViewPanel))]
    public partial class DataView : Control, IBaseView
    {
        /// <summary>
        /// Gets current <see cref="DataView"/> which owns specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="target">The specified <see cref="DependencyObject"/>.</param>
        /// <returns>The current <see cref="DataView"/>.</returns>
        public static DataView GetCurrent(DependencyObject target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            return target.FindVisaulAncestor<DataView>();
        }

        private static readonly DependencyPropertyKey ScrollablePropertyKey = DependencyProperty.RegisterReadOnly(nameof(Scrollable),
            typeof(bool), typeof(DataView), new FrameworkPropertyMetadata(BooleanBoxes.False));

        /// <summary>
        /// Identifies the <see cref="Scrollable"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ScrollableProperty = ScrollablePropertyKey.DependencyProperty;

        /// <summary>
        /// Identifies the <see cref="HorizontalScrollBarVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(HorizontalScrollBarVisibility),
            typeof(ScrollBarVisibility), typeof(DataView), new PropertyMetadata(ScrollBarVisibility.Auto));

        /// <summary>
        /// Identifies the <see cref="VerticalScrollBarVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(VerticalScrollBarVisibility),
            typeof(ScrollBarVisibility), typeof(DataView), new FrameworkPropertyMetadata(ScrollBarVisibility.Auto));

        /// <summary>
        /// Identifies the <see cref="ScrollLineHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ScrollLineHeightProperty = DependencyProperty.Register(nameof(ScrollLineHeight),
            typeof(double), typeof(DataView), new FrameworkPropertyMetadata(20.0d));

        /// <summary>
        /// Identifies the <see cref="ScrollLineWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ScrollLineWidthProperty = DependencyProperty.Register(nameof(ScrollLineWidth),
            typeof(double), typeof(DataView), new FrameworkPropertyMetadata(20.0d));

        private static readonly DependencyPropertyKey DataLoadStatePropertyKey = DependencyProperty.RegisterReadOnly(nameof(DataLoadState), typeof(DataLoadState), typeof(DataView),
            new FrameworkPropertyMetadata(DataLoadState.Idle));

        /// <summary>
        /// Identifies the <see cref="DataLoadState"/> dependency Property.
        /// </summary>
        public static readonly DependencyProperty DataLoadStateProperty = DataLoadStatePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey DataLoadErrorPropertyKey = DependencyProperty.RegisterReadOnly(nameof(DataLoadError), typeof(string), typeof(DataView),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="DataLoadError"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DataLoadErrorProperty = DataLoadErrorPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey DataLoadCancellablePropertyKey = DependencyProperty.RegisterReadOnly(nameof(DataLoadCancellable), typeof(bool),
            typeof(DataView), new FrameworkPropertyMetadata(BooleanBoxes.False));

        /// <summary>
        /// Identifies the <see cref="DataLoadCancellable"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty dataLoadCancellableProperty = DataLoadCancellablePropertyKey.DependencyProperty;

        static DataView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataView), new FrameworkPropertyMetadata(typeof(DataView)));
            FocusableProperty.OverrideMetadata(typeof(DataView), new FrameworkPropertyMetadata(BooleanBoxes.False));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(DataView), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(DataView), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
            ServiceManager.Register<ICommandService, CommandService>();
        }

        private DataPresenter _dataPresenter;
        /// <summary>
        /// Gets the <see cref="DataPresenter"/> which is associated with this DataView.
        /// </summary>
        public DataPresenter DataPresenter
        {
            get { return _dataPresenter; }
            internal set
            {
                Debug.Assert((value == null) != (DataPresenter == null));

                _dataPresenter = value;
                DataLoadState = DataLoadState.Idle;
                DataLoadError = null;
                if (value == null)
                {
                    ClearValue(ScrollablePropertyKey);
                    this.CleanupCommandEntries();
                }
                else
                    this.SetupCommandEntries(GetCommandService(value), GetCommandEntries);
            }
        }

        private static IEnumerable<CommandEntry> GetCommandEntries(ICommandService commandService, DataView dataView)
        {
            return commandService.GetCommandEntries(dataView);
        }

        internal void ResetDataLoadState()
        {
            DataLoadState = DataLoadState.Idle;
            DataLoadError = null;
            DataLoadCancellable = false;
        }

        internal void OnDataLoading(bool cancellable)
        {
            DataLoadState = DataLoadState.Loading;
            DataLoadError = null;
            DataLoadCancellable = cancellable;
        }

        internal void OnDataLoaded()
        {
            Debug.Assert(DataPresenter != null);
            DataLoadState = DataLoadState.Succeeded;
            DataLoadError = null;
            Scrollable = DataPresenter.Template.Orientation.HasValue;
            SetElementsPanel();
        }

        internal void OnDataLoadCancelling()
        {
            DataLoadState = DataLoadState.Cancelling;
            DataLoadError = null;
            DataLoadCancellable = false;
        }

        internal void OnDataLoadCancelled()
        {
            DataLoadState = DataLoadState.Cancelled;
            DataLoadError = null;
            DataLoadCancellable = false;
        }

        internal void OnDataLoadFailed(string dataLoadError)
        {
            DataLoadState = DataLoadState.Failed;
            DataLoadError = dataLoadError;
            DataLoadCancellable = false;
        }

        private LayoutManager LayoutManager
        {
            get { return DataPresenter == null ? null : DataPresenter.LayoutManager; }
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// Gets a value indicates whether displayed data is scrollable.
        /// </summary>
        public bool Scrollable
        {
            get { return (bool)GetValue(ScrollableProperty); }
            private set { SetValue(ScrollablePropertyKey, BooleanBoxes.Box(value)); }
        }

        /// <summary>
        /// Gets or sets a value to indicate the visibility of horizontal scroll bar.
        /// </summary>
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
            set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value to indicate the visibility of vertical scroll bar.
        /// </summary>
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); }
            set { SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        /// <summary>
        /// Gets or sets the height length when scrolling a line.
        /// </summary>
        public double ScrollLineHeight
        {
            get { return (double)GetValue(ScrollLineHeightProperty); }
            set { SetValue(ScrollLineHeightProperty, value); }
        }

        /// <summary>
        /// Gets or sets the width length when scrolling a line.
        /// </summary>
        public double ScrollLineWidth
        {
            get { return (double)GetValue(ScrollLineWidthProperty); }
            set { SetValue(ScrollLineWidthProperty, value); }
        }

        /// <summary>
        /// Gets the state of data loading.
        /// </summary>
        public DataLoadState DataLoadState
        {
            get { return (DataLoadState)GetValue(DataLoadStateProperty); }
            internal set { SetValue(DataLoadStatePropertyKey, value); }
        }

        /// <summary>
        /// Gets the error of data loading.
        /// </summary>
        public string DataLoadError
        {
            get { return (string)GetValue(DataLoadErrorProperty); }
            internal set { SetValue(DataLoadErrorPropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets a value indicates whether data loading can be cancelled.
        /// </summary>
        public bool DataLoadCancellable
        {
            get { return (bool)GetValue(dataLoadCancellableProperty); }
            set { SetValue(DataLoadCancellablePropertyKey, BooleanBoxes.Box(value)); }
        }

        void IBaseView.RefreshScalarValidation()
        {
            var layoutManager = LayoutManager;
            if (layoutManager != null)
                this.RefreshValidation(layoutManager.GetScalarValidationInfo());
        }

        /// <summary>
        /// Gets the validation information associated with this control.
        /// </summary>
        public ValidationInfo ValidationInfo
        {
            get
            {
                var layoutManager = LayoutManager;
                return layoutManager == null ? ValidationInfo.Empty : layoutManager.GetScalarValidationInfo();
            }
        }

        BasePresenter IBaseView.Presenter
        {
            get { return DataPresenter; }
            set { DataPresenter = (DataPresenter)value; }
        }
    }
}
