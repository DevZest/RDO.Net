using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public sealed class RowValidation
    {
        internal RowValidation(InputManager inputManager)
        {
            _inputManager = inputManager;
            if (Mode == ValidationMode.Progressive)
                _progress = new Dictionary<RowPresenter, IColumns>();
        }

        private InputManager _inputManager;
        private Dictionary<RowPresenter, IColumns> _progress;

        internal void Reset()
        {
            if (_progress != null)
                _progress.Clear();
        }

        internal void MakeProgress<T>(RowInput<T> rowInput)
            where T : UIElement, new()
        {
            var currentRow = _inputManager.CurrentRow;
            Debug.Assert(currentRow != null);
            var sourceColumns = rowInput.Target;

            if (_progress != null)
            {
                var columns = GetProgress(currentRow);
                if (columns == null)
                    return;
                _progress[currentRow] = columns.Union(rowInput.Target);
            }
        }

        internal void ShowAll(RowPresenter rowPresenter)
        {
            if (_progress != null)
                _progress[rowPresenter] = null;
        }

        internal void OnRowDisposed(RowPresenter rowPresenter)
        {
            if (_progress != null && _progress.ContainsKey(rowPresenter))
                _progress.Remove(rowPresenter);
        }

        public ValidationMode Mode
        {
            get { return _inputManager.RowValidationMode; }
        }

        public bool IsVisible(RowPresenter rowPresenter, IColumns columns)
        {
            Check.NotNull(rowPresenter, nameof(rowPresenter));

            if (columns == null || columns.Count == 0)
                return false;

            if (_progress == null)
                return true;

            var progress = GetProgress(rowPresenter);
            return progress == null ? true : progress.IsSupersetOf(columns);
        }

        private IColumns GetProgress(RowPresenter rowPresenter)
        {
            Debug.Assert(_progress != null);
            IColumns result;
            if (_progress.TryGetValue(rowPresenter, out result))
                return result;
            return Columns.Empty;
        }

        public IReadOnlyList<FlushErrorMessage> FlushErrors
        {
            get { return _inputManager.RowFlushErrors; }
        }

        public IReadOnlyDictionary<RowPresenter, IColumnValidationMessages> Errors
        {
            get { return _inputManager.RowValidationErrors; }
        }

        public IReadOnlyDictionary<RowPresenter, IColumnValidationMessages> Warnings
        {
            get { return _inputManager.RowValidationWarnings; }
        }

        public IColumnValidationMessages CurrentRowErrors
        {
            get { return _inputManager.CurrentRowErrors; }
        }

        public IColumnValidationMessages CurrentRowWarnings
        {
            get { return _inputManager.CurrentRowWarnings; }
        }

        private Template Template
        {
            get { return _inputManager.Template; }
        }

        public IRowAsyncValidators AsyncValidators
        {
            get { return Template.RowAsyncValidators; }
        }

        public void Assign(IDataRowValidationResults validationResults)
        {
            if (validationResults == null)
                throw new ArgumentNullException(nameof(validationResults));
            _inputManager.Assign(validationResults);
        }

        public void Assign(IRowValidationResults validationResults)
        {
            if (validationResults == null)
                throw new ArgumentNullException(nameof(validationResults));
            _inputManager.Assign(validationResults);
        }

        public IReadOnlyDictionary<RowPresenter, IColumnValidationMessages> AssignedResults
        {
            get { return _inputManager.AssignedRowValidationResults; }
        }

        private RowPresenter CurrentRow
        {
            get { return _inputManager.CurrentRow; }
        }

        public void FlushCurrentRow()
        {
            _inputManager.FlushCurrentRow();
        }

        public void Validate(int errorLimit = 1, int warningLimit = 0)
        {
            if (errorLimit < 1)
                throw new ArgumentOutOfRangeException(nameof(errorLimit));
            if (warningLimit < 0)
                throw new ArgumentOutOfRangeException(nameof(warningLimit));

            if (CurrentRow == null)
                return;

            var errors = 0;
            var warnings = 0;
            var moreToValidate = Validate(CurrentRow, ref errors, errorLimit, ref warnings, warningLimit);
            if (moreToValidate)
            {
                foreach (var rowPresenter in _inputManager.Rows)
                {
                    if (rowPresenter == CurrentRow || rowPresenter.IsVirtual)
                        continue;

                    moreToValidate = Validate(rowPresenter, ref errors, errorLimit, ref warnings, warningLimit);
                    if (!moreToValidate)
                        break;
                }
            }

            _inputManager.InvalidateView();
        }

        private bool Validate(RowPresenter rowPresenter, ref int errors, int errorLimit, ref int warnings, int warningLimit)
        {
            Debug.Assert(rowPresenter != null);
            rowPresenter.Validate(false);
            if (Errors.ContainsKey(rowPresenter))
                errors++;
            if (Warnings.ContainsKey(rowPresenter))
                warnings++;
            return errors < errorLimit || warnings < warningLimit;
        }
    }
}
