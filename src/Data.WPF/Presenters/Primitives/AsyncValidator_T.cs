using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class AsyncValidator<T> : AsyncValidator
        where T : class
    {
        internal AsyncValidator(Template template)
            : base(template)
        {
        }

        private bool _pendingValidationRequest;

        private Task<T> _awaitingTask;

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
