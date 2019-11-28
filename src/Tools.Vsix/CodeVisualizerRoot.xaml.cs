using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for CodeVisualizerRoot.xaml
    /// </summary>
    public partial class CodeVisualizerRoot : Grid
    {
        public CodeVisualizerRoot()
        {
            InitializeComponent();
            RefreshIsBusy(IsBusy);
        }

        public void Initialize(Action onGotFocus, Action onLostFocus, Action<DependencyPropertyChangedEventArgs> onPropertyChanged)
        {
            _onGotFocus = onGotFocus;
            _onLostFocus = onLostFocus;
            _onPropertyChanged = onPropertyChanged;
        }

        private Action _onGotFocus;
        private Action _onLostFocus;
        private Action<DependencyPropertyChangedEventArgs> _onPropertyChanged;

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            _onGotFocus?.Invoke();
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            _onLostFocus?.Invoke();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            _onPropertyChanged?.Invoke(e);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (value == _isBusy)
                    return;

                RefreshIsBusy(_isBusy = value);
            }
        }

        private void RefreshIsBusy(bool value)
        {
            _progressBar.Visibility = value ? Visibility.Visible : Visibility.Hidden;
            _progressBar.IsIndeterminate = value;
            _contentPresenter.IsEnabled = !value;
        }

        public UIElement Content
        {
            get { return _contentPresenter.Content as UIElement; }
            set { _contentPresenter.Content = value; }
        }
    }
}
