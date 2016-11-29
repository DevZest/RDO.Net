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
        private IColumnSet _columns = ColumnSet.Empty;
        private List<Func<RowPresenter, T, bool>> _flushFuncs = new List<Func<RowPresenter, T, bool>>();
        private Func<T, ReverseBindingMessage> _getFlushingMessage;

        internal void Attach(T element)
        {
            _flushTrigger.Attach(element);
        }

        internal void Detach(T element)
        {
            _flushTrigger.Detach(element);
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
            _flushFuncs.Add((rowPresenter, element) => {
                var value = dataGetter(element);
                if (column.AreEqual(rowPresenter.GetValue(column), value))
                    return false;
                rowPresenter.EditValue(column, dataGetter(element));
                return true;
                });
            return this;
        }

        public RowReverseBinding<T> ProvideFlushingMessage(Func<T, ReverseBindingMessage> getFlushingMessage)
        {
            VerifyNotSealed();
            _getFlushingMessage = getFlushingMessage;
            return this;
        }

        internal override IColumnSet Columns
        {
            get { return _columns; }
        }

        private ReverseBindingMessage GetFlushMessage(T element)
        {
            Debug.Assert(Binding != null && element.GetBinding() == Binding);
            element.GetRowPresenter().VerifyIsCurrent();
            return _getFlushingMessage == null ? ReverseBindingMessage.Empty : _getFlushingMessage(element);
        }

        internal bool IsDirty { get; private set; }

        internal void Flush(T element)
        {
            var message = GetFlushMessage(element);
            bool flushingMessageChanged = ValidationManager.UpdateFlushingMessage(this, message);
            if (message.IsEmpty || message.Severity == ValidationSeverity.Warning)
            {
                var rowPresenter = element.GetRowPresenter();
                foreach (var flush in _flushFuncs)
                {
                    var flushed = flush(rowPresenter, element);
                    if (flushed)
                        IsDirty = true;
                }
            }
            if (flushingMessageChanged)
                OnErrorsChanged();
        }
    }
}
