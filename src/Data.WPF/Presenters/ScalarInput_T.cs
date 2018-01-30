using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using DevZest.Data;
using System.Threading.Tasks;

namespace DevZest.Data.Presenters
{
    public sealed class ScalarInput<T> : Input<T, ScalarBinding, IScalars>
        where T : UIElement, new()
    {
        internal ScalarInput(ScalarBinding<T> scalarBinding, Trigger<T> flushTrigger, Trigger<T> progressiveFlushTrigger)
            : base(flushTrigger, progressiveFlushTrigger)
        {
            Debug.Assert(scalarBinding != null);
            ScalarBinding = scalarBinding;
        }

        public ScalarBinding<T> ScalarBinding { get; private set; }

        public sealed override ScalarBinding Binding
        {
            get { return ScalarBinding; }
        }

        private ScalarValidation ScalarValidation
        {
            get { return InputManager.ScalarValidation; }
        }

        public sealed override FlushingError GetFlushingError(UIElement element)
        {
            return ScalarValidation.GetFlushingError(element);
        }

        internal sealed override void SetFlushingError(UIElement element, FlushingError flushingError)
        {
            ScalarValidation.SetFlushingError(element, flushingError);
        }

        public ScalarInput<T> WithFlushingValidator(Func<T, string> flushingValidator)
        {
            SetFlushingValidator(flushingValidator);
            return this;
        }

        private IScalars _target = Scalars.Empty;
        public override IScalars Target
        {
            get { return _target; }
        }   

        private List<Func<T, bool>> _flushFuncs = new List<Func<T, bool>>();

        public ScalarInput<T> WithFlush(Scalar scalar, Func<T, bool> flushFunc)
        {
            Check.NotNull(scalar, nameof(scalar));
            Check.NotNull(flushFunc, nameof(flushFunc));

            VerifyNotSealed();
            _target = _target.Union(scalar);
            _flushFuncs.Add(flushFunc);
            return this;
        }

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
                return scalar.ChangeValue(value);
            });
            return this;
        }

        internal override bool IsValidationVisible
        {
            get { return InputManager.ScalarValidation.IsVisible(Target); }
        }

        internal override void FlushCore(T element, bool makeProgress)
        {
            var valueChanged = DoFlush(element);
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

        private Action<T, ScalarPresenter> _onRefresh;
        internal void Refresh(T v, ScalarPresenter p)
        {
            if (!IsFlushing && GetFlushingError(v) == null)
                ScalarBinding.Refresh(v, p);
            var flushingError = GetFlushingError(v);
            if (_onRefresh != null)
            {
                p.FlushingError = flushingError;
                _onRefresh(v, p);
                p.FlushingError = null;
            }
            v.RefreshValidation(GetValidationPresenter(p.FlowIndex));
        }

        public ValidationPresenter GetValidationPresenter(int flowIndex = 0)
        {
            if (flowIndex < 0 || flowIndex >= InputManager.FlowRepeatCount)
                throw new ArgumentOutOfRangeException(nameof(flowIndex));
            return InputManager.GetValidationPresenter(this, flowIndex);
        }

        public bool HasValidationError(int flowIndex = 0)
        {
            if (flowIndex < 0 || flowIndex >= InputManager.FlowRepeatCount)
                throw new ArgumentOutOfRangeException(nameof(flowIndex));
            return InputManager.HasValidationError(this, flowIndex);
        }

        public ScalarInput<T> WithRefreshAction(Action<T, ScalarPresenter> onRefresh)
        {
            VerifyNotSealed();
            _onRefresh = onRefresh;
            return this;
        }

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
