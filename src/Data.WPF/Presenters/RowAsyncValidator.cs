using DevZest.Data.Presenters.Primitives;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents row level async validator.
    /// </summary>
    public abstract class RowAsyncValidator : AsyncValidator<IDataValidationErrors>, IRowAsyncValidators
    {
        internal static RowAsyncValidator Create(string displayName, IColumns sourceColumns, Func<DataRow, Task<string>> validator)
        {
            return new SingleErrorValidator(displayName, sourceColumns, validator);
        }

        internal static RowAsyncValidator Create(string displayName, IColumns sourceColumns, Func<DataRow, Task<IEnumerable<string>>> validator)
        {
            return new MultipleErrorValidator(displayName, sourceColumns, validator);
        }

        private sealed class SingleErrorValidator : RowAsyncValidator
        {
            public SingleErrorValidator(string displayName, IColumns columns, Func<DataRow, Task<string>> validator)
                : base(displayName, columns)
            {
                _validator = validator;
            }

            private readonly Func<DataRow, Task<string>> _validator;

            internal override async Task<IDataValidationErrors> ValidateAsync()
            {
                var message = await _validator(CurrentRow.DataRow);
                return string.IsNullOrEmpty(message) ? DataValidationErrors.Empty : new DataValidationError(message, SourceColumns);
            }
        }

        private sealed class MultipleErrorValidator : RowAsyncValidator
        {
            public MultipleErrorValidator(string displayName, IColumns columns, Func<DataRow, Task<IEnumerable<string>>> validator)
                : base(displayName, columns)
            {
                _validator = validator;
            }

            private readonly Func<DataRow, Task<IEnumerable<string>>> _validator;

            internal override async Task<IDataValidationErrors> ValidateAsync()
            {
                var messages = await _validator(CurrentRow.DataRow);
                var result = DataValidationErrors.Empty;
                if (messages == null)
                    return result;
                foreach (var message in messages)
                {
                    if (!string.IsNullOrEmpty(message))
                        result = result.Add(new DataValidationError(message, SourceColumns));
                }
                return result.Seal();
            }
        }

        private RowAsyncValidator(string displayName, IColumns sourceColumns)
            : base(displayName)
        {
            Debug.Assert(sourceColumns != null && sourceColumns.Count > 0);
            _sourceColumns = sourceColumns.Seal();
        }

        private readonly IColumns _sourceColumns;
        /// <summary>
        /// Gets the source columns of the validator.
        /// </summary>
        public IColumns SourceColumns
        {
            get { return _sourceColumns; }
        }

        internal RowPresenter CurrentRow
        {
            get { return InputManager?.CurrentRow; }
        }

        internal sealed override IDataValidationErrors EmptyResult
        {
            get { return DataValidationErrors.Empty; }
        }

        /// <inheritdoc/>
        public override void Run()
        {
            if (CurrentRow == null)
                return;
            base.Run();
        }

        internal sealed override void OnStatusChanged()
        {
            Debug.Assert(CurrentRow != null);
            InputManager.RowValidation.UpdateAsyncErrors(this);
            InvalidateView();
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
        RowAsyncValidator IRowAsyncValidators.this[IColumns sourceColumns]
        {
            get { return sourceColumns == SourceColumns ? this : null; }
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

        internal AsyncValidationFault GetFault(IColumns container)
        {
            return Fault != null && (container == null || container.IsSupersetOf(SourceColumns)) ? Fault : null;
        }
    }
}
