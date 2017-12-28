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
            {
                _progress = new Dictionary<RowPresenter, IColumns>();
                _valueChanged = new Dictionary<RowPresenter, IColumns>();
            }
        }

        private InputManager _inputManager;
        private Dictionary<RowPresenter, IColumns> _progress;
        private Dictionary<RowPresenter, IColumns> _valueChanged;

        internal void Reset()
        {
            if (_progress != null)
            {
                _progress.Clear();
                _valueChanged.Clear();
            }
        }

        internal void UpdateProgress<T>(RowInput<T> rowInput, bool valueChanged, bool makeProgress)
            where T : UIElement, new()
        {
            Debug.Assert(valueChanged || makeProgress);

            if (_progress == null)
                return;

            var currentRow = _inputManager.CurrentRow;
            Debug.Assert(currentRow != null);
            var sourceColumns = rowInput.Target;
            if (sourceColumns == null || sourceColumns.Count == 0)
                return;

            if (makeProgress)
            {
                var columns = GetProgress(_progress, currentRow);
                if (columns == null || (!valueChanged && !Exists(_valueChanged, currentRow, sourceColumns)))
                    return;
                _progress[currentRow] = columns.Union(sourceColumns);
            }
            else
                _valueChanged[currentRow] = GetProgress(_valueChanged, currentRow).Union(sourceColumns);
        }

        internal void ShowAll(RowPresenter rowPresenter)
        {
            if (_progress == null)
                return;

            if (_valueChanged.ContainsKey(rowPresenter))
                _valueChanged.Remove(rowPresenter);
            _progress[rowPresenter] = null;
        }

        internal void OnRowDisposed(RowPresenter rowPresenter)
        {
            if (_progress == null)
                return;

            if (_progress.ContainsKey(rowPresenter))
                _progress.Remove(rowPresenter);
            if (_valueChanged.ContainsKey(rowPresenter))
                _valueChanged.Remove(rowPresenter);
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

            return Exists(_progress, rowPresenter, columns);
        }

        private static bool Exists(Dictionary<RowPresenter, IColumns> progressByRow, RowPresenter rowPresenter, IColumns columns)
        {
            Debug.Assert(progressByRow != null);
            Debug.Assert(rowPresenter != null);
            Debug.Assert(columns != null && columns.Count > 0);

            var progress = GetProgress(progressByRow, rowPresenter);
            return progress == null ? true : progress.IsSupersetOf(columns);
        }

        private static IColumns GetProgress(Dictionary<RowPresenter, IColumns> progressByRow, RowPresenter rowPresenter)
        {
            Debug.Assert(progressByRow != null);
            IColumns result;
            if (progressByRow.TryGetValue(rowPresenter, out result))
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
