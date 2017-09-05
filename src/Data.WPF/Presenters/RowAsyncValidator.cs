using DevZest.Data.Presenters.Primitives;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using DevZest.Data;
using System.Diagnostics.CodeAnalysis;

namespace DevZest.Data.Presenters
{
    public abstract class RowAsyncValidator : AsyncValidator<IRowValidationResults>, IRowAsyncValidators
    {
        internal static RowAsyncValidator Create<T>(RowInput<T> rowInput, Func<Task<IColumnValidationMessages>> action, Action postAction)
            where T : UIElement, new()
        {
            return new RowInputAsyncValidator<T>(rowInput, action, postAction);
        }

        internal static RowAsyncValidator Create(Template template, IColumns sourceColumns, Func<Task<IColumnValidationMessages>> action, Action postAction)
        {
            return new CurrentRowAsyncValidator(template, sourceColumns, action, postAction);
        }

        internal static RowAsyncValidator Create(Template template, IColumns sourceColumns, Func<Task<IDataRowValidationResults>> action, Action postAction)
        {
            return new AllRowAsyncValidator(template, sourceColumns, action, postAction);
        }

        private static async Task<IRowValidationResults> Validate(Func<Task<IColumnValidationMessages>> action, RowPresenter currentRow)
        {
            var messages = await action();
            return messages == null || messages.Count == 0 || currentRow == null
                ? RowValidationResults.Empty : RowValidationResults.Empty.Add(currentRow, messages);
        }

        private sealed class RowInputAsyncValidator<T> : RowAsyncValidator
            where T : UIElement, new()
        {
            public RowInputAsyncValidator(RowInput<T> rowInput, Func<Task<IColumnValidationMessages>> action, Action postAction)
                : base(postAction)
            {
                Debug.Assert(rowInput != null);
                Debug.Assert(action != null);
                _rowInput = rowInput;
                _action = action;
            }

            private readonly RowInput<T> _rowInput;
            private readonly Func<Task<IColumnValidationMessages>> _action;

            public override IColumns SourceColumns
            {
                get { return _rowInput.Target; }
            }

            internal override InputManager InputManager
            {
                get { return _rowInput.InputManager; }
            }

            private RowPresenter CurrentRow
            {
                get { return InputManager.CurrentRow; }
            }

            internal override IRowInput RowInput
            {
                get { return _rowInput; }
            }

            public override RowValidationScope ValidationScope
            {
                get { return RowValidationScope.Current; }
            }

            protected override async Task<IRowValidationResults> ValidateAsync()
            {
                return await Validate(_action, CurrentRow);
            }
        }

        private abstract class ColumnsAsyncValidator : RowAsyncValidator
        {
            protected ColumnsAsyncValidator(Template template, IColumns sourceColumns, Action postAction)
                : base(postAction)
            {
                Debug.Assert(template != null);
                _template = template;
                _sourceColumns = sourceColumns;
            }

            private readonly Template _template;
            private readonly IColumns _sourceColumns;

            internal override InputManager InputManager
            {
                get { return _template.InputManager; }
            }

            public override IColumns SourceColumns
            {
                get { return _sourceColumns; }
            }
        }

        private sealed class CurrentRowAsyncValidator : ColumnsAsyncValidator
        {
            public CurrentRowAsyncValidator(Template template, IColumns sourceColumns, Func<Task<IColumnValidationMessages>> action, Action postAction)
                : base(template, sourceColumns, postAction)
            {
                Debug.Assert(action != null);
                _action = action;
            }

            private readonly Func<Task<IColumnValidationMessages>> _action;

            private RowPresenter CurrentRow
            {
                get { return InputManager.CurrentRow; }
            }

            internal override IRowInput RowInput
            {
                get { return null; }
            }

            public override RowValidationScope ValidationScope
            {
                get { return RowValidationScope.Current; }
            }

            protected override async Task<IRowValidationResults> ValidateAsync()
            {
                return await Validate(_action, CurrentRow);
            }
        }

