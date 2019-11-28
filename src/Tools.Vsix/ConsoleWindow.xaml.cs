using DevZest.Data.DbInit;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for ConsoleWindow.xaml
    /// </summary>
    public partial class ConsoleWindow : DialogWindow, IProgress<string>
    {
        private enum ConsoleWindowStatus
        {
            Idle = 0,
            Running,
            Succeeded,
            Failed,
            Cancelling
        }

        private ConsoleWindowStatus _status = ConsoleWindowStatus.Idle;
        private ConsoleWindowStatus Status
        {
            get { return _status; }
            set
            {
                if (_status == value)
                    return;

                _status = value;
                Refresh();
            }
        }

        private void Refresh()
        {
            _buttonBack.Visibility = Status == ConsoleWindowStatus.Failed ? Visibility.Visible : Visibility.Hidden;
            _buttonClose.Visibility = Status == ConsoleWindowStatus.Running || Status == ConsoleWindowStatus.Cancelling ? Visibility.Hidden : Visibility.Visible;
            _buttonClose.IsDefault = Status == ConsoleWindowStatus.Succeeded;
            _buttonCancel.Visibility = Status == ConsoleWindowStatus.Running || Status == ConsoleWindowStatus.Cancelling ? Visibility.Visible : Visibility.Hidden;
            IsCloseButtonEnabled = _buttonCancel.IsEnabled = Status != ConsoleWindowStatus.Cancelling;
        }

        public ConsoleWindow()
        {
            InitializeComponent();
            Refresh();
            Loaded += OnLoaded;
        }

        private ConsoleWindow(string title, ConsoleWindowOperationDelegate operation)
            : this()
        {
            Title = title;
            _operation = operation;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_operation != null)
                Status = await RunOperationAsync();
        }

        private async Task<ConsoleWindowStatus> RunOperationAsync()
        {
            using (_cts = new CancellationTokenSource())
            {
                try
                {
                    Status = ConsoleWindowStatus.Running;
                    var result = await _operation(this, _cts.Token);
                    if (result == ExitCodes.Succeeded)
                        return ConsoleWindowStatus.Succeeded;
                    else
                        return ConsoleWindowStatus.Failed;
                }
                catch (OperationCanceledException)
                {
                    this.ReportLine(UserMessages.ConsoleWindow_OperationCancelled);
                    return ConsoleWindowStatus.Failed;
                }
                catch (Exception ex)
                {
                    this.ReportLine(UserMessages.ConsoleWindow_OperationFaulted);
                    Report(ex.Message);
                    return ConsoleWindowStatus.Failed;
                }
            }
        }

        private CancellationTokenSource _cts;
        private readonly ConsoleWindowOperationDelegate _operation;
        public static bool Run(string title, ConsoleWindowOperationDelegate operation)
        {
            return new ConsoleWindow(title, operation).ShowDialog().Value;
        }

        public void Report(string value)
        {
            _output.Log(value);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Cancel();

            if (Status == ConsoleWindowStatus.Cancelling)
                e.Cancel = true;

            base.OnClosing(e);
        }

        private void Cancel()
        {
            if (Status == ConsoleWindowStatus.Running)
            {
                Status = ConsoleWindowStatus.Cancelling;
                _cts.Cancel();
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _output.Close();
        }
    }
}
