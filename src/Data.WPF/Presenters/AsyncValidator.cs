using DevZest.Data.Presenters.Primitives;
using System;
using System.Threading.Tasks;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents validator that will be executed asynchronously.
    /// </summary>
    public abstract class AsyncValidator
    {
        /// <summary>
        /// Initializes a new instance of  <see cref="AsyncValidator"/> class.
        /// </summary>
        /// <param name="displayName">The display name of this async validator, used in <see cref="AsyncValidationFault"/> error message.</param>
        protected AsyncValidator(string displayName)
        {
            _displayName = displayName;
        }

        private readonly string _displayName;
        /// <summary>
        /// Gets the display name of this async validator, used in <see cref="AsyncValidationFault"/> error message.
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
        }

        private Template _template;
        /// <summary>
        /// Gets the <see cref="Template"/> that owns this async validator.
        /// </summary>
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
        /// <summary>
        /// Gets the status of this async validator.
        /// </summary>
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
        /// <summary>
        /// Gets the last exception of executing this async validator.
        /// </summary>
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

        /// <summary>
        /// Executes this async validator.
        /// </summary>
        public abstract void Run();

#if DEBUG
        internal abstract Task LastRunTask { get; }
#endif

        private BasePresenter Presenter
        {
            get { return InputManager.Presenter; }
        }

        /// <summary>
        /// Gets the the error of executing this async validator such as network failure which can be retried.
        /// </summary>
        protected AsyncValidationFault Fault { get; private set; }

        private AsyncValidationFault CoerceFault()
        {
            if (Exception == null)
                return null;

            var presenter = Presenter;
            if (presenter == null)
                return new AsyncValidationFault(this, null);
            else
                return new AsyncValidationFault(this, presenter.FormatFaultMessage);
        }
    }
}
