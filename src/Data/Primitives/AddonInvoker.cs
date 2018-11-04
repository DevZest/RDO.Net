using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    public abstract class AddonInvoker<T>
        where T : class, IAddon
    {
        protected AddonInvoker(AddonBag addonBag)
        {
            addonBag.VerifyNotNull(nameof(addonBag));
            _addonBag = addonBag;
        }

        private AddonBag _addonBag;

        public bool IsAsync { get; private set; }

        public TaskStatus TaskStatus { get; private set; }

        public Exception OriginalException { get; private set; }

        public Exception Exception { get; private set; }

        private void SetExceptionThrown(Exception exception)
        {
            OriginalException = exception;
            Exception = exception;
        }

        protected void Invoke(Action operation, Action<T> onExecuting, Action<T> onExecuted)
        {
            var addons = BeginExecute(false, onExecuting);
            try
            {
                operation();
            }
            catch (Exception ex)
            {
                SetExceptionThrown(ex);
                OnExecuted(onExecuted, addons);
                throw;
            }

            OnExecuted(onExecuted, addons);
        }

        private void OnExecuted(Action<T> onExecuted, ReadOnlyCollection<T> addons)
        {
            try
            {
                foreach (var addon in addons)
                    onExecuted(addon);
            }
            catch (Exception outerEx)
            {
                Exception = outerEx;
                throw;
            }
        }

        private ReadOnlyCollection<T> BeginExecute(bool isAsync, Action<T> onExecuting)
        {
            IsAsync = isAsync;
            if (isAsync)
                TaskStatus = TaskStatus.Created;
            SetExceptionThrown(null);
            var addons = _addonBag.GetAddons<T>();
            foreach (var addon in addons)
                onExecuting(addon);
            return addons;
        }

        protected Task InvokeAsync(Task operation, Action<T> executing, Action<T> executed)
        {
            var addons = BeginExecute(true, executing);

            var tcs = new TaskCompletionSource<object>();
            operation.ContinueWith(
                t =>
                {
                    TaskStatus = t.Status;

                    if (t.IsFaulted)
                        SetExceptionThrown(t.Exception.InnerException);

                    try
                    {
                        foreach (var addon in addons)
                            executed(addon);
                    }
                    catch (Exception ex)
                    {
                        Exception = ex;
                    }

                    if (Exception != null)
                        tcs.SetException(Exception);
                    else if (t.IsCanceled)
                        tcs.SetCanceled();
                    else
                        tcs.SetResult(null);
                }, TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }
    }
}
