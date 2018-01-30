using DevZest.Data.Views.Primitives;
using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using System.Collections.Generic;

namespace DevZest.Data.Views
{
    [TemplatePart(Name = "PART_Panel", Type = typeof(DataViewPanel))]
    public class DataView : Control
    {
        public abstract class Commands
        {
            public static readonly RoutedUICommand RetryDataLoad = new RoutedUICommand(UserMessages.DataViewCommands_RetryDataLoadCommandText, nameof(RetryDataLoad), typeof(Commands));
            public static readonly RoutedUICommand CancelDataLoad = new RoutedUICommand(UserMessages.DataViewCommands_CancelDataLoadCommandText, nameof(CancelDataLoad), typeof(Commands));
        }

        public interface ICommandService : IService
        {
            IEnumerable<CommandEntry> GetCommandEntries(DataView dataView);
        }

        private sealed class CommandService : ICommandService
        {
            public DataPresenter DataPresenter { get; private set; }

            public void Initialize(DataPresenter dataPresenter)
            {
                DataPresenter = dataPresenter;
            }

            public IEnumerable<CommandEntry> GetCommandEntries(DataView dataView)
            {
                yield return Commands.CancelDataLoad.Bind(CancelLoadData, CanCancelLoadData);
                yield return Commands.RetryDataLoad.Bind(ReloadData, CanReloadData);
            }

            private void ReloadData(object sender, ExecutedRoutedEventArgs e)
            {
                DataPresenter.Reload();
            }

            private void CanReloadData(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = DataPresenter.CanReload;
            }

            private void CancelLoadData(object sender, ExecutedRoutedEventArgs e)
            {
                DataPresenter.CancelLoading();
            }

            private void CanCancelLoadData(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = DataPresenter.CanCancelLoading;
            }
        }

        private static readonly DependencyPropertyKey CurrentPropertyKey = DependencyProperty.RegisterAttachedReadOnly("Current", typeof(DataView),
            typeof(DataView), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty CurrentProperty = CurrentPropertyKey.DependencyProperty;

        public static DataView GetCurrent(DependencyObject target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            return (DataView)target.GetValue(CurrentProperty);
        }

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

        private static readonly DependencyPropertyKey DataLoadStatePropertyKey = DependencyProperty.RegisterReadOnly(nameof(DataLoadState), typeof(DataLoadState), typeof(DataView),
            new FrameworkPropertyMetadata(DataLoadState.Idle));
        public static readonly DependencyProperty DataLoadStateProperty = DataLoadStatePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey DataLoadErrorPropertyKey = DependencyProperty.RegisterReadOnly(nameof(DataLoadError), typeof(string), typeof(DataView),
            new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty DataLoadErrorProperty = DataLoadErrorPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey DataLoadCancellablePropertyKey = DependencyProperty.RegisterReadOnly(nameof(DataLoadCancellable), typeof(bool),
            typeof(DataView), new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty dataLoadCancellableProperty = DataLoadCancellablePropertyKey.DependencyProperty;

        static DataView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataView), new FrameworkPropertyMetadata(typeof(DataView)));
            FocusableProperty.OverrideMetadata(typeof(DataView), new FrameworkPropertyMetadata(BooleanBoxes.False));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(DataView), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(DataView), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
            ServiceManager.Register<ICommandService, CommandService>();
        }

        public DataView()
        {
            SetValue(CurrentPropertyKey, this);
        }

        private DataPresenter _dataPresenter;
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
                    this.SetupCommandEntries(DataPresenter.GetService<ICommandService>().GetCommandEntries(this));
            }
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

        public DataLoadState DataLoadState
        {
            get { return (DataLoadState)GetValue(DataLoadStateProperty); }
            internal set { SetValue(DataLoadStatePropertyKey, value); }
        }

        public string DataLoadError
        {
            get { return (string)GetValue(DataLoadErrorProperty); }
            internal set { SetValue(DataLoadErrorPropertyKey, value); }
        }

        public bool DataLoadCancellable
        {
            get { return (bool)GetValue(dataLoadCancellableProperty); }
            set { SetValue(DataLoadCancellablePropertyKey, BooleanBoxes.Box(value)); }
        }

        internal void RefreshScalarValidationErrors()
        {
            var layoutManager = LayoutManager;
            if (layoutManager != null)
                this.RefreshValidation(layoutManager.GetValidationPresenter(this));
        }
    }
}
