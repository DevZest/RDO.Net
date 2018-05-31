﻿using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public class RowInput<T> : Input<T, RowBinding, IColumns>
        where T : UIElement, new()
    {
        internal RowInput(RowBinding<T> rowBinding, Trigger<T> flushTrigger, Trigger<T> progressiveFlushTrigger)
            : base(flushTrigger, progressiveFlushTrigger)
        {
            RowBinding = rowBinding;
        }

        public RowBinding<T> RowBinding { get; private set; }

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
        public override IColumns Target
        {
            get { return _target; }
        }

        private List<Func<RowPresenter, T, bool>> _flushFuncs = new List<Func<RowPresenter, T, bool>>();

        private RowPresenter CurrentRow
        {
            get { return InputManager.CurrentRow; }
        }

        public RowInput<T> WithFlushingValidator(Func<T, string> flushingValidaitor)
        {
            SetFlushingValidator(flushingValidaitor);
            return this;
        }

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

        public RowInput<T> WithFlush(Column column, Func<RowPresenter, T, bool> flushFunc)
        {
            column.VerifyNotNull(nameof(column));
            flushFunc.VerifyNotNull(nameof(flushFunc));
            VerifyNotSealed();
            _target = _target.Union(column);
            _flushFuncs.Add(flushFunc);
            return this;
        }

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

        public FlushingError GetFlushingError(RowPresenter rowPresenter)
        {
            RowBinding rowBinding = RowBinding;
            var element = rowBinding[rowPresenter];
            return element != null ? GetFlushingError(element) : null;
        }

        public ValidationInfo GetValidationInfo(RowPresenter rowPresenter)
        {
            rowPresenter.VerifyNotNull(nameof(rowPresenter));
            return RowValidation.GetInfo(rowPresenter, this);
        }

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
