using DevZest.Data.Windows.Primitives;
using DevZest.Data.Windows.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
        private HashSet<RowPresenter> _progress;
        private Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>> _validationErrors = new Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>>();
        private Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>> _validationWarnings = new Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>>();
        private Func<RowPresenter, Task<ValidationMessage>> _asyncValidator;
        private Dictionary<RowPresenter, AsyncValidationState> _asyncValidatorStates;
        private HashSet<RowPresenter> _pendingAsyncValidators;
        private Dictionary<RowPresenter, ValidationMessage> _asyncValidationMessages;

        private ValidationMode ValidationMode
        {
            get { return Template.ValidationMode; }
        }

        private ValidationScope ValidationScope
        {
            get { return Template.ValidationScope; }
        }

        internal override void Seal(Binding binding)
        {
            base.Seal(binding);
            if (ValidationMode == ValidationMode.Progressive)
                _progress = new HashSet<RowPresenter>();
        }

        private void MakeProgress()
        {
            var currentRow = CurrentRow;
            Debug.Assert(currentRow != null);
            if (_progress != null && !_progress.Contains(currentRow))
                _progress.Add(currentRow);
        }

        internal void BypassProgressiveMode()
        {
            _progress = null;
        }

        internal void OnCurrentRowChanged()
        {
            if (_progress != null)
            {
                if (ValidationScope == ValidationScope.CurrentRow)
                    _progress.Clear();
            }
            else if (ValidationMode == ValidationMode.Progressive)
                _progress = new HashSet<RowPresenter>();
        }

        internal void OnRowDisposed(RowPresenter rowPresenter)
        {
            if (_progress != null && _progress.Contains(rowPresenter))
                _progress.Remove(rowPresenter);
        }

        private RowPresenter CurrentRow
        {
            get { return ValidationManager == null ? null : ValidationManager.CurrentRow; }
        }

        internal void SetValidationResult(RowValidationResult errors, RowValidationResult warnings)
        {
            if (_progress != null && _progress.Count == 0)
                return;

            bool errorsChanged = SetValidationResult(_validationErrors, errors);
            bool warningsChanged = SetValidationResult(_validationWarnings, warnings);
            if (errorsChanged || warningsChanged)
                ValidationManager.InvalidateElements();
        }

        private bool SetValidationResult(Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>> messages, RowValidationResult validationResult)
        {
            bool oldIsEmpty = messages.Count > 0;
            messages.Clear();
            foreach (var entry in validationResult.Entries)
            {
                if (_progress != null && !_progress.Contains(entry.Row))
                    continue;
                var filteredMessages = Filter(entry.Messages);
                if (filteredMessages.Count > 0)
                    messages.Add(entry.Row, filteredMessages);
            }

            return !oldIsEmpty || messages.Count > 0;
        }

        private IReadOnlyList<ValidationMessage> Filter(IReadOnlyList<ValidationMessage<IColumnSet>> messages)
        {
            List<ValidationMessage> result = null;
            foreach (var message in messages)
            {
                throw new NotImplementedException();
                //if (message.Source.IntersectsWith(Columns))
                //    result = result.AddItem(new ValidationMessage(message.Id, message.Description, message.Severity));
            }

            return result.ToReadOnlyList();
        }

        internal IReadOnlyList<ValidationMessage> GetErrors(RowPresenter rowPresenter)
        {
            Debug.Assert(rowPresenter != null);

            List<ValidationMessage> result = null;

            if (rowPresenter == CurrentRow && HasPreValidatorError)
                result = result.AddItem(PreValidatorError);

            IReadOnlyList<ValidationMessage> errors;
            if (_validationErrors.TryGetValue(rowPresenter, out errors))
                result = result.AddItems(errors);

            return result.ToReadOnlyList();
        }

        internal IReadOnlyList<ValidationMessage> GetWarnings(RowPresenter rowPresenter)
        {
            Debug.Assert(rowPresenter != null);

            List<ValidationMessage> result = null;

            IReadOnlyList<ValidationMessage> warnings;
            if (_validationWarnings.TryGetValue(rowPresenter, out warnings))
                result = result.AddItems(warnings);

            return result.ToReadOnlyList();
        }

        public RowInput<T> WithPreValidator(Func<T, string> preValidator, Trigger<T> preValidatorTrigger)
        {
            SetPreValidator(preValidator, preValidatorTrigger);
            return this;
        }

        public RowInput<T> WithAsyncValidator(Func<RowPresenter, Task<ValidationMessage>> asyncValidator)
        {
            VerifyNotSealed();
            _asyncValidator = asyncValidator;
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

        internal override void FlushCore(T element)
        {
            Debug.Assert(CurrentRow != null && CurrentRow == element.GetRowPresenter());
            var currentRow = CurrentRow;
            var flushed = DoFlush(currentRow, element);
            if (flushed)
            {
                MakeProgress();
                if (ValidationMode != ValidationMode.Explicit)
                    ValidationManager.Validate(false);
            }
            throw new NotImplementedException();
        }

        private bool DoFlush(RowPresenter rowPresenter, T element)
        {
            bool result = false;
            foreach (var flush in _flushFuncs)
            {
                var flushed = flush(rowPresenter, element);
                if (flushed)
                    result = true;
            }
            return result;
        }
    }
}
