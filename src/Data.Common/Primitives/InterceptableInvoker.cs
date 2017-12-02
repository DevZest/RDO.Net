using DevZest.Data.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    public abstract class InterceptableInvoker<T>
        where T : class, IResource
    {
        protected InterceptableInvoker(ResourceContainer interceptable)
        {
            Check.NotNull(interceptable, nameof(interceptable));
            _interceptable = interceptable;
        }

        private ResourceContainer _interceptable;

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
            var interceptors = Executing(false, executing);
            try
            {
                operation();
            }
            catch (Exception ex)
            {
                SetExceptionThrown(ex);
                Executed(executed, interceptors);
                throw;
            }

            Executed(executed, interceptors);
        }

        private void Executed(Action<T> executed, ReadOnlyCollection<T> interceptors)
        {
            try
            {
                foreach (var interceptor in interceptors)
                    executed(interceptor);
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
            var interceptors = _interceptable.GetResources<T>();
            foreach (var interceptor in interceptors)
                executing(interceptor);
            return interceptors;
        }

        protected Task InvokeAsync(Task operation, Action<T> executing, Action<T> executed, CancellationToken cancellationToken)
        {
            var interceptors = Executing(true, executing);

            var tcs = new TaskCompletionSource<object>();
            operation.ContinueWith(
                t =>
                {
                    TaskStatus = t.Status;

                    if (t.IsFaulted)
                        SetExceptionThrown(t.Exception.InnerException);

                    try
                    {
                        foreach (var interceptor in interceptors)
                            executed(interceptor);
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
                }, cancellationToken);

            return tcs.Task;
        }
    }
}
