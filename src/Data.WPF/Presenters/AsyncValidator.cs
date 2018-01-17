using DevZest.Data.Presenters.Primitives;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DevZest.Data.Presenters
{
    public abstract class AsyncValidator
    {
        internal AsyncValidator(Template template)
        {
            Debug.Assert(template != null);
            _template = template;
        }

        private readonly Template _template;
        internal InputManager InputManager
        {
            get { return _template.InputManager; }
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
