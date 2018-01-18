using DevZest.Data;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public sealed class RowInput<T> : Input<T, RowBinding, IColumns>
        where T : UIElement, new()
    {
        internal RowInput(RowBinding<T> rowBinding, Trigger<T> flushTrigger, Trigger<T> progressiveFlushTrigger)
            : base(flushTrigger, progressiveFlushTrigger)
        {
            RowBinding = rowBinding;
        }

        public RowBinding<T> RowBinding { get; private set; }

        public sealed override RowBinding Binding
        {
            get { return RowBinding; }
        }

        private RowValidation RowValidation
        {
            get { return InputManager.RowValidation; }
        }

        public sealed override FlushError GetFlushError(UIElement element)
        {
            return RowValidation.GetFlushError(element);
        }

        internal sealed override void SetFlushError(UIElement element, FlushError inputError)
        {
            RowValidation.SetFlushError(element, inputError);
        }

        private IColumns _target = Columns.Empty;
        public override IColumns Target
        {
            get { return _target; }
        }

        private List<Func<RowPresenter, T, bool>> _flushFuncs = new List<Func<RowPresenter, T, bool>>();

        private RowPresenter CurrentRow
        {
            get { return InputManager.CurrentRow; }
        }

        public RowInput<T> WithFlushValidator(Func<T, string> flushValidaitor)
        {
            SetFlushValidator(flushValidaitor);
            return this;
        }

        public RowInput<T> WithFlush<TData>(Column<TData> column, Func<T, TData> getValue)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            VerifyNotSealed();
            _target = _target.Union(column);
            _flushFuncs.Add((rowPresenter, element) =>
            {
                if (getValue == null)
                    return false;
                var value = getValue(element);
                if (column.AreEqual(rowPresenter.GetValue(column), value))
                    return false;
                rowPresenter.EditValue(column, value);
                return true;
            });
            return this;
        }

        public RowInput<T> WithFlush(Column column, Func<RowPresenter, T, bool> flushFunc)
        {
            Check.NotNull(column, nameof(column));
            Check.NotNull(flushFunc, nameof(flushFunc));
            VerifyNotSealed();
            _target = _target.Union(column);
            _flushFuncs.Add(flushFunc);
            return this;
        }

        public RowInput<T> WithFlush(Column column, Func<T, ColumnValueBag> valueBagGetter)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (valueBagGetter == null)
                throw new ArgumentNullException(nameof(valueBagGetter));

            VerifyNotSealed();
            _target = _target.Union(column);
            _flushFuncs.Add((rowPresenter, element) =>
            {
                if (valueBagGetter == null)
                    return false;
                var value = valueBagGetter(element)[column];
                if (object.Equals(rowPresenter.GetObject(column), value))
                    return false;
                rowPresenter[column] = value;
                return true;
            });
            return this;
        }

        internal override bool IsValidationVisible
        {
            get { return InputManager.RowValidation.IsVisible(CurrentRow, Target); }
        }

        internal override void FlushCore(T element, bool makeProgress)
        {
            Debug.Assert(CurrentRow != null);
            var currentRow = CurrentRow;
            if (currentRow != element.GetRowPresenter())
                throw new InvalidOperationException(DiagnosticMessages.RowInput_FlushCurrentRowOnly);
            var valueChanged = DoFlush(currentRow, element);
            InputManager.RowValidation.OnFlushed(this, makeProgress, valueChanged);
        }

        private bool DoFlush(RowPresenter rowPresenter, T element)
        {
            bool result = false;
            for (int i = 0; i < _flushFuncs.Count; i++)
            {
                var flush = _flushFuncs[i];
                var flushed = flush(rowPresenter, element);
                if (flushed)
                    result = true;
            }
            return result;
        }

        public FlushError GetFlushError(RowPresenter rowPresenter)
        {
            RowBinding rowBinding = RowBinding;
            var element = rowBinding[rowPresenter];
            if (element != null)
            {
                var inputError = GetFlushError(element);
                if (inputError != null)
                    return inputError;
            }
            return null;
        }

        public IValidationErrors GetValidationErrors(RowPresenter rowPresenter)
        {
            Check.NotNull(rowPresenter, nameof(rowPresenter));
            return rowPresenter.GetValidationErrors(this);
        }

        public bool HasValidationError(RowPresenter rowPresenter)
        {
            Check.NotNull(rowPresenter, nameof(rowPresenter));
            return rowPresenter.HasValidationError(this);
        }

        private void RefreshValidation(T element, RowPresenter rowPresenter)
        {
            element.RefreshValidation(GetValidationErrors(rowPresenter));
        }

        private Action<T, RowPresenter, FlushError> _onRefresh;
        internal void Refresh(T element, RowPresenter rowPresenter)
        {
            if (!IsFlushing && GetFlushError(element) == null)
                RowBinding.Refresh(element, rowPresenter);
            if (_onRefresh != null)
                _onRefresh(element, rowPresenter, GetFlushError(element));
            RefreshValidation(element, rowPresenter);
        }

        public RowInput<T> WithRefreshAction(Action<T, RowPresenter, FlushError> onRefresh)
        {
            VerifyNotSealed();
            _onRefresh = onRefresh;
            return this;
        }

        public RowAsyncValidator CreateAsyncValidator(Func<DataRow, Task<string>> validator)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            return RowAsyncValidator.Create(Target, validator);
        }

        public RowAsyncValidator CreateAsyncValidator(Func<DataRow, Task<IEnumerable<string>>> validator)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));

            return RowAsyncValidator.Create(Target, validator);
        }

        public RowBinding<T> EndInput()
        {
            _target = _target.Seal();
            return RowBinding;
        }

        internal override bool IsPrecedingOf(Input<RowBinding, IColumns> input)
        {
            Debug.Assert(input != null);
            if (!input.Target.Overlaps(Target))
                return false;
            else if (input.Target.IsSupersetOf(Target))
                return true;
            else
                return input.Index < Index;
        }
    }
}
