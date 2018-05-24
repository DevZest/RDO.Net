using DevZest.Data.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    public abstract class ExtensibleObjectInvoker<T>
        where T : class, IExtension
    {
        protected ExtensibleObjectInvoker(ExtensibleObject extensibleObject)
        {
            Check.NotNull(extensibleObject, nameof(extensibleObject));
            _extensibleObject = extensibleObject;
        }

        private ExtensibleObject _extensibleObject;

        public bool IsAsync { get; private set; }

        public TaskStatus TaskStatus { get; private set; }

        public Exception OriginalException { get; private set; }

        public Exception Exception { get; private set; }

        private void SetExceptionThrown(Exception exception)
        {
            OriginalException = exception;
            Exception = exception;
        }

        protected void Invoke(Action operation, Action<T> executing, Action<T> executed)
        {
            var extensions = Executing(false, executing);
            try
            {
                operation();
            }
            catch (Exception ex)
            {
                SetExceptionThrown(ex);
                Executed(executed, extensions);
                throw;
            }

            Executed(executed, extensions);
        }

        private void Executed(Action<T> executed, ReadOnlyCollection<T> extensions)
        {
            try
            {
                foreach (var extension in extensions)
                    executed(extension);
            }
            catch (Exception outerEx)
            {
                Exception = outerEx;
                throw;
            }
        }

        private ReadOnlyCollection<T> Executing(bool isAsync, Action<T> executing)
        {
            IsAsync = isAsync;
            if (isAsync)
                TaskStatus = TaskStatus.Created;
            SetExceptionThrown(null);
            var extensions = _extensibleObject.GetExtensions<T>();
            foreach (var extension in extensions)
                executing(extension);
            return extensions;
        }

        protected Task InvokeAsync(Task operation, Action<T> executing, Action<T> executed)
        {
            var extensions = Executing(true, executing);

            var tcs = new TaskCompletionSource<object>();
            operation.ContinueWith(
                t =>
                {
                    TaskStatus = t.Status;

                    if (t.IsFaulted)
                        SetExceptionThrown(t.Exception.InnerException);

                    try
                    {
                        foreach (var extension in extensions)
                            executed(extension);
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
