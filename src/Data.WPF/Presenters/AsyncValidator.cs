using DevZest.Data.Presenters.Primitives;
using System;
using System.Threading.Tasks;

namespace DevZest.Data.Presenters
{
    public abstract class AsyncValidator
    {
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

        public AsyncValidatorStatus Status { get; internal set; } = AsyncValidatorStatus.Inactive;

        public Exception Exception { get; internal set; }

        public abstract void Run();

#if DEBUG
        internal abstract Task LastRunTask { get; }
#endif
    }
}
