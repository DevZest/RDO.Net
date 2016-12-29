using DevZest.Data.Windows.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class ValidationManager : ElementManager
    {
        protected ValidationManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy, bool emptyBlockViewList)
            : base(template, dataSet, where, orderBy, emptyBlockViewList)
        {
            if (ValidationMode == ValidationMode.Progressive)
                _progress = new Dictionary<Windows.RowPresenter, IValidationSource<Column>>();
        }

        private bool _showAll;
        private Dictionary<RowPresenter, IValidationSource<Column>> _progress;
        private Dictionary<RowPresenter, IReadOnlyList<ValidationMessage<Column>>> _errors;
        private Dictionary<RowPresenter, IReadOnlyList<ValidationMessage<Column>>> _warnings;

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

        internal bool ShouldRunAsyncValidator(RowPresenter rowPresenter, IValidationSource<Column> validationSource)
        {
            return IsVisible(rowPresenter, validationSource) && HasNoError(rowPresenter, validationSource);
        }

        private bool IsVisible(RowPresenter rowPresenter, IValidationSource<Column> validationSource)
        {
            if (_showAll)
                return true;

            if (_progress == null)
                return false;

            if (validationSource.Count == 0)
                return false;
            return GetProgress(rowPresenter).IsSupersetOf(validationSource);
        }

        public ICollection<KeyValuePair<RowPresenter, IReadOnlyList<ValidationMessage<Column>>>> Errors
        {
            get
            {
                if (_errors != null)
                    return _errors;
                else
                    return EmptyReadOnlyDictionary<RowPresenter, IReadOnlyList<ValidationMessage<Column>>>.Singleton;
            }
        }

        public ICollection<KeyValuePair<RowPresenter, IReadOnlyList<ValidationMessage<Column>>>> Warnings
        {
            get
            {
                if (_warnings != null)
                    return _warnings;
                else
                    return EmptyReadOnlyDictionary<RowPresenter, IReadOnlyList<ValidationMessage<Column>>>.Singleton;
            }
        }

        private static IReadOnlyList<ValidationMessage> GetValidationMessages(Dictionary<RowPresenter, IReadOnlyList<ValidationMessage<Column>>> dictionary, RowPresenter rowPresenter, IValidationSource<Column> validationSource)
        {
            if (dictionary == null)
                return Array<ValidationMessage>.Empty;

            IReadOnlyList<ValidationMessage<Column>> messages;
            if (!dictionary.TryGetValue(rowPresenter, out messages))
                return Array<ValidationMessage>.Empty;

            List<ValidationMessage> result = null;
            foreach (var message in messages)
            {
                if (message.Source.SetEquals(validationSource))
                    result = result.AddItem(new ValidationMessage(message.Id, message.Description, message.Severity));
            }

            return result.ToReadOnlyList();
        }

        private bool HasNoError(RowPresenter rowPresenter, IValidationSource<Column> validationSource)
        {
            if (_errors == null)
                return true;

            IReadOnlyList<ValidationMessage<Column>> messages;
            if (!_errors.TryGetValue(rowPresenter, out messages))
                return true;

            foreach (var message in messages)
            {
                if (message.Source.SetEquals(validationSource))
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
            get { return Template.RowBindings.HasPreValidatorError(); }
        }

        private IValidationSource<Column> GetProgress(RowPresenter rowPresenter)
        {
            Debug.Assert(_progress != null);
            IValidationSource<Column> result;
            if (_progress.TryGetValue(rowPresenter, out result))
                return result;
            return ValidationSource<Column>.Empty;
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

        private Dictionary<RowPresenter, IReadOnlyList<ValidationMessage<Column>>> Validate(Dictionary<RowPresenter, IReadOnlyList<ValidationMessage<Column>>> result, ValidationSeverity severity, int maxEntries)
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

        protected override void OnCurrentRowChanged(RowPresenter oldValue)
        {
            base.OnCurrentRowChanged(oldValue);
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

        public void SetValidationResult(ValidationResult validationResult)
        {

        }
    }
}
