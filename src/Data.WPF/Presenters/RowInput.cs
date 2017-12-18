using DevZest.Data;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace DevZest.Data.Presenters
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

        public sealed override FlushErrorMessage GetFlushError(UIElement element)
        {
            return InputManager.GetRowFlushError(element);
        }

        internal sealed override void SetFlushError(UIElement element, FlushErrorMessage inputError)
        {
            InputManager.SetRowFlushError(element, inputError);
        }

        internal IColumns Target { get; private set; } = Columns.Empty;
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
            Target = Target.Union(column);
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

        public RowInput<T> WithFlush(Column column, Func<T, ColumnValueBag> valueBagGetter)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (valueBagGetter == null)
                throw new ArgumentNullException(nameof(valueBagGetter));

            VerifyNotSealed();
            Target = Target.Union(column);
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

        internal override void FlushCore(T element)
        {
            Debug.Assert(CurrentRow != null);
            var currentRow = CurrentRow;
            if (currentRow != element.GetRowPresenter())
                throw new InvalidOperationException(DiagnosticMessages.RowInput_FlushCurrentRowOnly);
            var flushed = DoFlush(currentRow, element);
            if (flushed)
                InputManager.MakeProgress(this);
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

        public FlushErrorMessage GetFlushError(RowPresenter rowPresenter)
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

        public IColumnValidationMessages GetErrors(RowPresenter rowPresenter)
        {
            var result = ColumnValidationMessages.Empty;
            result = AddValidationMessages(result, InputManager.RowValidationErrors, rowPresenter, x => IsVisible(x, rowPresenter, true));
            result = AddAsyncValidationMessages(result, rowPresenter, ValidationSeverity.Error);
            result = AddValidationMessages(result, InputManager.AssignedRowValidationResults, rowPresenter, x => x.Severity == ValidationSeverity.Error && IsVisible(x, rowPresenter, false));
            return result;
        }

        public IColumnValidationMessages GetWarnings(RowPresenter rowPresenter)
        {
            var result = ColumnValidationMessages.Empty;
            result = AddValidationMessages(result, InputManager.RowValidationWarnings, rowPresenter, x => IsVisible(x, rowPresenter, true));
            result = AddAsyncValidationMessages(result, rowPresenter, ValidationSeverity.Warning);
            result = AddValidationMessages(result, InputManager.AssignedRowValidationResults, rowPresenter, x => x.Severity == ValidationSeverity.Warning && IsVisible(x, rowPresenter, false));
            return result;
        }

        private bool IsVisible(ColumnValidationMessage validationMessage, RowPresenter rowPresenter, bool progressVisible)
        {
            var source = validationMessage.Source;
            return source.Overlaps(Target) && InputManager.RowValidationProgress.IsVisible(rowPresenter, source) == progressVisible;
        }

        private static IColumnValidationMessages AddValidationMessages(IColumnValidationMessages result, IRowValidationResults dictionary, RowPresenter rowPresenter, Func<ColumnValidationMessage, bool> predict)
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

        private IColumnValidationMessages AddAsyncValidationMessages(IColumnValidationMessages result, RowPresenter rowPresenter, ValidationSeverity severity)
        {
            var asyncValidators = Template.RowAsyncValidators;
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
            element.RefreshValidation(() => GetFlushError(element), () => GetErrors(rowPresenter), () => GetWarnings(rowPresenter));
        }

        private Action<T, RowPresenter, FlushErrorMessage> _onRefresh;
        internal void Refresh(T element, RowPresenter rowPresenter)
        {
            if (!IsFlushing && GetFlushError(element) == null)
                RowBinding.Refresh(element, rowPresenter);
            if (_onRefresh != null)
                _onRefresh(element, rowPresenter, GetFlushError(element));
            RefreshValidation(element, rowPresenter);
        }

        public RowInput<T> WithRefreshAction(Action<T, RowPresenter, FlushErrorMessage> onRefresh)
        {
            VerifyNotSealed();
            _onRefresh = onRefresh;
            return this;
        }

        public RowInput<T> AddAsyncValidator(Func<Task<IColumnValidationMessages>> action, Action postAction = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            VerifyNotSealed();

            var asyncValidator = RowAsyncValidator.Create<T>(this, action, postAction);
            Template.InternalRowAsyncValidators = Template.InternalRowAsyncValidators.Add(asyncValidator);
            return this;
        }

        public RowBinding<T> EndInput()
        {
            return RowBinding;
        }

        private IRowAsyncValidators _asyncValidators;
        public IRowAsyncValidators AsyncValidators
        {
            get
            {
                if (InputManager == null)
                    return null;

                if (_asyncValidators == null)
                    _asyncValidators = Template.RowAsyncValidators.Where(x => x.RowInput == this);
                return _asyncValidators;
            }
        }
    }
}
