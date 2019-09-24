using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents the container for scalar data.
    /// </summary>
    public sealed class ScalarContainer : ReadOnlyCollection<Scalar>
    {
        internal interface IOwner
        {
            void InvalidateView();
            void SuspendInvalidateView();
            void ResumeInvalidateView();
            void OnValueChanged(IScalars scalars);
            bool QueryEndEdit();
            void OnBeginEdit();
            void OnCancelEdit();
            void OnEndEdit();
            void OnEdit(Scalar scalar);
        }

        internal ScalarContainer(IOwner owner)
            : base(new List<Scalar>())
        {
            Debug.Assert(owner != null);
            _owner = owner;
        }

        private readonly IOwner _owner;

        private bool _isEditing;
        /// <summary>
        /// Gets the value indicates whether scalar data is in editing mode.
        /// </summary>
        public bool IsEditing
        {
            get { return _isEditing; }
            private set
            {
                Debug.Assert(_isEditing != value);
                _isEditing = value;
                _owner.InvalidateView();
            }
        }

        /// <summary>
        /// Begins scalar editing mode.
        /// </summary>
        public void BeginEdit()
        {
            if (IsEditing)
                return;

            _owner.SuspendInvalidateView();
            for (int i = 0; i < Count; i++)
                this[i].BeginEdit();
            IsEditing = true;
            _owner.OnBeginEdit();
            _owner.ResumeInvalidateView();
        }

        /// <summary>
        /// Cancels scalar editing mode.
        /// </summary>
        public void CancelEdit()
        {
            if (!IsEditing)
                throw new InvalidOperationException(DiagnosticMessages._VerifyIsEditing);

            _owner.SuspendInvalidateView();
            for (int i = 0; i < Count; i++)
                this[i].CancelEdit();
            IsEditing = false;
            _owner.OnCancelEdit();
            _owner.ResumeInvalidateView();
        }

        /// <summary>
        /// Ends scalar editing mode.
        /// </summary>
        /// <returns><see langword="true"/> if scalar editing mode ended successfully, otherwise <see langword="false"/>.</returns>
        public bool EndEdit()
        {
            if (!IsEditing)
                throw new InvalidOperationException(DiagnosticMessages._VerifyIsEditing);

            if (Count == 0)
            {
                IsEditing = false;
                return true;
            }

            if (!_owner.QueryEndEdit())
                return false;

            _owner.SuspendInvalidateView();
            SuspendValueChangedNotification();
            for (int i = 0; i < Count; i++)
                this[i].EndEdit();
            IsEditing = false;
            _owner.OnEndEdit();
            ResumeValueChangedNotification();
            _owner.ResumeInvalidateView();
            return true;
        }

        internal bool Edit<T>(Scalar<T> scalar, T value)
        {
            Debug.Assert(scalar != null && scalar.Container == this);

            _owner.SuspendInvalidateView();
            BeginEdit();
            var result = scalar.SetValue(value);
            _owner.OnEdit(scalar);
            _owner.ResumeInvalidateView();
            return result;
        }

        private int _suspendValueChangedCount;
        private IScalars _pendingValueChangedScalars = Scalars.Empty;

        /// <summary>
        /// Gets the value indicates whether vlaue changed notification is suspended.
        /// </summary>
        public bool IsValueChangedNotificationSuspended
        {
            get { return _suspendValueChangedCount > 0; }
        }

        /// <summary>
        /// Suspends value changed notification.
        /// </summary>
        public void SuspendValueChangedNotification()
        {
            _suspendValueChangedCount++;
        }

        /// <summary>
        /// Resumes value changed notification.
        /// </summary>
        public void ResumeValueChangedNotification()
        {
            if (_suspendValueChangedCount <= 0)
                throw new InvalidOperationException(DiagnosticMessages.ScalarContainer_ResumeValueChangedWithoutSuspend);
            _suspendValueChangedCount--;
            if (_pendingValueChangedScalars.Count > 0)
                NotifyValueChanged(_pendingValueChangedScalars.Seal());
            _pendingValueChangedScalars = Scalars.Empty;
        }

        internal void OnValueChanged(Scalar scalar)
        {
            if (IsValueChangedNotificationSuspended)
                _pendingValueChangedScalars = _pendingValueChangedScalars.Add(scalar);
            else
                NotifyValueChanged(scalar);
            _owner.InvalidateView();
        }

        private void NotifyValueChanged(IScalars scalars)
        {
            _owner.OnValueChanged(scalars);
        }

        internal Scalar<T> CreateNew<T>(T value = default(T), IEqualityComparer<T> equalityComparer = null)
        {
            var result = Scalar<T>.Create(this, Count, equalityComparer, value);
            Items.Add(result);
            return result;
        }

        internal Scalar<T> CreateNew<T>(Func<T> getter, Action<T> setter, IEqualityComparer<T> equalityComparer = null)
        {
            var result = Scalar<T>.Create(this, Count, equalityComparer, getter, setter);
            Items.Add(result);
            return result;
        }
    }
}
