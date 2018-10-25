using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace DevZest.Data.Presenters
{
    internal interface IScalarValidation
    {
        FlushingError GetFlushingError(UIElement element);
        void SetFlushingError(UIElement element, string flushingErrorMessage);
        void OnFlushed<T>(ScalarInput<T> scalarInput, bool makeProgress, bool valueChanged) where T : UIElement, new();
        ValidationInfo GetInfo(Input<ScalarBinding, IScalars> input, int flowIndex);
        bool HasError(Input<ScalarBinding, IScalars> input, int flowIndex, bool? blockingPrecedence);
        bool IsLockedByFlushingError(UIElement element);
    }

    public sealed class ScalarValidation : IScalarValidation
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
                    return Array.Empty<FlushingError>();
                return _flushingErrors;
            }
        }

        internal FlushingError GetFlushingError(UIElement element)
        {
            return _flushingErrors.GetFlushingError(element);
        }

        FlushingError IScalarValidation.GetFlushingError(UIElement element)
        {
            return GetFlushingError(element);
        }

        void IScalarValidation.SetFlushingError(UIElement element, string flushingErrorMessage)
        {
            var flushingError = string.IsNullOrEmpty(flushingErrorMessage) ? null : new FlushingError(flushingErrorMessage, element);
            InternalFlushingErrors.SetFlushError(element, flushingError);
        }

        private IScalarValidationErrors _errors = ScalarValidationErrors.Empty;
        private IScalarValidationErrors _asyncErrors = ScalarValidationErrors.Empty;

        public IReadOnlyList<ScalarValidationError> Errors
        {
            get { return _errors; }
        }

        internal ValidationInfo GetInfo(DataView dataView)
        {
            Debug.Assert(dataView != null);

            if (FlowRepeatCount == 1)
            {
                for (int i = 0; i < Inputs.Count; i++)
                {
                    if (HasError(Inputs[i], 0, true) || IsValidatingStatus(Inputs[i], true))
                        return ValidationInfo.Empty;
                }
            }

            var errors = GetErrors(ValidationErrors.Empty, null, false);
            errors = GetErrors(errors, null, true);
            if (errors.Count > 0)
                return ValidationInfo.Error(errors.Seal());

            foreach (var asyncValidator in AsyncValidators)
            {
                if (asyncValidator.Status == AsyncValidatorStatus.Running)
                    return ValidationInfo.Validating;
            }

            return ValidationInfo.Empty;
        }

        private int FlowRepeatCount
        {
            get { return _inputManager.FlowRepeatCount; }
        }

        internal ValidationInfo GetInfo(Input<ScalarBinding, IScalars> input, int flowIndex)
        {
            Debug.Assert(input != null);

            var flushingError = GetFlushingError(input.Binding[flowIndex]);
            if (flushingError != null)
                return ValidationInfo.Error(flushingError);

            if (FlowRepeatCount > 1)
                return ValidationInfo.Empty;

            if (AnyBlockingErrorInput(input, flowIndex, true) || AnyBlockingValidatingInput(input, true))
                return ValidationInfo.Empty;

            var errors = GetErrors(ValidationErrors.Empty, input.Target, false);
            errors = GetErrors(errors, input.Target, true);
            if (errors.Count > 0)
                return ValidationInfo.Error(errors.Seal());

            if (IsValidatingStatus(input, null))
                return ValidationInfo.Validating;

            if (!IsVisible(input.Target) || AnyBlockingErrorInput(input, flowIndex, false) || AnyBlockingValidatingInput(input, false))
                return ValidationInfo.Empty;
            else
                return ValidationInfo.Validated;
        }

        private bool AnyBlockingErrorInput(Input<ScalarBinding, IScalars> input, int flowIndex, bool isPreceding)
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                if (input.Index == i)
                    continue;
                var canBlock = isPreceding ? Inputs[i].IsPrecedingOf(input) : input.IsPrecedingOf(Inputs[i]);
                if (canBlock && HasError(Inputs[i], flowIndex, null))
                    return true;
            }
            return false;
        }

        internal bool HasError(Input<ScalarBinding, IScalars> input, int flowIndex, bool? blockingPrecedence)
        {
            var flushingError = GetFlushingError(input.Binding[flowIndex]);
            if (flushingError != null)
                return true;

            if (FlowRepeatCount > 1)
                return false;

            if (blockingPrecedence.HasValue)
            {
                if (AnyBlockingErrorInput(input, flowIndex, blockingPrecedence.Value))
                    return false;
            }

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

            if (isAsync)
            {
                foreach (var asyncValidator in AsyncValidators)
                {
                    if (asyncValidator.GetFault(scalars) != null)
                        return true;
                }
            }

            return false;
        }

        private IValidationErrors GetErrors(IValidationErrors result, IScalars scalars, bool isAsync)
        {
            var errors = isAsync ? this._asyncErrors : this._errors;
            var ensureVisible = !isAsync;

            for (int i = 0; i < errors.Count; i++)
            {
                var error = errors[i];
                if (ensureVisible && !IsVisible(error.Source))
                    continue;
                if (scalars == null || scalars.IsSupersetOf(error.Source))
                    result = result.Add(error);
            }

            if (isAsync)
            {
                foreach (var asyncValidator in AsyncValidators)
                {
                    var fault = asyncValidator.GetFault(scalars);
                    if (fault != null)
                        result = result.Add(fault);
                }
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

        void IScalarValidation.OnFlushed<T>(ScalarInput<T> scalarInput, bool makeProgress, bool valueChanged)
        {
            if (!makeProgress && !valueChanged)
                return;

            if (valueChanged)
            {
                UpdateAsyncErrors(scalarInput.Target);
                if (Mode != ValidationMode.Explicit)
                    Validate(false);
            }

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
                if (_progress.IsSupersetOf(scalars))
                    return valueChanged;
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

        private void UpdateAsyncErrors(IScalars changedScalars)
        {
            _asyncErrors = Remove(_asyncErrors, x => x.Source.Overlaps(changedScalars));
        }

        internal void UpdateAsyncErrors(ScalarAsyncValidator scalarAsyncValidator)
        {
            var sourceScalars = scalarAsyncValidator.SourceScalars;
            _asyncErrors = Remove(_asyncErrors, x => x.Source.SetEquals(sourceScalars));
            _asyncErrors = Merge(_asyncErrors, scalarAsyncValidator.Results);
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
                    if (result == errors)
                        result = Merge(ScalarValidationErrors.Empty, errors, i);
                }
                else
                {
                    if (result != errors)
                        result = result.Add(error);
                }
            }
            return result;
        }

        private Input<ScalarBinding, IScalars>[] _inputs;
        public IReadOnlyList<Input<ScalarBinding, IScalars>> Inputs
        {
            get
            {
                InitInputs();
                return _inputs;
            }
        }

        private bool IsAttachedScalarBindingsInvalidated
        {
            get { return DataPresenter == null ? false : DataPresenter.IsAttachedScalarBindingsInvalidated; }
        }

        private void InitInputs()
        {
            if (_inputs != null && !IsAttachedScalarBindingsInvalidated)
                return;

            _inputs = GetInputs().ToArray();
            for (int i = 0; i < _inputs.Length; i++)
                _inputs[i].Index = i;
            DataPresenter?.ResetIsAttachedScalarBindingsInvalidated();
        }

        private IEnumerable<Input<ScalarBinding, IScalars>> GetInputs()
        {
            foreach (var result in GetInputs(Template.ScalarBindings))
                yield return result;

            if (DataPresenter != null)
            {
                foreach (var result in GetInputs(DataPresenter.AttachedScalarBindings))
                    yield return result;
            }
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
                        var errors = _inputManager.GetValidationInfo(input, flowIndex).Errors;
                        for (int i = 0; i < errors.Count; i++)
                            result = result.Add(errors[i]);
                    }
                }

                {
                    var errors = _inputManager.GetValidationInfo(DataPresenter.View).Errors;
                    for (int i = 0; i < errors.Count; i++)
                        result = result.Add(errors[i]);
                }
                return result.Seal();
            }
        }

        private bool AnyBlockingValidatingInput(Input<ScalarBinding, IScalars> input, bool isPreceding)
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                if (input.Index == i)
                    continue;
                var canBlock = isPreceding ? Inputs[i].IsPrecedingOf(input) : input.IsPrecedingOf(Inputs[i]);
                if (canBlock && IsValidatingStatus(Inputs[i], null))
                    return true;
            }
            return false;
        }

        private bool IsValidatingStatus(Input<ScalarBinding, IScalars> input, bool? blockingPrecedence)
        {
            if (FlowRepeatCount > 1)
                return false;

            if (blockingPrecedence.HasValue)
            {
                if (AnyBlockingValidatingInput(input, blockingPrecedence.Value))
                    return false;
            }

            foreach (var asyncValidator in AsyncValidators)
            {
                if (asyncValidator.Status == AsyncValidatorStatus.Running && input.Target.IsSupersetOf(asyncValidator.SourceScalars))
                    return true;
            }
            return false;
        }

        ValidationInfo IScalarValidation.GetInfo(Input<ScalarBinding, IScalars> input, int flowIndex)
        {
            return GetInfo(input, flowIndex);
        }

        bool IScalarValidation.HasError(Input<ScalarBinding, IScalars> input, int flowIndex, bool? blockingPrecedence)
        {
            return HasError(input, flowIndex, blockingPrecedence);
        }

        bool IScalarValidation.IsLockedByFlushingError(UIElement element)
        {
            return GetFlushingError(element) != null;
        }

        public bool HasVisibleError
        {
            get
            {
                if (FlushingErrors.Count > 0)
                    return true;

                if (_asyncErrors.Count > 0)
                    return true;

                if (_errors.Any(x => IsVisible(x.Source)))
                    return true;

                return AsyncValidators.Any(x => x.Status == AsyncValidatorStatus.Faulted);
            }
        }

        public bool IsValidating
        {
            get { return AsyncValidators.Any(x => x.Status == AsyncValidatorStatus.Running); }
        }

        private sealed class Snapshot
        {
            private readonly ScalarValidation _source;
            private readonly bool _showAll;
            private readonly IScalars _progress;
            private readonly IScalars _valueChanged;

            public Snapshot(ScalarValidation source)
            {
                _source = source;
                _showAll = source._showAll;
                _progress = source._progress;
                _valueChanged = source._progress;
            }

            public void Restore()
            {
                _source._showAll = _showAll;
                _source._progress = _progress;
                _source._valueChanged = _valueChanged;
            }
        }

        private Snapshot _snapshot;
        internal void EnterEdit()
        {
            _snapshot = new Snapshot(this);
        }

        internal void CancelEdit()
        {
            _snapshot.Restore();
            ExitEdit();
        }

        internal void ExitEdit()
        {
            Debug.Assert(!DataPresenter.ScalarContainer.IsEditing);

            _snapshot = null;
            _flushingErrors = null;
            Validate(false);
            _asyncErrors = ScalarValidationErrors.Empty;
            foreach (var asyncValidator in AsyncValidators)
                asyncValidator.Reset();
        }

        public void SetAsyncErrors(IScalarValidationErrors value)
        {
            _asyncErrors = value.Seal();
            InvalidateView();
        }
    }
}
