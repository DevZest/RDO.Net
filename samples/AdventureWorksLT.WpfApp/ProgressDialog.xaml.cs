using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DevZest.Samples.AdventureWorksLT
{
    public struct ProgressDialogResult<T>
    {
        internal ProgressDialogResult(T value, Exception exception)
        {
            Value = value;
            Exception = exception;
        }

        public readonly T Value;
        public readonly Exception Exception;
    }

    public struct ProgressDialogResult
    {
        internal ProgressDialogResult(Exception exception)
        {
            Exception = exception;
        }

        public readonly Exception Exception;
    }

    /// <summary>
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : Window
    {
        private ProgressDialog()
        {
            InitializeComponent();
        }

        private Exception Exception { get; set; }

        public string Label
        {
            get { return _textLabel.Text; }
            set { _textLabel.Text = value; }
        }

        private CancellationTokenSource _cancellationTokenSource;

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            _cancellationTokenSource.Cancel();
            _cancelButton.IsEnabled = false;
        }

        public static ProgressDialogResult<T> Execute<T>(Func<CancellationToken, Task<T>> func, Window ownerWindow, string windowTitle = null, string label = null)
        {
            var progressDialog = CreateProgressDialog(ownerWindow, windowTitle, label);
            var value = progressDialog.ShowDialog(func);
            return new ProgressDialogResult<T>(value, progressDialog.Exception);
        }

        private T ShowDialog<T>(Func<CancellationToken, Task<T>> func)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var task = func(_cancellationTokenSource.Token);
            return ShowDialog(task);
        }

        private abstract class AsyncActionBase
        {
            protected AsyncActionBase(ProgressDialog dialog)
            {
                Dialog = dialog;
            }

            public ProgressDialog Dialog { get; private set; }

            public async void Run()
            {
                try
                {
                    await RunAsync();
                }
                catch (Exception ex)
                {
                    Dialog.Exception = ex;
                }
                finally
                {
                    Dialog.Close();
                }
            }

            protected abstract Task RunAsync();
        }

        private sealed class AsyncAction<T> : AsyncActionBase
        {
            public AsyncAction(Task<T> task, ProgressDialog dialog)
                : base(dialog)
            {
                _task = task;
            }

            private Task<T> _task;

            public T Result { get; private set; }

            protected override async Task RunAsync()
            {
                Result = await _task;
            }
        }

        private T ShowDialog<T>(Task<T> task)
        {
            var asyncAction = new AsyncAction<T>(task, this);
            ShowDialog(asyncAction);
            return asyncAction.Result;
        }

        public static ProgressDialogResult Execute(Func<CancellationToken, Task> func, Window ownerWindow, string windowTitle=null, string label = null)
        {
            var progressDialog = CreateProgressDialog(ownerWindow, windowTitle, label);
            progressDialog.ShowDialog(func);
            return new ProgressDialogResult(progressDialog.Exception);
        }

        private static ProgressDialog CreateProgressDialog(Window ownerWindow, string windowTitle, string label)
        {
            return new ProgressDialog()
            {
                Owner = ownerWindow,
                Title = windowTitle ?? "Executing...",
                Label = label ?? "Please wait...",
            };
        }

        private void ShowDialog(Func<CancellationToken, Task> func)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var task = func(_cancellationTokenSource.Token);
            ShowDialog(task);
        }

        private sealed class AsyncAction : AsyncActionBase
        {
            public AsyncAction(Task task, ProgressDialog dialog)
                : base(dialog)
            {
                _task = task;
            }

            private Task _task;

            protected override async Task RunAsync()
            {
                await _task;
            }
        }

        private void ShowDialog(Task task)
        {
            var asyncAction = new AsyncAction(task, this);
            ShowDialog(asyncAction);
        }

        private AsyncActionBase _asyncAction;
        private void ShowDialog(AsyncActionBase asyncAction)
        {
            _asyncAction = asyncAction;
            ShowDialog();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            _asyncAction.Run();
        }
    }
}
