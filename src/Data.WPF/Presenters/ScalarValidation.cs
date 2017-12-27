using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public sealed class ScalarValidation
    {
        internal ScalarValidation(InputManager inputManager)
        {
            _inputManager = inputManager;
            if (Mode == ValidationMode.Progressive)
                _progress = Scalars.Empty;
        }

        private InputManager _inputManager;
        private bool _showAll;
        private IScalars _progress;

        private Template Template
        {
            get { return _inputManager.Template; }
        }

        internal void Reset()
        {
            _showAll = false;

            if (_progress != null)
                _progress = Scalars.Empty;
        }

        internal void MakeProgress<T>(ScalarInput<T> scalarInput)
            where T : UIElement, new()
        {
            var scalars = scalarInput.Target;

            if (!_showAll && _progress != null)
                _progress = _progress.Union(scalars);
        }

        internal void ShowAll()
        {
            if (_showAll)
                return;

            _showAll = true;
            if (_progress != null)
                _progress = Scalars.Empty;
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
