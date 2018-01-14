using DevZest.Data.Presenters.Primitives;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace DevZest.Data.Presenters
{
    public abstract class AsyncValidator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private AsyncValidatorStatus _status = AsyncValidatorStatus.Created;
        public AsyncValidatorStatus Status
        {
            get { return _status; }
            internal set
            {
                if (_status == value)
                    return;
                _status = value;
                OnPropertyChanged(nameof(Status));
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
                OnPropertyChanged(nameof(Exception));
            }
        }

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
                Status = AsyncValidatorStatus.Created;
                InputManager.InvalidateView();
            }
        }

        internal abstract void Reset();
    }
}
