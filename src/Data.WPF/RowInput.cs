using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class RowInput<T> : Input<T>, IRowInput
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
                ValidateInput(element);
                if (GetInputError(element) != null)
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

        public IAbstractValidationMessageGroup GetErrors(RowPresenter rowPresenter)
        {
            RowBinding rowBinding = RowBinding;
            var element = rowBinding[rowPresenter];
            if (element != null)
            {
                var inputError = GetInputError(RowBinding[rowPresenter]);
                if (inputError != null)
                    return inputError;
            }

            var result = ValidationMessageGroup.Empty;
            result = AddValidationMessages(result, InputManager.Errors, rowPresenter, x => IsVisible(x, rowPresenter, true));
            result = AddAsyncValidationMessages(result, rowPresenter, ValidationSeverity.Error);
            result = AddValidationMessages(result, InputManager.ValidationResult, rowPresenter, x => x.Severity == ValidationSeverity.Error && IsVisible(x, rowPresenter, false));
            return result;
        }

        public IValidationMessageGroup GetWarnings(RowPresenter rowPresenter)
        {
            var result = ValidationMessageGroup.Empty;
            result = AddValidationMessages(result, InputManager.Warnings, rowPresenter, x => IsVisible(x, rowPresenter, true));
            result = AddAsyncValidationMessages(result, rowPresenter, ValidationSeverity.Warning);
            result = AddValidationMessages(result, InputManager.ValidationResult, rowPresenter, x => x.Severity == ValidationSeverity.Warning && IsVisible(x, rowPresenter, false));
            return result;
        }

        private bool IsVisible(ValidationMessage validationMessage, RowPresenter rowPresenter, bool progressVisible)
        {
            var source = validationMessage.Source;
            return source.SetEquals(Columns) && InputManager.Progress.IsVisible(rowPresenter, source) == progressVisible;
        }

        private static IValidationMessageGroup AddValidationMessages(IValidationMessageGroup result, IValidationDictionary dictionary, RowPresenter rowPresenter, Func<ValidationMessage, bool> predict)
        {
            if (dictionary.ContainsKey(rowPresenter))
            {
                var messages = dictionary[rowPresenter];
                for (int i = 0; i < messages.Count; i++)
                {
                    var message = messages[i];
                    if (predict(message))
                        result = result.Add(message);
                }
            }

            return result;
        }

        private IValidationMessageGroup AddAsyncValidationMessages(IValidationMessageGroup result, RowPresenter rowPresenter, ValidationSeverity severity)
        {
            var asyncValidators = Template.AsyncValidators;
            for (int i = 0; i < asyncValidators.Count; i++)
            {
                var asyncValidator = asyncValidators[i];
                var dictionary = severity == ValidationSeverity.Error ? asyncValidator.Errors : asyncValidator.Warnings;
                result = AddValidationMessages(result, dictionary, rowPresenter, x => IsVisible(x, rowPresenter, true));
            }

            return result;
        }

        private void RefreshValidation(T element, RowPresenter rowPresenter)
        {
            element.RefreshValidation(GetErrors(rowPresenter), GetWarnings(rowPresenter));
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
            Template.InternalAsyncValidators = Template.InternalAsyncValidators.Add(asyncValidator);
            return this;
        }

        public RowBinding<T> EndInput()
        {
            return RowBinding;
        }

        private IAsyncValidatorGroup _asyncValidators;
        public IAsyncValidatorGroup AsyncValidators
        {
            get
            {
                if (InputManager == null)
                    return null;

                if (_asyncValidators == null)
                    _asyncValidators = Template.AsyncValidators.Where(x => x.RowInput == this);
                return _asyncValidators;
            }
        }
    }
}
