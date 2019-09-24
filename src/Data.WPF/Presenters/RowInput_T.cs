using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents the handler of row level two way data binding flushing from view to presenter.
    /// </summary>
    /// <typeparam name="T">The type of view element.</typeparam>
    public class RowInput<T> : Input<T, RowBinding, IColumns>
        where T : UIElement, new()
    {
        internal RowInput(RowBinding<T> rowBinding, Trigger<T> flushTrigger, Trigger<T> progressiveFlushTrigger)
            : base(flushTrigger, progressiveFlushTrigger)
        {
            RowBinding = rowBinding;
        }

        /// <summary>
        /// Gets the row binding.
        /// </summary>
        public RowBinding<T> RowBinding { get; private set; }

        /// <inheritdoc/>
        public sealed override RowBinding Binding
        {
            get { return RowBinding; }
        }

        private IRowValidation _rowValidation;
        private IRowValidation RowValidation
        {
            get { return _rowValidation ?? InputManager.RowValidation; }
        }

        internal void InjectRowValidation(IRowValidation rowValidation)
        {
            Debug.Assert(rowValidation != null);
            _rowValidation = rowValidation;
        }

        /// <inheritdoc/>
        public override FlushingError GetFlushingError(UIElement element)
        {
            return RowValidation.GetFlushingError(element);
        }

        internal sealed override void SetFlushingError(UIElement element, string flushingErrorMessage)
        {
            RowValidation.SetFlushingError(element, flushingErrorMessage);
        }

        internal sealed override bool IsLockedByFlushingError(UIElement element)
        {
            return RowValidation.IsLockedByFlushingError(element);
        }

        private IColumns _target = Columns.Empty;
        /// <inheritdoc/>
        public override IColumns Target
        {
            get { return _target; }
        }

        private List<Func<RowPresenter, T, bool>> _flushFuncs = new List<Func<RowPresenter, T, bool>>();

        private RowPresenter CurrentRow
        {
            get { return InputManager.CurrentRow; }
        }

        /// <summary>
        /// Sets flushing validator.
        /// </summary>
        /// <param name="flushingValidator">The flushing validator.</param>
        /// <returns>This row input for fluent coding.</returns>
        public RowInput<T> WithFlushingValidator(Func<T, string> flushingValidator)
        {
            SetFlushingValidator(flushingValidator);
            return this;
        }

        /// <summary>
        /// Setup the flushing operation.
        /// </summary>
        /// <typeparam name="TData">Data type of column.</typeparam>
        /// <param name="column">The column.</param>
        /// <param name="getValue">The delegate to return data value.</param>
        /// <returns>This row input for fluent coding.</returns>
        public RowInput<T> WithFlush<TData>(Column<TData> column, Func<T, TData> getValue)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            VerifyNotSealed();
            _target = _target.Union(column);
            _flushFuncs.Add((p, v) =>
            {
                if (getValue == null)
                    return false;
                var value = getValue(v);
                if (column.AreEqual(p.GetValue(column), value))
                    return false;
                p.EditValue(column, value);
                return true;
            });
            return this;
        }

        /// <summary>
        /// Setup the flushing operation.
        /// </summary>
        /// <typeparam name="TData">Data type of column.</typeparam>
        /// <param name="column">The column.</param>
        /// <param name="getValue">The delegate to return data value.</param>
        /// <returns>This row input for fluent coding.</returns>
        public RowInput<T> WithFlush<TData>(Column<TData> column, Func<RowPresenter, T, TData> getValue)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            VerifyNotSealed();
            _target = _target.Union(column);
            _flushFuncs.Add((p, v) =>
            {
                if (getValue == null)
                    return false;
                var value = getValue(p, v);
                if (column.AreEqual(p.GetValue(column), value))
                    return false;
                p.EditValue(column, value);
                return true;
            });
            return this;
        }

        /// <summary>
        /// Setup the flushing operation.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="flush">The delegate to flush input.</param>
        /// <returns>This row input for fluent coding.</returns>
        public RowInput<T> WithFlush(Column column, Func<RowPresenter, T, bool> flush)
        {
            column.VerifyNotNull(nameof(column));
            flush.VerifyNotNull(nameof(flush));
            VerifyNotSealed();
            _target = _target.Union(column);
            _flushFuncs.Add(flush);
            return this;
        }

        /// <summary>
        /// Setup the flushing operation.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="valueBagGetter">The delegate to get column value bag.</param>
        /// <returns>This row input for fluent coding.</returns>
        public RowInput<T> WithFlush(Column column, Func<T, ColumnValueBag> valueBagGetter)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (valueBagGetter == null)
                throw new ArgumentNullException(nameof(valueBagGetter));

            VerifyNotSealed();
            _target = _target.Union(column);
            _flushFuncs.Add((rowPresenter, element) =>
            {
                if (valueBagGetter == null)
                    return false;
                var value = valueBagGetter(element)[column];
                if (object.Equals(rowPresenter[column], value))
                    return false;
                rowPresenter[column] = value;
                return true;
            });
            return this;
        }

        private bool IsValidationVisible
        {
            get { return InputManager.RowValidation.IsVisible(CurrentRow, Target); }
        }

        internal override void FlushCore(T element, bool isFlushing, bool isProgressiveFlushing)
        {
            Debug.Assert(isFlushing || isProgressiveFlushing);
            Debug.Assert(CurrentRow != null);
            var currentRow = CurrentRow;
            if (currentRow != element.GetRowPresenter())
            {
                if (isFlushing)
                    throw new InvalidOperationException(DiagnosticMessages.RowInput_FlushCurrentRowOnly);
                else
                    return;
            }
            var valueChanged = DoFlush(currentRow, element);
            var makeProgress = isProgressiveFlushing || IsValidationVisible;
            RowValidation.OnFlushed(this, isProgressiveFlushing || IsValidationVisible, valueChanged);
        }

        private bool DoFlush(RowPresenter rowPresenter, T element)
        {
            bool result = false;
            for (int i = 0; i < _flushFuncs.Count; i++)
            {
                var flush = _flushFuncs[i];
                var flushed = flush(rowPresenter, element);
                if (flushed)
                    result = true;
            }
            return result;
        }

        /// <summary>
        /// Gets the flushing error.
        /// </summary>
        /// <param name="rowPresenter">The row presenter.</param>
        /// <returns>The flushing error.</returns>
        public FlushingError GetFlushingError(RowPresenter rowPresenter)
        {
            RowBinding rowBinding = RowBinding;
            var element = rowBinding[rowPresenter];
            return element != null ? GetFlushingError(element) : null;
        }

        /// <summary>
        /// Get the validation info.
        /// </summary>
        /// <param name="rowPresenter">The row presenter.</param>
        /// <returns>The validation info.</returns>
        public ValidationInfo GetValidationInfo(RowPresenter rowPresenter)
        {
            rowPresenter.VerifyNotNull(nameof(rowPresenter));
            return RowValidation.GetInfo(rowPresenter, this);
        }

        /// <summary>
        /// Determines whether validation error exists for specified row presenter.
        /// </summary>
        /// <param name="rowPresenter">The row presenter.</param>
        /// <returns><see langword="true"/> if validation error exists, otherwise <see langword="false"/>.</returns>
        public bool HasValidationError(RowPresenter rowPresenter)
        {
            rowPresenter.VerifyNotNull(nameof(rowPresenter));
            return RowValidation.HasError(rowPresenter, this, true);
        }

        internal void Refresh(T element, RowPresenter rowPresenter)
        {
            if (!IsFlushing && !IsLockedByFlushingError(element))
                RowBinding.Refresh(element, rowPresenter);
            element.RefreshValidation(GetValidationInfo(rowPresenter));
        }

        /// <summary>
        /// Ends the input implementation.
        /// </summary>
        /// <returns>The row binding for fluent coding.</returns>
        public RowBinding<T> EndInput()
        {
            _target = _target.Seal();
            return RowBinding;
        }

        internal override bool IsPrecedingOf(Input<RowBinding, IColumns> input)
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
