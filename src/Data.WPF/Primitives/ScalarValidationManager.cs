using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class ScalarValidationManager : ValidationManager
    {
        protected ScalarValidationManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy, bool emptyBlockViewList)
            : base(template, dataSet, where, orderBy, emptyBlockViewList)
        {
        }

        private bool _showAll;
        private IValidationSource<Scalar> _progress = ValidationSource<Scalar>.Empty;
        private List<ValidationMessage<Scalar>> _errors;
        private List<ValidationMessage<Scalar>> _warnings;

        protected override void Reload()
        {
            base.Reload();
            Reset();
        }

        private void Reset()
        {
            _showAll = false;
            _progress = ValidationSource<Scalar>.Empty;

            if (_errors != null)
                _errors.Clear();

            if (_warnings != null)
                _warnings.Clear();

            if (ScalarValidationMode == ValidationMode.Implicit)
                ValidateScalars();
        }

        private ValidationMode ScalarValidationMode
        {
            get { return Template.ScalarValidationMode; }
        }

        public void ValidateScalars()
        {
            ValidateScalars(true);
        }

        private void ValidateScalars(bool showAll)
        {
            if (showAll)
                ShowAll();

            throw new NotImplementedException();
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
            RunAsyncValidators();
            InvalidateElements();
        }

        private void RunAsyncValidators()
        {
            foreach (var scalarBinding in Template.ScalarBindings)
            {
                if (ShouldRunAsyncValidator(scalarBinding))
                    scalarBinding.RunAsyncValidator();
            }
        }

        private bool IsVisible(ScalarBinding scalarBinding)
        {
            if (_showAll)
                return true;

            var validationSource = scalarBinding.ValidationSource;
            if (validationSource.Count == 0)
                return false;
            return _progress.IsSupersetOf(validationSource);
        }

        private bool ShouldRunAsyncValidator(ScalarBinding scalarBinding)
        {
            if (!IsVisible(scalarBinding))
                return false;

            return scalarBinding.HasAsyncValidator
                && !scalarBinding.HasPreValidatorError;
                //&& IsEmpty(_errors, scalarBinding.ValidationSource);
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
            get
            {
                foreach (var scalarBinding in Template.ScalarBindings)
                {
                    if (scalarBinding.HasPreValidatorError)
                        return true;
                }
                return false;
            }
        }
    }
}
