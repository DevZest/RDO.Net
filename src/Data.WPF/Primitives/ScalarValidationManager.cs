using DevZest.Data.Windows.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class ScalarValidationManager : ValidationManager
    {
        protected ScalarValidationManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy, bool emptyBlockViewList, Func<IEnumerable<ValidationMessage<Scalar>>> validateScalars)
            : base(template, dataSet, where, orderBy, emptyBlockViewList)
        {
            _validateScalars = validateScalars;
        }

        private readonly Func<IEnumerable<ValidationMessage<Scalar>>> _validateScalars;
        private bool _showAll;
        private IValidationSource<Scalar> _progress = ValidationSource<Scalar>.Empty;
        private List<ValidationMessage<Scalar>> _errors;
        private List<ValidationMessage<Scalar>> _warnings;

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
            _progress = ValidationSource<Scalar>.Empty;

            if (ScalarValidationMode == ValidationMode.Implicit)
                ValidateScalars(true);
            else
                ClearValidationMessages();
        }

        private ValidationMode ScalarValidationMode
        {
            get { return Template.ScalarValidationMode; }
        }

        public void ValidateScalars()
        {
            ValidateScalars(true);
            InvalidateElements();
        }

        private void ValidateScalars(bool showAll)
        {
            if (showAll)
                ShowAll();

            if (_validateScalars == null)
                return;

            ClearValidationMessages();

            foreach (var message in _validateScalars())
            {
                if (message.Severity == ValidationSeverity.Error)
                    _errors = _errors.AddItem(message);
                else
                {
                    Debug.Assert(message.Severity == ValidationSeverity.Warning);
                    _warnings = _warnings.AddItem(message);
                }
            }
        }

        private void ShowAll()
        {
            if (_showAll)
                return;

            _showAll = true;
            _progress = ValidationSource<Scalar>.Empty;
        }

        internal void MakeProgress<T>(ScalarInput<T> scalarInput)
            where T : UIElement, new()
        {
            if (!_showAll && ScalarValidationMode == ValidationMode.Progressive)
                _progress = _progress.Union(scalarInput.SourceScalars);

            if (ScalarValidationMode != ValidationMode.Explicit)
                ValidateScalars(false);
            scalarInput.RunAsyncValidator();
            InvalidateElements();
        }

        internal bool IsVisible(IValidationSource<Scalar> validationSource)
        {
            if (_showAll)
                return true;

            if (validationSource.Count == 0)
                return false;
            return _progress.IsSupersetOf(validationSource);
        }

        internal bool HasNoError(IValidationSource<Scalar> validationSource)
        {
            if (_errors == null)
                return true;

            foreach (var error in _errors)
            {
                if (error.Source.SetEquals(validationSource))
                    return false;
            }

            return true;
        }

        public IReadOnlyList<ValidationMessage<Scalar>> ScalarErrors
        {
            get
            {
                if (_errors != null)
                    return _errors;
                else
                    return Array<ValidationMessage<Scalar>>.Empty;
            }
        }

        public IReadOnlyList<ValidationMessage<Scalar>> ScalarWarnings
        {
            get
            {
                if (_warnings != null)
                    return _warnings;
                else
                    return Array<ValidationMessage<Scalar>>.Empty;
            }
        }

        public bool HasScalarPreValidatorError
        {
            get { return Template.ScalarBindings.HasPreValidatorError(); }
        }

        internal IReadOnlyList<ValidationMessage> GetErrors<T>(ScalarInput<T> scalarInput)
            where T : UIElement, new()
        {
            if (!IsVisible(scalarInput.SourceScalars))
                return Array<ValidationMessage>.Empty;

            return GetValidationMessages(_errors, scalarInput.SourceScalars);
        }

        internal IReadOnlyList<ValidationMessage> GetWarnings<T>(ScalarInput<T> scalarInput)
            where T : UIElement, new()
        {
            if (!IsVisible(scalarInput.SourceScalars))
                return Array<ValidationMessage>.Empty;

            return GetValidationMessages(_warnings, scalarInput.SourceScalars);
        }

        private static IReadOnlyList<ValidationMessage> GetValidationMessages(IReadOnlyList<ValidationMessage<Scalar>> messages, IValidationSource<Scalar> validationSource)
        {
            if (messages == null)
                return Array<ValidationMessage>.Empty;

            List<ValidationMessage> result = null;
            foreach (var message in messages)
            {
                if (message.Source.SetEquals(validationSource))
                    result = result.AddItem(new ValidationMessage(message.Id, message.Description, message.Severity));
            }

            return result.ToReadOnlyList();
        }
    }
}
