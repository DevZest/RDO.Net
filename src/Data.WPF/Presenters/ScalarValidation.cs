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
        }

        private InputManager _inputManager;
        private bool _showAll;
        private IScalars _progress;
        private IScalars _valueChanged;

        private Template Template
        {
            get { return _inputManager.Template; }
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
            get { return _inputManager.ScalarValidationMode; }
        }

        public bool IsVisible(IScalars scalars)
        {
            if (scalars == null || scalars.Count == 0)
                return false;

            return _showAll || _progress == null ? true : _progress.IsSupersetOf(scalars);
        }

        public IReadOnlyList<FlushErrorMessage> FlushErrors
        {
            get { return _inputManager.ScalarFlushErrors; }
        }

        public IReadOnlyList<ScalarValidationMessage> Errors
        {
            get { return _inputManager.ScalarValidationErrors; }
        }

        public IReadOnlyList<ScalarValidationMessage> Warnings
        {
            get { return _inputManager.ScalarValidationWarnings; }
        }

        public IScalarValidationMessages GetErrors(IScalars scalars)
        {
            Check.NotNull(scalars, nameof(scalars));
            return _inputManager.GetValidationErrors(scalars);
        }

        public IScalarValidationMessages GetWarnings(IScalars scalars)
        {
            Check.NotNull(scalars, nameof(scalars));
            return _inputManager.GetValidationWarnings(scalars);
        }

        public IScalarAsyncValidators AsyncValidators
        {
            get { return Template.ScalarAsyncValidators; }
        }

        public void Assign(IScalarValidationMessages validationResults)
        {
            if (validationResults == null)
                throw new ArgumentNullException(nameof(validationResults));
            _inputManager.Assign(validationResults);
        }

        public IReadOnlyList<ScalarValidationMessage> AssignedResults
        {
            get { return _inputManager.AssignedScalarValidationResults; }
        }

        public void Flush()
        {
            _inputManager.FlushScalars();
        }

        public void Validate()
        {
            _inputManager.ValidateScalars();
        }
    }
}
