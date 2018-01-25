﻿using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public sealed class ScalarValidation
    {
        internal ScalarValidation(InputManager inputManager)
        {
            _inputManager = inputManager;
            InitInputs();
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

        private FlushingErrorCollection _flushingErrors;
        private FlushingErrorCollection InternalFlushingErrors
        {
            get
            {
                if (_flushingErrors == null)
                    _flushingErrors = new FlushingErrorCollection(_inputManager);
                return _flushingErrors;
            }
        }

        public IReadOnlyList<FlushingError> FlushingErrors
        {
            get
            {
                if (_flushingErrors == null)
                    return Array<FlushingError>.Empty;
                return _flushingErrors;
            }
        }

        internal FlushingError GetFlushingError(UIElement element)
        {
            return _flushingErrors.GetFlushingError(element);
        }

        internal void SetFlushingError(UIElement element, FlushingError flushingError)
        {
            InternalFlushingErrors.SetFlushError(element, flushingError);
        }

        private IScalarValidationErrors _errors = ScalarValidationErrors.Empty;
        private IScalarValidationErrors _asyncErrors = ScalarValidationErrors.Empty;

        public IReadOnlyList<ScalarValidationError> Errors
        {
            get { return _errors; }
        }

        internal IValidationErrors GetErrors(DataView dataView)
        {
            Debug.Assert(dataView != null);

            if (FlowRepeatCount == 1)
            {
                for (int i = 0; i < Inputs.Count; i++)
                {
                    if (HasError(Inputs[i], 0, true))
                        return ValidationErrors.Empty;
                }
            }

            var errors = GetErrors(null, false);
            var asyncErrors = GetErrors(null, true);
            return ValidationErrors.Empty.Merge(errors).Merge(asyncErrors).Seal();
        }

        private int FlowRepeatCount
        {
            get { return _inputManager.FlowRepeatCount; }
        }

        internal IValidationErrors GetErrors(Input<ScalarBinding, IScalars> input, int flowIndex)
        {
            Debug.Assert(input != null);

            var flushingError = GetFlushingError(input.Binding[flowIndex]);
            if (flushingError != null)
                return flushingError;

            if (FlowRepeatCount > 1)
                return ValidationErrors.Empty;

            if (AnyPrecedingInputHasError(input, flowIndex))
                return ValidationErrors.Empty;

            var errors = GetErrors(input.Target, false);
            var asyncErrors = GetErrors(input.Target, true);
            return ValidationErrors.Empty.Merge(errors).Merge(asyncErrors).Seal();
        }

        private bool AnyPrecedingInputHasError(Input<ScalarBinding, IScalars> input, int flowIndex)
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                if (input.Index == i)
                    continue;
                if (Inputs[i].IsPrecedingOf(input) && HasError(Inputs[i], flowIndex, false))
                    return true;
            }
            return false;
        }

        internal bool HasError(Input<ScalarBinding, IScalars> input, int flowIndex, bool blockByPrecedingInput)
        {
            var flushingError = GetFlushingError(input.Binding[flowIndex]);
            if (flushingError != null)
                return true;

            if (FlowRepeatCount > 1)
                return false;

            if (blockByPrecedingInput && AnyPrecedingInputHasError(input, flowIndex))
                return false;

            if (HasError(input.Target, false))
                return true;
            if (HasError(input.Target, true))
                return true;
            return false;
        }

        private bool HasError(IScalars scalars, bool isAsync)
        {
            var errors = isAsync ? this._asyncErrors : this._errors;
            var ensureVisible = !isAsync;
            for (int i = 0; i < errors.Count; i++)
            {
                var error = errors[i];
                if (ensureVisible && !IsVisible(error.Source))
                    continue;
                if (scalars == null || scalars.IsSupersetOf(error.Source))
                    return true;
            }

            return false;
        }

        private IValidationErrors GetErrors(IScalars scalars, bool isAsync)
        {
            var errors = isAsync ? this._asyncErrors : this._errors;
            var ensureVisible = !isAsync;

            var result = ValidationErrors.Empty;
            for (int i = 0; i < errors.Count; i++)
            {
                var error = errors[i];
                if (ensureVisible && !IsVisible(error.Source))
                    continue;
                if (scalars == null || scalars.IsSupersetOf(error.Source))
                    result = result.Add(error);
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

            ClearErrors();
            var errors = _inputManager.PerformValidateScalars();
            for (int i = 0; i < errors.Count; i++)
                _errors = _errors.Add(errors[i]);
            _errors = _errors.Seal();
        }

        private void ClearErrors()
        {
            _errors = ScalarValidationErrors.Empty;
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
            if (Errors.Count == 0)
                return false;

            for (int i = 0; i < Errors.Count; i++)
            {
                var message = Errors[i];
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

        internal void UpdateAsyncErrors(ScalarAsyncValidator scalarAsyncValidator)
        {
            var sourceScalars = scalarAsyncValidator.SourceScalars;
            _errors = Remove(_errors, x => x.Source.SetEquals(sourceScalars));
            _errors = Merge(_errors, scalarAsyncValidator.Results);
        }

        private static IScalarValidationErrors Merge(IScalarValidationErrors result, IScalarValidationErrors errors)
        {
            return Merge(result, errors, errors.Count);
        }

        private static IScalarValidationErrors Merge(IScalarValidationErrors result, IScalarValidationErrors errors, int count)
        {
            for (int i = 0; i < count; i++)
                result = result.Add(errors[i]);
            return result;
        }

        private static IScalarValidationErrors Remove(IScalarValidationErrors errors, Predicate<ScalarValidationError> predicate)
        {
            var result = errors;

            for (int i = 0; i < errors.Count; i++)
            {
                var error = errors[i];
                if (predicate(error))
                {
                    if (result != errors)
                        result = result.Add(error);
                }
                else
                {
                    if (result == errors)
                        result = Merge(ScalarValidationErrors.Empty, errors, i);
                }
            }
            return result;
        }

        private Input<ScalarBinding, IScalars>[] _inputs;
        public IReadOnlyList<Input<ScalarBinding, IScalars>> Inputs
        {
            get { return _inputs; }
        }

        private void InitInputs()
        {
            _inputs = GetInputs(Template.ScalarBindings).ToArray();
            for (int i = 0; i < Inputs.Count; i++)
                Inputs[i].Index = i;
        }

        private static IEnumerable<Input<ScalarBinding, IScalars>> GetInputs(IReadOnlyList<ScalarBinding> scalarBindings)
        {
            if (scalarBindings == null)
                yield break;
            for (int i = 0; i < scalarBindings.Count; i++)
            {
                var scalarBinding = scalarBindings[i];
                var scalarInput = scalarBinding.ScalarInput;
                if (scalarInput != null)
                    yield return scalarInput;

                foreach (var childInput in GetInputs(scalarBinding.ChildBindings))
                    yield return childInput;
            }
        }

        public IValidationErrors VisibleErrors
        {
            get
            {
                var result = ValidationErrors.Empty;
                foreach (var input in Inputs)
                {
                    for (int flowIndex = 0; flowIndex < FlowRepeatCount; flowIndex++)
                    {
                        var errors = _inputManager.GetValidationErrors(input, flowIndex);
                        for (int i = 0; i < errors.Count; i++)
                            result = result.Add(errors[i]);
                    }
                }

                {
                    var errors = _inputManager.GetValidationErrors(DataPresenter.View);
                    for (int i = 0; i < errors.Count; i++)
                        result = result.Add(errors[i]);
                }
                return result.Seal();
            }
        }
    }
}
