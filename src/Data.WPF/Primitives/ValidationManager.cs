using DevZest.Data.Windows.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class ValidationManager : ElementManager
    {
        private sealed class InputErrorCollection : KeyedCollection<UIElement, ViewInputError>
        {
            protected override UIElement GetKeyForItem(ViewInputError item)
            {
                return item.Source;
            }
        }

        protected ValidationManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy, bool emptyContainerViewList)
            : base(template, dataSet, where, orderBy, emptyContainerViewList)
        {
            Progress = new ValidationProgress(this);
        }

        private InputErrorCollection _scalarInputErrors;
        private InputErrorCollection InternalScalarInputErrors
        {
            get
            {
                if (_scalarInputErrors == null)
                    _scalarInputErrors = new InputErrorCollection();
                return _scalarInputErrors;
            }
        }

        internal ViewInputError GetScalarInputError(UIElement element)
        {
            return GetInputError(_scalarInputErrors, element);
        }

        private static ViewInputError GetInputError(InputErrorCollection inputErrors, UIElement element)
        {
            if (inputErrors == null)
                return null;
            return inputErrors.Contains(element) ? inputErrors[element] : null;
        }

        internal void SetScalarInputError(UIElement element, ViewInputError inputError)
        {
            SetInputError(InternalScalarInputErrors, element, inputError);
        }

        private static void SetInputError(InputErrorCollection inputErrors, UIElement element, ViewInputError inputError)
        {
            Debug.Assert(inputErrors != null);
            inputErrors.Remove(element);
            if (inputError != null)
                inputErrors.Add(inputError);
        }

        public IReadOnlyList<ViewInputError> ScalarInputErrors
        {
            get
            {
                if (_scalarInputErrors == null)
                    return Array<ViewInputError>.Empty;
                return _scalarInputErrors;
            }
        }

        private InputErrorCollection _rowInputErrors;
        private InputErrorCollection InternalRowInputErrors
        {
            get
            {
                if (_rowInputErrors == null)
                    _rowInputErrors = new InputErrorCollection();
                return _rowInputErrors;
            }
        }

        internal ViewInputError GetRowInputError(UIElement element)
        {
            return GetInputError(_rowInputErrors, element);
        }

        internal void SetRowInputError(UIElement element, ViewInputError inputError)
        {
            SetInputError(InternalScalarInputErrors, element, inputError);
        }

        public IReadOnlyList<ViewInputError> RowInputErrors
        {
            get
            {
                if (_rowInputErrors == null)
                    return Array<ViewInputError>.Empty;
                return _rowInputErrors;
            }
        }

        public ValidationProgress Progress { get; private set; }
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
            Progress.Reset();

            if (ValidationMode == ValidationMode.Implicit)
                Validate(true);
            else
                ClearValidationMessages();
        }

        internal bool ShouldRunAsyncValidator(RowPresenter rowPresenter, IColumnSet columns)
        {
            return Progress.IsVisible(rowPresenter, columns) && HasNoError(rowPresenter, columns);
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
            if (!Progress.IsVisible(rowPresenter, rowInput.SourceColumns))
                return Array<ValidationMessage>.Empty;

            return GetValidationMessages(_errors, rowPresenter, rowInput.SourceColumns);
        }

        internal IReadOnlyList<ValidationMessage> GetWarnings<T>(RowPresenter rowPresenter, RowInput<T> rowInput)
            where T : UIElement, new()
        {
            if (!Progress.IsVisible(rowPresenter, rowInput.SourceColumns))
                return Array<ValidationMessage>.Empty;

            return GetValidationMessages(_warnings, rowPresenter, rowInput.SourceColumns);
        }

        internal void MakeProgress<T>(RowPresenter rowPresenter, RowInput<T> rowInput)
            where T : UIElement, new()
        {
            Progress.MakeProgress(rowPresenter, rowInput);
            if (ValidationMode != ValidationMode.Explicit)
                Validate(false);
            rowInput.RunAsyncValidator(rowPresenter);
            InvalidateElements();
        }

        internal ValidationMode ValidationMode
        {
            get { return Template.ValidationMode; }
        }

        internal ValidationScope ValidationScope
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
                Progress.ShowAll();

            ClearValidationMessages();
            _errors = Validate(_errors, ValidationSeverity.Error, Template.MaxValidationErrors);
            _warnings = Validate(_warnings, ValidationSeverity.Warning, Template.MaxValidationWarnings);
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
            Progress.OnCurrentRowChanged();
        }

        protected override void DisposeRow(RowPresenter rowPresenter)
        {
            base.DisposeRow(rowPresenter);

            Progress.OnRowDisposed(rowPresenter);

            if (_errors.ContainsKey(rowPresenter))
                _errors.Remove(rowPresenter);

            if (_warnings.ContainsKey(rowPresenter))
                _warnings.Remove(rowPresenter);

            foreach (var rowBinding in Template.RowBindings)
                rowBinding.OnRowDisposed(rowPresenter);
        }
    }
}
