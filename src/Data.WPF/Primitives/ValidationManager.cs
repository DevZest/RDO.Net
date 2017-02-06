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
        public IValidationDictionary Errors { get; private set; } = ValidationDictionary.Empty;
        public IValidationDictionary Warnings { get; private set; } = ValidationDictionary.Empty;

        private void ClearValidationMessages()
        {
            Errors = Warnings = ValidationDictionary.Empty;
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

        private static IValidationMessageGroup GetValidationMessages(IValidationDictionary dictionary, RowPresenter rowPresenter, IColumnSet columns)
        {
            Debug.Assert(dictionary != null);

            IValidationMessageGroup messages;
            if (!dictionary.TryGetValue(rowPresenter, out messages))
                return ValidationMessageGroup.Empty;

            var result = ValidationMessageGroup.Empty;
            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (message.Source.SetEquals(columns))
                    result = result.Add(message);
            }

            return result;
        }

        private bool HasNoError(RowPresenter rowPresenter, IColumnSet columns)
        {
            if (Errors.Count == 0)
                return true;

            IValidationMessageGroup messages;
            if (!Errors.TryGetValue(rowPresenter, out messages))
                return true;

            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (message.Source.SetEquals(columns))
                    return false;
            }

            return true;
        }

        internal IValidationMessageGroup GetErrors<T>(RowPresenter rowPresenter, RowInput<T> rowInput)
            where T : UIElement, new()
        {
            if (!Progress.IsVisible(rowPresenter, rowInput.SourceColumns))
                return ValidationMessageGroup.Empty;

            return GetValidationMessages(Errors, rowPresenter, rowInput.SourceColumns);
        }

        internal IValidationMessageGroup GetWarnings<T>(RowPresenter rowPresenter, RowInput<T> rowInput)
            where T : UIElement, new()
        {
            if (!Progress.IsVisible(rowPresenter, rowInput.SourceColumns))
                return ValidationMessageGroup.Empty;

            return GetValidationMessages(Warnings, rowPresenter, rowInput.SourceColumns);
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
            Errors = Validate(Errors, ValidationSeverity.Error, Template.MaxValidationErrors);
            Warnings = Validate(Warnings, ValidationSeverity.Warning, Template.MaxValidationWarnings);
        }

        private IValidationDictionary Validate(IValidationDictionary result, ValidationSeverity severity, int maxEntries)
        {
            if (CurrentRow != null)
            {
                var messages = CurrentRow.DataRow.Validate(severity);
                if (messages.Count > 0)
                    result = result.Add(CurrentRow, messages);
            }

            if (ValidationScope == ValidationScope.AllRows)
            {
                foreach (var row in Rows)
                {
                    if (row == CurrentRow)
                        continue;

                    var messages = row.DataRow.Validate(severity);
                    if (messages.Count > 0)
                        result = result.Add(row, messages);

                    if (result.Count == maxEntries)
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

            if (Errors.ContainsKey(rowPresenter))
                Errors = Errors.Remove(rowPresenter);

            if (Warnings.ContainsKey(rowPresenter))
                Warnings = Warnings.Remove(rowPresenter);

            foreach (var rowBinding in Template.RowBindings)
                rowBinding.OnRowDisposed(rowPresenter);
        }
    }
}
