using DevZest.Data.Primitives;
using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class RowInput<T> : Input<T>
        where T : UIElement, new()
    {
        internal RowInput(RowBinding<T> rowBinding, Trigger<T> flushTrigger)
            : base(flushTrigger)
        {
            RowBinding = rowBinding;
        }

        public RowBinding<T> RowBinding { get; private set; }

        public sealed override TwoWayBinding Binding
        {
            get { return RowBinding; }
        }

        internal sealed override ViewInputError GetInputError(UIElement element)
        {
            return InputManager.GetRowInputError(element);
        }

        internal sealed override void SetInputError(UIElement element, ViewInputError inputError)
        {
            InputManager.SetRowInputError(element, inputError);
        }

        internal IColumnSet Columns { get; private set; } = ColumnSet.Empty;
        private List<Func<RowPresenter, T, bool>> _flushFuncs = new List<Func<RowPresenter, T, bool>>();

        private void MakeProgress()
        {
            var currentRow = CurrentRow;
            Debug.Assert(currentRow != null);
            InputManager.MakeProgress(currentRow, this);
        }

        private RowPresenter CurrentRow
        {
            get { return InputManager.CurrentRow; }
        }

        public RowInput<T> WithInputValidator(Func<T, InputError> inputValidaitor, Trigger<T> inputValidationTrigger)
        {
            SetInputValidator(inputValidaitor, inputValidationTrigger);
            return this;
        }

        public RowInput<T> WithFlush<TData>(Column<TData> column, Func<T, TData> getValue)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            VerifyNotSealed();
            Columns = Columns.Union(column);
            _flushFuncs.Add((rowPresenter, element) =>
            {
                if (getValue == null)
                    return false;
                var value = getValue(element);
                if (column.AreEqual(rowPresenter.GetValue(column), value))
                    return false;
                rowPresenter.EditValue(column, getValue(element));
                return true;
            });
            return this;
        }

        internal override void FlushCore(T element)
        {
            Debug.Assert(CurrentRow != null);
            var currentRow = CurrentRow;
            if (currentRow != element.GetRowPresenter())
                throw new InvalidOperationException(Strings.RowInput_FlushCurrentRowOnly);
            var flushed = DoFlush(currentRow, element);
            if (flushed)
                MakeProgress();
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

        private void GetValidationMessages(T element, RowPresenter rowPresenter, out IAbstractValidationMessageGroup errors, out IAbstractValidationMessageGroup warnings)
        {
            errors = GetInputError(element);
            if (errors != null)
            {
                warnings = ValidationMessageGroup.Empty;
                return;
            }

            IValidationMessageGroup resultErrors, resultWarnings;
            GetValidationMessages(rowPresenter, out resultErrors, out resultWarnings);
            errors = resultErrors;
            warnings = resultWarnings;
        }

        private void GetValidationMessages(RowPresenter rowPresenter, out IValidationMessageGroup errors, out IValidationMessageGroup warnings)
        {
            errors = warnings = ValidationMessageGroup.Empty;
        }

        private void RefreshValidation(T element, RowPresenter rowPresenter)
        {
            IAbstractValidationMessageGroup errors, warnings;
            GetValidationMessages(element, rowPresenter, out errors, out warnings);
            element.RefreshValidation(errors, warnings);
        }

        private Action<T, RowPresenter, ViewInputError> _onRefresh;
        internal void Refresh(T element, RowPresenter rowPresenter)
        {
            if (_onRefresh != null)
                _onRefresh(element, rowPresenter, GetInputError(element));
            else if (rowPresenter != CurrentRow)
                RowBinding.Refresh(element, rowPresenter);
            RefreshValidation(element, rowPresenter);
        }

        public RowInput<T> WithRefreshAction(Action<T, RowPresenter, ViewInputError> onRefresh)
        {
            VerifyNotSealed();
            _onRefresh = onRefresh;
            return this;
        }

        public RowInput<T> AddAsyncValidator(Func<Task<IValidationMessageGroup>> action, Action postAction = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            VerifyNotSealed();

            var asyncValidator = AsyncValidator.Create<T>(this, action, postAction);
            Template.AsyncValidators = Template.AsyncValidators.Add(asyncValidator);
            return this;
        }

        public RowBinding<T> EndInput()
        {
            return RowBinding;
        }
    }
}
