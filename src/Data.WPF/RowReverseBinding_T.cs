using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class RowReverseBinding<T> : RowReverseBinding
        where T : UIElement, new()
    {
        internal static RowReverseBinding<T> Create<TData>(Trigger<T> flushTrigger, Column<TData> column, Func<T, TData> dataGetter)
        {
            return new RowReverseBinding<T>(flushTrigger).Bind(column, dataGetter);
        }
            

        private RowReverseBinding(Trigger<T> flushTrigger)
        {
            _flushTrigger = flushTrigger;
        }

        private Trigger<T> _flushTrigger;
        private Trigger<T> _flushErrorTrigger;
        private IColumnSet _columns = ColumnSet.Empty;
        private List<Action<RowPresenter, T>> _flushActions = new List<Action<RowPresenter, T>>();
        private Func<T, ReverseBindingError> _flushErrorFunc;

        internal void Attach(T element)
        {
            _flushTrigger.Attach(element);
            if (_flushErrorTrigger != null)
                _flushErrorTrigger.Attach(element);
        }

        internal void Detach(T element)
        {
            _flushTrigger.Detach(element);
            if (_flushErrorTrigger != null)
                _flushErrorTrigger.Detach(element);
        }

        internal override IReadOnlyList<object> GetErrors()
        {
            return ValidationManager.GetErrors(this);
        }

        public RowReverseBinding<T> Bind<TData>(Column<TData> column, Func<T, TData> dataGetter)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (dataGetter == null)
                throw new ArgumentNullException(nameof(dataGetter));

            VerifyNotSealed();
            _columns = _columns.Merge(column);
            _flushActions.Add((rowPresenter, element) => rowPresenter.EditValue(column, dataGetter(element)));
            return this;
        }

        public RowReverseBinding<T> FlushError(Func<T, ReverseBindingError> flushErrorFunc, Trigger<T> flushErrorTrigger = null)
        {
            VerifyNotSealed();
            _flushErrorFunc = flushErrorFunc;
            if (flushErrorTrigger != null)
            {
                flushErrorTrigger.Initialize(this.FlushError);
                _flushErrorTrigger = flushErrorTrigger;
            }
            return this;
        }

        internal override IColumnSet Columns
        {
            get { return _columns; }
        }

        private ReverseBindingError GetError(T element)
        {
            Debug.Assert(Binding != null && element.GetBinding() == Binding);
            element.GetRowPresenter().VerifyIsCurrent();
            return _flushErrorFunc == null ? ReverseBindingError.Empty : _flushErrorFunc(element);
        }

        internal void Flush(T element)
        {
            var error = GetError(element);
            bool flushingErrorChanged = ValidationManager.UpdateFlushingError(this, error);
            if (error.IsEmpty)
            {
                var rowPresenter = element.GetRowPresenter();
                foreach (var flushAction in _flushActions)
                    flushAction(rowPresenter, element);
            }
            if (flushingErrorChanged)
                OnErrorsChanged();
        }

        private void FlushError(T element)
        {
            var error = GetError(element);
            var flushingErrorChanged = ValidationManager.UpdateFlushingError(this, error);
            if (flushingErrorChanged)
                OnErrorsChanged();
        }
    }
}
