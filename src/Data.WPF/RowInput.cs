using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class RowInput<T> : Input<T>
        where T : UIElement, new()
    {
        internal static RowInput<T> Create<TData>(Trigger<T> flushTrigger, Column<TData> column, Func<T, TData> getValue)
        {
            return new RowInput<T>(flushTrigger).Bind(column, getValue);
        }
            

        private RowInput(Trigger<T> flushTrigger)
            : base(flushTrigger)
        {
        }

        private IColumnSet _columns = ColumnSet.Empty;
        private List<Func<RowPresenter, T, bool>> _flushFuncs = new List<Func<RowPresenter, T, bool>>();
        private Func<RowPresenter, ValidationMessage> _postValidator;

        public RowInput<T> WithPreValidator(Func<T, ValidationMessage> preValidator, Trigger<T> preValidatorTrigger)
        {
            SetPreValidator(preValidator, preValidatorTrigger);
            return this;
        }

        public RowInput<T> WithPostValidator(Func<RowPresenter, ValidationMessage> postValidator)
        {
            VerifyNotSealed();
            _postValidator = postValidator;
            return this;
        }

        public RowInput<T> Bind<TData>(Column<TData> column, Func<T, TData> getValue)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (getValue == null)
                throw new ArgumentNullException(nameof(getValue));

            VerifyNotSealed();
            _columns = _columns.Merge(column);
            _flushFuncs.Add((rowPresenter, element) =>
            {
                var value = getValue(element);
                if (column.AreEqual(rowPresenter.GetValue(column), value))
                    return false;
                rowPresenter.EditValue(column, getValue(element));
                return true;
            });
            return this;
        }

        internal IColumnSet Columns
        {
            get { return _columns; }
        }

        internal override bool DoFlush(T element)
        {
            bool result = false;
            var rowPresenter = element.GetRowPresenter();
            foreach (var flush in _flushFuncs)
            {
                var flushed = flush(rowPresenter, element);
                if (flushed)
                    result = true;
            }
            return result;
        }

        internal override void RefreshValidationMessages()
        {
            ValidationManager.RefreshCurrentRowValidationMessages();
        }

        internal override IEnumerable<ValidationMessage> GetValidationMessages(ValidationSeverity severity)
        {
            return ValidationManager.GetValidationMessages(this, severity);
        }

        internal override IEnumerable<ValidationMessage> GetMergedValidationMessages(ValidationSeverity severity)
        {
            return ValidationManager.GetMergedValidationMessages(this, severity);
        }
    }
}
