using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents the handler of scalar level two way data binding flushing from view to presenter.
    /// </summary>
    /// <typeparam name="T">The type of view element.</typeparam>
    public class ScalarInput<T> : Input<T, ScalarBinding, IScalars>
        where T : UIElement, new()
    {
        internal ScalarInput(ScalarBinding<T> scalarBinding, Trigger<T> flushTrigger, Trigger<T> progressiveFlushTrigger)
            : base(flushTrigger, progressiveFlushTrigger)
        {
            Debug.Assert(scalarBinding != null);
            ScalarBinding = scalarBinding;
        }

        /// <summary>
        /// Gets the scalar binding.
        /// </summary>
        public ScalarBinding<T> ScalarBinding { get; private set; }

        /// <inheritdoc/>
        public sealed override ScalarBinding Binding
        {
            get { return ScalarBinding; }
        }

        private IScalarValidation _scalarValidation;
        private IScalarValidation ScalarValidation
        {
            get { return _scalarValidation ?? InputManager.ScalarValidation; }
        }

        internal void InjectScalarValidation(IScalarValidation scalarValidation)
        {
            Debug.Assert(scalarValidation != null);
            _scalarValidation = scalarValidation;
        }

        /// <inheritdoc/>
        public override FlushingError GetFlushingError(UIElement element)
        {
            return ScalarValidation.GetFlushingError(element);
        }

        internal sealed override bool IsLockedByFlushingError(UIElement element)
        {
            return ScalarValidation.IsLockedByFlushingError(element);
        }

        internal sealed override void SetFlushingError(UIElement element, string flushingErrorMessage)
        {
            ScalarValidation.SetFlushingError(element, flushingErrorMessage);
        }

        /// <summary>
        /// Sets flushing validator.
        /// </summary>
        /// <param name="flushingValidator">The flushing validator.</param>
        /// <returns>This scalar input for fluent coding.</returns>
        public ScalarInput<T> WithFlushingValidator(Func<T, string> flushingValidator)
        {
            SetFlushingValidator(flushingValidator);
            return this;
        }

        private IScalars _target = Scalars.Empty;
        /// <inheritdoc/>
        public override IScalars Target
        {
            get { return _target; }
        }   

        private List<Func<T, bool>> _flushFuncs = new List<Func<T, bool>>();

        /// <summary>
        /// Setup the flushing operation.
        /// </summary>
        /// <param name="scalar">The scalar data.</param>
        /// <param name="flush">The delegate to flush input.</param>
        /// <returns>This scalar input for fluent coding.</returns>
        public ScalarInput<T> WithFlush(Scalar scalar, Func<T, bool> flush)
        {
            scalar.VerifyNotNull(nameof(scalar));
            flush.VerifyNotNull(nameof(flush));

            VerifyNotSealed();
            _target = _target.Union(scalar);
            _flushFuncs.Add(flush);
            return this;
        }

        /// <summary>
        /// Setup the flushing operation.
        /// </summary>
        /// <typeparam name="TData">Data type of scalar data.</typeparam>
        /// <param name="scalar">The scalar data.</param>
        /// <param name="getValue">The delegate to return data value.</param>
        /// <returns>This scalar input for fluent coding.</returns>
        public ScalarInput<T> WithFlush<TData>(Scalar<TData> scalar, Func<T, TData> getValue)
        {
            if (scalar == null)
                throw new ArgumentNullException(nameof(scalar));

            VerifyNotSealed();
            _target = _target.Union(scalar);
            _flushFuncs.Add(element =>
            {
                if (getValue == null)
                    return false;
                var value = getValue(element);
                return scalar.EditValue(value);
            });
            return this;
        }

        private bool IsValidationVisible
        {
            get { return InputManager.ScalarValidation.IsVisible(Target); }
        }

        internal override void FlushCore(T element, bool isFlushing, bool isProgressiveFlushing)
        {
            var valueChanged = DoFlush(element);
            var makeProgress = isProgressiveFlushing || IsValidationVisible;
            ScalarValidation.OnFlushed(this, makeProgress, valueChanged);
        }

        private bool DoFlush(T element)
        {
            bool result = false;
            for (int i = 0; i < _flushFuncs.Count; i++)
            {
                var flush = _flushFuncs[i];
                var flushed = flush(element);
                if (flushed)
                    result = true;
            }
            return result;
        }

        internal void Refresh(T v, ScalarPresenter p)
        {
            if (!IsFlushing && !IsLockedByFlushingError(v))
                ScalarBinding.Refresh(v, p);
            v.RefreshValidation(GetValidationInfo(p.FlowIndex));
        }

        /// <summary>
        /// Gets the validation info.
        /// </summary>
        /// <param name="flowIndex">The flow index.</param>
        /// <returns>The validation info.</returns>
        public ValidationInfo GetValidationInfo(int flowIndex = 0)
        {
            if (flowIndex < 0 || flowIndex >= InputManager.FlowRepeatCount)
                throw new ArgumentOutOfRangeException(nameof(flowIndex));
            return ScalarValidation.GetInfo(this, flowIndex);
        }

        /// <summary>
        /// Determines whether validation error exists.
        /// </summary>
        /// <param name="flowIndex">The flow index.</param>
        /// <returns><see langword="true"/> if validation error exists, otherwise <see langword="false"/>.</returns>
        public bool HasValidationError(int flowIndex = 0)
        {
            if (flowIndex < 0 || flowIndex >= InputManager.FlowRepeatCount)
                throw new ArgumentOutOfRangeException(nameof(flowIndex));
            return ScalarValidation.HasError(this, flowIndex, true);
        }

        /// <summary>
        /// Ends the input implementation.
        /// </summary>
        /// <returns>The scalar binding for fluent coding.</returns>
        public ScalarBinding<T> EndInput()
        {
            _target = _target.Seal();
            return ScalarBinding;
        }

        internal override bool IsPrecedingOf(Input<ScalarBinding, IScalars> input)
        {
            Debug.Assert(input != null && input != this);
            if (!input.Target.Overlaps(Target))
                return false;
            else if (input.Target.SetEquals(Target))
                return IsPlaceholder || !input.IsPlaceholder;
            else if (input.Target.IsSupersetOf(Target))
                return true;
            else if (Target.IsSupersetOf(input.Target))
                return false;
            else
                return Index < input.Index;
        }
    }
}
