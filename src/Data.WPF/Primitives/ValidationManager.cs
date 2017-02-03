using DevZest.Data.Windows.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class ValidationManager : ElementManager
    {
        protected ValidationManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy, bool emptyContainerViewList)
            : base(template, dataSet, where, orderBy, emptyContainerViewList)
        {
            if (ValidationMode == ValidationMode.Progressive)
                _progress = new Dictionary<Windows.RowPresenter, IColumnSet>();
        }

        private bool _showAll;
        private Dictionary<RowPresenter, IColumnSet> _progress;
        private Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>> _errors;
        private Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>> _warnings;

        private void ClearValidationMessages()
        {
            if (_errors != null)
                _errors.Clear();
            if (_warnings != null)
                _warnings.Clear();
        }

        protected override void Reload()
        {
            base.Reload();
            Reset();
        }

        private void Reset()
        {
            _showAll = false;

            if (_progress != null)
                _progress.Clear();

            if (ValidationMode == ValidationMode.Implicit)
                Validate(true);
            else
                ClearValidationMessages();
        }

        internal bool ShouldRunAsyncValidator(RowPresenter rowPresenter, IColumnSet columns)
        {
            return IsVisible(rowPresenter, columns) && HasNoError(rowPresenter, columns);
        }

        private bool IsVisible(RowPresenter rowPresenter, IColumnSet columns)
        {
            if (_showAll)
                return true;

            if (_progress == null)
                return false;

            if (columns.Count == 0)
                return false;
            return GetProgress(rowPresenter).IsSupersetOf(columns);
        }

        public IReadOnlyDictionary<RowPresenter, IReadOnlyList<ValidationMessage>> Errors
        {
            get
            {
                if (_errors != null)
                    return _errors;
                else
                    return EmptyReadOnlyDictionary<RowPresenter, IReadOnlyList<ValidationMessage>>.Singleton;
            }
        }

        public IReadOnlyDictionary<RowPresenter, IReadOnlyList<ValidationMessage>> Warnings
        {
            get
            {
                if (_warnings != null)
                    return _warnings;
                else
                    return EmptyReadOnlyDictionary<RowPresenter, IReadOnlyList<ValidationMessage>>.Singleton;
            }
        }

        private static IReadOnlyList<ValidationMessage> GetValidationMessages(Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>> dictionary, RowPresenter rowPresenter, IColumnSet columns)
        {
            if (dictionary == null)
                return Array<ValidationMessage>.Empty;

            IReadOnlyList<ValidationMessage> messages;
            if (!dictionary.TryGetValue(rowPresenter, out messages))
                return Array<ValidationMessage>.Empty;

            List<ValidationMessage> result = null;
            foreach (var message in messages)
            {
                if (message.Source.SetEquals(columns))
                    result = result.AddItem(message);
            }

            return result.ToReadOnlyList();
        }

        private bool HasNoError(RowPresenter rowPresenter, IColumnSet columns)
        {
            if (_errors == null)
                return true;

            IReadOnlyList<ValidationMessage> messages;
            if (!_errors.TryGetValue(rowPresenter, out messages))
                return true;

            foreach (var message in messages)
            {
                if (message.Source.SetEquals(columns))
                    return false;
            }

            return true;
        }

        internal IReadOnlyList<ValidationMessage> GetErrors<T>(RowPresenter rowPresenter, RowInput<T> rowInput)
            where T : UIElement, new()
        {
            if (!IsVisible(rowPresenter, rowInput.SourceColumns))
                return Array<ValidationMessage>.Empty;

            return GetValidationMessages(_errors, rowPresenter, rowInput.SourceColumns);
        }

        internal IReadOnlyList<ValidationMessage> GetWarnings<T>(RowPresenter rowPresenter, RowInput<T> rowInput)
            where T : UIElement, new()
        {
            if (!IsVisible(rowPresenter, rowInput.SourceColumns))
                return Array<ValidationMessage>.Empty;

            return GetValidationMessages(_warnings, rowPresenter, rowInput.SourceColumns);
        }

        public bool HasPreValidatorError
        {
            get { return Template.RowBindings.HasInputError(); }
        }

        private IColumnSet GetProgress(RowPresenter rowPresenter)
        {
            Debug.Assert(_progress != null);
            IColumnSet result;
            if (_progress.TryGetValue(rowPresenter, out result))
                return result;
            return ColumnSet.Empty;
        }

        internal void MakeProgress<T>(RowPresenter rowPresenter, RowInput<T> rowInput)
            where T : UIElement, new()
        {
            var sourceColumns = rowInput.SourceColumns;

            if (!_showAll && _progress != null)
            {
                var progress = GetProgress(rowPresenter).Union(rowInput.SourceColumns);
                if (progress.Count > 0)
                    _progress[rowPresenter] = progress;
            }

            if (ValidationMode != ValidationMode.Explicit)
                Validate(false);
            rowInput.RunAsyncValidator(rowPresenter);
            InvalidateElements();
        }

        private ValidationMode ValidationMode
        {
            get { return Template.ValidationMode; }
        }

        private ValidationScope ValidationScope
        {
            get { return Template.ValidationScope; }
        }

        public void Validate()
        {
            Validate(true);
            InvalidateElements();
        }

        private void Validate(bool showAll)
        {
            if (showAll)
                ShowAll();

            ClearValidationMessages();
            _errors = Validate(_errors, ValidationSeverity.Error, Template.MaxValidationErrors);
            _warnings = Validate(_warnings, ValidationSeverity.Warning, Template.MaxValidationWarnings);
        }

        private void ShowAll()
        {
            if (_showAll)
                return;

            _showAll = true;
            _progress.Clear();
        }

        private Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>> Validate(Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>> result, ValidationSeverity severity, int maxEntries)
        {
            if (CurrentRow != null)
            {
                var messages = CurrentRow.DataRow.Validate(severity);
                if (messages.Count > 0)
                    result = result.AddEntry(CurrentRow, messages);
            }

            if (ValidationScope == ValidationScope.AllRows)
            {
                foreach (var row in Rows)
                {
                    if (row == CurrentRow)
                        continue;

                    var messages = row.DataRow.Validate(severity);
                    if (messages.Count > 0)
                        result = result.AddEntry(row, messages);

                    if (result.GetCount() == maxEntries)
                        break;
                }
            }

            return result;
        }

        protected override void OnCurrentRowChanged(RowPresenter oldValue, bool reload)
        {
            base.OnCurrentRowChanged(oldValue, reload);
            if (_progress != null)
            {
                if (ValidationScope == ValidationScope.CurrentRow)
                    _progress.Clear();
            }
        }

        protected override void DisposeRow(RowPresenter rowPresenter)
        {
            base.DisposeRow(rowPresenter);

            if (_progress != null && _progress.ContainsKey(rowPresenter))
                _progress.Remove(rowPresenter);

            if (_errors.ContainsKey(rowPresenter))
                _errors.Remove(rowPresenter);

            if (_warnings.ContainsKey(rowPresenter))
                _warnings.Remove(rowPresenter);

            foreach (var rowBinding in Template.RowBindings)
                rowBinding.OnRowDisposed(rowPresenter);
        }
    }
}
