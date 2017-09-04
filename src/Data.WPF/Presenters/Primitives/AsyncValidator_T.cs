using System;
using System.Threading.Tasks;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class AsyncValidator<T> : AsyncValidator
    {
        protected AsyncValidator(Action postAction)
        {
            _postAction = postAction;
        }

        private bool _pendingValidationRequest;
        private readonly Action _postAction;

        private Task<T> _awaitingTask;

        public sealed override async void Run()
        {
#if DEBUG
            var runningTask = LastRunningTask = ValidateAsync();
#else
            var runningTask = ValidateAsync();
#endif
            await runningTask;
        }

        protected abstract void ClearValidationMessages();

        protected abstract Task<T> ValidateCoreAsync();

        protected abstract void RefreshValidationMessages(T result);

        protected abstract T EmptyValidationResult { get; }

        private async Task ValidateAsync()
        {
            if (Status == AsyncValidatorStatus.Running)
            {
                _pendingValidationRequest = true;
                return;
            }

            ClearValidationMessages();
            Exception = null;
            Status = AsyncValidatorStatus.Running;
            InputManager.InvalidateView();

            T result;
            AsyncValidatorStatus status;
            Exception exception;
            do
            {
                _pendingValidationRequest = false;
                var task = _awaitingTask = ValidateCoreAsync();
                try
                {
                    result = await task;
                    if (task != _awaitingTask)   // cancelled
                        return;
                    status = AsyncValidatorStatus.Completed;
                    exception = null;
                }
                catch (Exception ex)
                {
                    if (task != _awaitingTask)  // cancelled
                        return;
                    result = EmptyValidationResult;
                    status = AsyncValidatorStatus.Faulted;
                    exception = ex;
                }
            }
            while (_pendingValidationRequest);

            _awaitingTask = null;
            Exception = exception;
            Status = status;
            RefreshValidationMessages(result);

            if (_postAction != null)
                _postAction();

            InputManager.InvalidateView();
        }

        internal sealed override void Reset()
        {
            if (Status == AsyncValidatorStatus.Running)
                _awaitingTask = null;

            _pendingValidationRequest = false;
            Status = AsyncValidatorStatus.Created;
            Exception = null;
            ClearValidationMessages();
        }
    }
}
