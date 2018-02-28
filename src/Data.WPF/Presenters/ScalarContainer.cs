using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    public sealed class ScalarContainer : ReadOnlyCollection<Scalar>
    {
        internal interface IOwner
        {
            void InvalidateView();
            void SuspendInvalidateView();
            void ResumeInvalidateView();
            void OnValueChanged(IScalars scalars);
        }

        internal ScalarContainer(IOwner owner)
            : base(new List<Scalar>())
        {
            Debug.Assert(owner != null);
            _owner = owner;
        }

        private readonly IOwner _owner;

        public bool IsEditing { get; private set; }

        public void BeginEdit()
        {
            if (IsEditing)
                return;

            IsEditing = true;
            _owner.InvalidateView();
        }

        public void CancelEdit()
        {
            if (!IsEditing)
                throw new InvalidOperationException(DiagnosticMessages.RowPresenter_VerifyIsEditing);

            _owner.SuspendInvalidateView();
            for (int i = 0; i < Count; i++)
                this[i].CancelEdit();
            IsEditing = false;
            _owner.ResumeInvalidateView();
        }

        public bool EndEdit()
        {
            if (!IsEditing)
                throw new InvalidOperationException(DiagnosticMessages.RowPresenter_VerifyIsEditing);

            _owner.SuspendInvalidateView();
            SuspendValueChangedNotification();
            for (int i = 0; i < Count; i++)
                this[i].EndEdit();
            IsEditing = false;
            ResumeValueChangedNotification();
            _owner.ResumeInvalidateView();
            return true;
        }

        internal bool Edit<T>(Scalar<T> scalar, T value)
        {
            Debug.Assert(scalar != null && scalar.Container == this);

            _owner.SuspendInvalidateView();
            BeginEdit();
            var result = scalar.AssignValue(value);
            _owner.ResumeInvalidateView();
            return result;
        }

        private int _suspendValueChangedCount;
        private IScalars _pendingValueChangedScalars = Scalars.Empty;

        public bool IsValueChangedNotificationSuspended
        {
            get { return _suspendValueChangedCount > 0; }
        }

        public void SuspendValueChangedNotification()
        {
            _suspendValueChangedCount++;
        }

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
        }

        private void NotifyValueChanged(IScalars scalars)
        {
            _owner.OnValueChanged(scalars);
        }

        internal Scalar<T> CreateNew<T>(T value = default(T), IComparer<T> comparer = null)
        {
            var result = new Scalar<T>(this, Count, value, comparer);
            Items.Add(result);
            return result;
        }
    }
}