        private sealed class AllRowAsyncValidator : ColumnsAsyncValidator
        {
            public AllRowAsyncValidator(Template template, IColumns sourceColumns, Func<Task<IDataRowValidationResults>> action, Action postAction)
                : base(template, sourceColumns, postAction)
            {
                Debug.Assert(action != null);
                _action = action;
            }

            private readonly Func<Task<IDataRowValidationResults>> _action;

            internal override IRowInput RowInput
            {
                get { return null; }
            }

            public override RowValidationScope ValidationScope
            {
                get { return RowValidationScope.All; }
            }

            protected override async Task<IRowValidationResults> ValidateAsync()
            {
                var result = await _action();
                return result == null ? RowValidationResults.Empty : InputManager.ToRowValidationResults(result);
            }
        }

#if DEBUG
        public RowAsyncValidator()
            : base(null)
        {
        }
#endif

        private RowAsyncValidator(Action postAction)
            : base(postAction)
        {
        }

        public abstract IColumns SourceColumns { get; }

        private IRowValidationResults _errors = RowValidationResults.Empty;
        public IRowValidationResults Errors
        {
            get { return _errors; }
            private set
            {
                Debug.Assert(value != null && value.IsSealed);
                if (_errors == value)
                    return;
                _errors = value;
                OnPropertyChanged(nameof(Errors));
                RefreshHasError();
            }
        }

        private bool _hasError;
        public sealed override bool HasError
        {
            get { return _hasError; }
        }
        private void RefreshHasError()
        {
            var value = Errors.Count > 0;
            if (value == _hasError)
                return;
            _hasError = value;
            OnPropertyChanged(nameof(HasError));
        }

        private IRowValidationResults _warnings = RowValidationResults.Empty;
        public IRowValidationResults Warnings
        {
            get { return _warnings; }
            private set
            {
                Debug.Assert(value != null && value.IsSealed);
                if (_warnings == value)
                    return;
                _warnings = value;
                OnPropertyChanged(nameof(Warnings));
            }
        }

        private bool _hasWarning;
        public sealed override bool HasWarning
        {
            get { return _hasWarning; }
        }
        private void RefreshHasWarning()
        {
            var value = Warnings.Count > 0;
            if (value == _hasWarning)
                return;
            _hasWarning = value;
            OnPropertyChanged(nameof(HasWarning));
        }

        internal abstract IRowInput RowInput { get; }

        public abstract RowValidationScope ValidationScope { get; }

        protected sealed override void ClearValidationMessages()
        {
            Errors = Warnings = RowValidationResults.Empty;
        }

        protected sealed override void RefreshValidationMessages(IRowValidationResults result)
        {
            Errors = result.Where(ValidationSeverity.Error);
            Warnings = result.Where(ValidationSeverity.Warning);
        }

        protected sealed override IRowValidationResults EmptyValidationResult
        {
            get { return RowValidationResults.Empty; }
        }

        internal void OnRowDisposed(RowPresenter rowPresenter)
        {
            if (Errors.ContainsKey(rowPresenter))
                Errors = Errors.Remove(rowPresenter);
            if (Warnings.ContainsKey(rowPresenter))
                Warnings = Warnings.Remove(rowPresenter);
        }

        internal void OnCurrentRowChanged()
        {
            if (ValidationScope == RowValidationScope.Current)
                Reset();
        }

        #region IRowAsyncValidators

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        bool IRowAsyncValidators.IsSealed
        {
            get { return true; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        int IReadOnlyCollection<RowAsyncValidator>.Count
        {
            get { return 1; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        RowAsyncValidator IReadOnlyList<RowAsyncValidator>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IRowAsyncValidators IRowAsyncValidators.Seal()
        {
            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IRowAsyncValidators IRowAsyncValidators.Add(RowAsyncValidator value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            return RowAsyncValidators.New(this, value);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator<RowAsyncValidator> IEnumerable<RowAsyncValidator>.GetEnumerator()
        {
            yield return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

#endregion
    }
}
