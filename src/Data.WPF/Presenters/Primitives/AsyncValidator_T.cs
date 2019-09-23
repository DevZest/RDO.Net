using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Represents validator with strongly typed result that will be executed asynchronously.
    /// </summary>
    /// <typeparam name="T">Type of validation results.</typeparam>
    public abstract class AsyncValidator<T> : AsyncValidator
        where T : class
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AsyncValidator"/> class.
        /// </summary>
        /// <param name="displayName">The display name of this async validator, used in <see cref="AsyncValidationFault"/> error message.</param>
        protected AsyncValidator(string displayName)
            : base(displayName)
        {
        }

        private bool _pendingValidationRequest;

        private Task<T> _awaitingTask;

        /// <inheritdoc/>
        public override async void Run()
        {
            if (Status == AsyncValidatorStatus.Running)
            {
                _pendingValidationRequest = true;
                return;
            }

#if DEBUG
            var statusChanged = await (_lastRunTask = RunAsync());
            _lastRunTask = null;
#else
            var statusChanged = await RunAsync();
#endif
            if (statusChanged)
                OnStatusChanged();
        }

        internal abstract void OnStatusChanged();

        internal abstract T EmptyResult { get; }

        private T _results;
        /// <summary>
        /// Gets the validation results.
        /// </summary>
        public T Results
        {
            get { return _results ?? EmptyResult; }
        }

        internal abstract Task<T> ValidateAsync();

        private async Task<bool> RunAsync()
        {
            Exception = null;
            Status = AsyncValidatorStatus.Running;
            InvalidateView();

            T result;
            AsyncValidatorStatus status;
            Exception exception;
            do
            {
                _pendingValidationRequest = false;
                var task = _awaitingTask = ValidateAsync();
                try
                {
                    result = await task;
                    if (task != _awaitingTask)   // cancelled
                        return false;
                    status = result != null && result != EmptyResult ? AsyncValidatorStatus.Error : AsyncValidatorStatus.Validated;
                    exception = null;
                }
                catch (Exception ex)
                {
                    if (task != _awaitingTask)  // cancelled
                        return false;
                    result = EmptyResult;
                    Debug.Assert(result != null);
                    status = AsyncValidatorStatus.Faulted;
                    exception = ex;
                }
            }
            while (_pendingValidationRequest);

            _awaitingTask = null;
            _results = result;
            Exception = exception;
            Status = status;
            return true;
        }

        internal void Reset()
        {
            if (Status == AsyncValidatorStatus.Running)
                _awaitingTask = null;

            _pendingValidationRequest = false;
            _results = null;
            Status = AsyncValidatorStatus.Inactive;
            Exception = null;
        }

#if DEBUG
        private Task<bool> _lastRunTask;
        internal override Task LastRunTask
        {
            get { return _lastRunTask; }
        }
#endif
    }
}
