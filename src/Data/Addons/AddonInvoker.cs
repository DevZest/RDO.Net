using DevZest.Data.Primitives;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DevZest.Data.Addons
{
    /// <summary>
    /// Base class to wrap action execution and notify to <see cref="IAddon"/> objects.
    /// </summary>
    public abstract class AddonInvoker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonInvoker"/> class.
        /// </summary>
        /// <param name="addonBag">The <see cref="AddonBag"/>.</param>
        protected AddonInvoker(AddonBag addonBag)
        {
            AddonBag = addonBag.VerifyNotNull(nameof(addonBag));
        }

        /// <summary>
        /// Gets the <see cref="AddonBag"/>.
        /// </summary>
        public AddonBag AddonBag { get; }

        /// <summary>
        /// Gets a value indicates whether the executing action is asynchronous.
        /// </summary>
        public bool IsAsync { get; protected set; }

        /// <summary>
        /// Gets the <see cref="TaskStatus"/> of executing action.
        /// </summary>
        public TaskStatus TaskStatus { get; protected set; }

        /// <summary>
        /// Gets or sets the original <see cref="Exception"/> thrown during action execution.
        /// </summary>
        public Exception OriginalException { get; protected set; }

        /// <summary>
        /// Gets or sets the outmost <see cref="Exception"/> thrown during action execution.
        /// </summary>
        public Exception Exception { get; protected set; }
    }

    /// <summary>
    /// Base class to wrap action execution and notify to <see cref="IAddon"/> objects with specified type.
    /// </summary>
    /// <typeparam name="T">The implementation type of <see cref="IAddon"/> objects, which receive the notification of the action execution.</typeparam>
    public abstract class AddonInvoker<T> : AddonInvoker
        where T : class, IAddon
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonInvoker{T}"/> class.
        /// </summary>
        /// <param name="addonBag">The <see cref="AddonBag"/>.</param>
        protected AddonInvoker(AddonBag addonBag)
            : base(addonBag)
        {
        }

        private void SetExceptionThrown(Exception exception)
        {
            OriginalException = exception;
            Exception = exception;
        }

        /// <summary>
        /// Executes the action and notifies the addon objects.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <param name="onExecuting">Receives the notification before action executing.</param>
        /// <param name="onExecuted">Receives the notification after action executed.</param>
        protected void Invoke(Action action, Action<T> onExecuting, Action<T> onExecuted)
        {
            var addons = BeginExecute(false, onExecuting);
            try
            {
                action();
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
            var addons = AddonBag.GetAddons<T>();
            foreach (var addon in addons)
                onExecuting(addon);
            return addons;
        }

        /// <summary>
        /// Executes the action asynchronously and notifies the addon objects.
        /// </summary>
        /// <param name="action">The action to be executed asynchronously.</param>
        /// <param name="onExecuting">Receives the notification before action executing.</param>
        /// <param name="onExecuted">Receives the notification after action executed.</param>
        protected Task InvokeAsync(Task action, Action<T> onExecuting, Action<T> onExecuted)
        {
            var addons = BeginExecute(true, onExecuting);

            var tcs = new TaskCompletionSource<object>();
            action.ContinueWith(
                t =>
                {
                    TaskStatus = t.Status;

                    if (t.IsFaulted)
                        SetExceptionThrown(t.Exception.InnerException);

                    try
                    {
                        foreach (var addon in addons)
                            onExecuted(addon);
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
