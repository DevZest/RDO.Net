using DevZest.Data.Presenters.Primitives;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace DevZest.Data.Presenters
{
    public abstract class AsyncValidator
    {
        public AsyncValidatorStatus Status { get; internal set; } = AsyncValidatorStatus.Inactive;

        public Exception Exception { get; internal set; }

#if DEBUG
        internal Task LastRunningTask { get; set; }
#endif

        internal abstract InputManager InputManager { get; }

        public abstract bool HasError { get; }

        public abstract void Run();

        public void CancelRunning()
        {
            if (Status == AsyncValidatorStatus.Running)
            {
                Status = AsyncValidatorStatus.Inactive;
                InputManager.InvalidateView();
            }
        }

        internal abstract void Reset();
    }
}
