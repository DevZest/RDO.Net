using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public sealed class ScalarValidation
    {
        internal ScalarValidation(InputManager inputManager)
        {
            _inputManager = inputManager;
            if (Mode == ValidationMode.Progressive)
            {
                _progress = Scalars.Empty;
                _valueChanged = Scalars.Empty;
            }
            if (Mode == ValidationMode.Implicit)
                Validate();
        }

        private InputManager _inputManager;

        private Template Template
        {
            get { return _inputManager.Template; }
        }

        private DataPresenter DataPresenter
        {
            get { return _inputManager.DataPresenter; }
        }

        private void InvalidateView()
        {
            _inputManager.InvalidateView();
        }

        private FlushErrorCollection _flushErrors;
        private FlushErrorCollection InternalFlushErrors
        {
            get
            {
                if (_flushErrors == null)
                    _flushErrors = new FlushErrorCollection(_inputManager);
                return _flushErrors;
            }
        }

        public IReadOnlyList<FlushErrorMessage> FlushErrors
        {
            get
            {
                if (_flushErrors == null)
                    return Array<FlushErrorMessage>.Empty;
                return _flushErrors;
            }
        }

        internal FlushErrorMessage GetFlushError(UIElement element)
        {
            return _flushErrors.GetFlushError(element);
        }

        internal void SetFlushError(UIElement element, FlushErrorMessage inputError)
        {
            InternalFlushErrors.SetFlushError(element, inputError);
        }

        private IScalarValidationMessages _validationErrors = ScalarValidationMessages.Empty;
        private IScalarValidationMessages _validationWarnings = ScalarValidationMessages.Empty;

        public IReadOnlyList<ScalarValidationMessage> ValidationErrors
        {
            get { return _validationErrors; }
        }

        public IReadOnlyList<ScalarValidationMessage> ValidationWarnings
        {
            get { return _validationWarnings; }
        }

        public IScalarValidationMessages GetErrors(IScalars scalars)
        {
            var result = ScalarValidationMessages.Empty;
            if (IsVisible(scalars))
                result = AddValidationMessages(result, _validationErrors, scalars);
            result = AddAsyncValidationMessages(result, ValidationSeverity.Error, scalars);
            return result;
        }

        public IScalarValidationMessages GetWarnings(IScalars scalars)
        {
            var result = ScalarValidationMessages.Empty;
            if (IsVisible(scalars))
                result = AddValidationMessages(result, _validationWarnings, scalars);
            result = AddAsyncValidationMessages(result, ValidationSeverity.Warning, scalars);
            return result;
        }

        private static IScalarValidationMessages AddValidationMessages(IScalarValidationMessages result, IScalarValidationMessages messages, IScalars scalars)
        {
            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (message.Source.SetEquals(scalars))
                    result = result.Add(message);
            }
            return result;
        }

        private IScalarValidationMessages AddAsyncValidationMessages(IScalarValidationMessages result, ValidationSeverity severity, IScalars scalars)
        {
            var asyncValidators = Template.ScalarAsyncValidators;
            for (int i = 0; i < asyncValidators.Count; i++)
            {
                var asyncValidator = asyncValidators[i];
                var messages = severity == ValidationSeverity.Error ? asyncValidator.Errors : asyncValidator.Warnings;
                result = AddValidationMessages(result, messages, scalars);
            }

            return result;
        }

        public void Validate()
        {
            Validate(true);
            InvalidateView();
        }

        private void Validate(bool showAll)
        {
            if (showAll)
                ShowAll();

            ClearValidationMessages();
            var messages = _inputManager.PerformValidateScalars();
            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (message.Severity == ValidationSeverity.Error)
                    _validationErrors = _validationErrors.Add(message);
                else
                    _validationWarnings = _validationWarnings.Add(message);
            }
            _validationErrors = _validationErrors.Seal();
            _validationWarnings = _validationWarnings.Seal();
        }

        private void ClearValidationMessages()
        {
            _validationErrors = _validationWarnings = ScalarValidationMessages.Empty;
        }

        private bool _showAll;
        private IScalars _progress;
        private IScalars _valueChanged;

        internal void OnFlushed<T>(ScalarInput<T> scalarInput, bool makeProgress, bool valueChanged)
            where T : UIElement, new()
        {
            if (!makeProgress && !valueChanged)
                return;

            if (Mode != ValidationMode.Explicit)
                Validate(false);
            if (UpdateProgress(scalarInput, valueChanged, makeProgress))
                OnProgress(scalarInput);
            InvalidateView();
        }

        private void OnProgress<T>(ScalarInput<T> scalarInput)
            where T : UIElement, new()
        {
            if (Mode == ValidationMode.Explicit)
                return;

            if (HasError(scalarInput.Target))
                return;

            var asyncValidators = Template.ScalarAsyncValidators;
            for (int i = 0; i < asyncValidators.Count; i++)
            {
                var asyncValidator = asyncValidators[i];
                if (asyncValidator.SourceScalars.Intersect(scalarInput.Target).Count > 0)
                    asyncValidator.Run();
            }
        }

        private bool HasError(IScalars scalars)
        {
            if (ValidationErrors.Count == 0)
                return false;

            for (int i = 0; i < ValidationErrors.Count; i++)
            {
                var message = ValidationErrors[i];
                if (message.Source.SetEquals(scalars))
                    return true;
            }

            return false;
        }

        internal bool UpdateProgress<T>(ScalarInput<T> scalarInput, bool valueChanged, bool makeProgress)
            where T : UIElement, new()
        {
            Debug.Assert(valueChanged || makeProgress);

            if (Mode != ValidationMode.Progressive || _showAll)
                return valueChanged;

            var scalars = scalarInput.Target;
            if (scalars == null || scalars.Count == 0)
                return false;

            if (makeProgress)
            {
                if (valueChanged || _valueChanged.IsSupersetOf(scalars))
                {
                    _progress = _progress.Union(scalars);
                    return true;
                }
            }
            else
                _valueChanged = _valueChanged.Union(scalars);

            return false;
        }

        internal void ShowAll()
        {
            if (_showAll)
                return;

            _showAll = true;
            if (Mode == ValidationMode.Progressive)
            {
                _progress = Scalars.Empty;
                _valueChanged = Scalars.Empty;
            }
        }

        public ValidationMode Mode
        {
            get { return Template.ScalarValidationMode; }
        }

        public bool IsVisible(IScalars scalars)
        {
            if (scalars == null || scalars.Count == 0)
                return false;

            return _showAll || _progress == null ? true : _progress.IsSupersetOf(scalars);
        }

        public IScalarAsyncValidators AsyncValidators
        {
            get { return Template.ScalarAsyncValidators; }
        }
    }
}
