using DevZest.Data.Presenters.Primitives;
using System;
using System.Threading.Tasks;

namespace DevZest.Data.Presenters
{
    public abstract class AsyncValidator
    {
        protected AsyncValidator(string displayName)
        {
            _displayName = displayName;
        }

        private readonly string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
        }

        private Template _template;
        public Template Template
        {
            get { return _template; }
        }

        internal void Initialize(Template template)
        {
            if (_template != null)
                throw new InvalidOperationException(DiagnosticMessages.AsyncValidator_AlreadyInitialized);
            _template = template;
        }

        internal InputManager InputManager
        {
            get { return Template.InputManager; }
        }

        internal void InvalidateView()
        {
            InputManager.InvalidateView();
        }

        private AsyncValidatorStatus _status = AsyncValidatorStatus.Inactive;
        public AsyncValidatorStatus Status
        {
            get { return _status; }
            internal set
            {
                if (_status == value)
                    return;
                _status = value;
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        private Exception _exception;
        public Exception Exception
        {
            get { return _exception; }
            internal set
            {
                if (_exception == value)
                    return;
                _exception = value;
                Fault = CoerceFault();
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public abstract void Run();

#if DEBUG
        internal abstract Task LastRunTask { get; }
#endif

        private DataPresenterBase DataPresenterBase
        {
            get { return InputManager.DataPresenterBase; }
        }

        protected AsyncValidationFault Fault { get; private set; }

        private AsyncValidationFault CoerceFault()
        {
            if (Exception == null)
                return null;

            var dataPresenter = DataPresenterBase;
            if (dataPresenter == null)
                return new AsyncValidationFault(this, null);
            else
                return new AsyncValidationFault(this, dataPresenter.FormatFaultMessage);
        }
    }
}
