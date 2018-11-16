using System.Collections.Generic;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Windows.Controls;
using DevZest.Data.Views;
using DevZest.Data.Views.Primitives;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public abstract class DataPresenter : DataPresenterBase, IDataPresenter
    {
        protected DataPresenter()
            : base()
        {
        }

        public new DataView View { get; private set; }

        internal sealed override IDataView GetView()
        {
            return View;
        }

        internal sealed override void SetView(IDataView value)
        {
            View = (DataView)value;
        }

        public Orientation? LayoutOrientation
        {
            get { return Template?.Orientation; }
        }

        public DataSet DataSet
        {
            get { return LayoutManager?.DataSet; }
        }

        public bool IsRecursive
        {
            get { return LayoutManager == null ? false : LayoutManager.IsRecursive; }
        }

        public Predicate<DataRow> Where
        {
            get { return LayoutManager?.Where; }
            set { Apply(value, OrderBy); }
        }
        
        public IComparer<DataRow> OrderBy
        {
            get { return LayoutManager?.OrderBy; }
            set { Apply(Where, value); }
        }

        public virtual void Apply(Predicate<DataRow> where, IComparer<DataRow> orderBy)
        {
            RequireLayoutManager();
            if (where != Where || OrderBy != orderBy)
                PerformApply(where, orderBy);
        }

        internal abstract void PerformApply(Predicate<DataRow> where, IComparer<DataRow> orderBy);

        public IReadOnlyList<RowPresenter> Rows
        {
            get { return LayoutManager?.Rows; }
        }

        public RowPresenter CurrentRow
        {
            get { return LayoutManager?.CurrentRow; }
            set
            {
                if (value == CurrentRow)
                    return;

                VerifyRowPresenter(value, nameof(value));
                var layoutManager = RequireLayoutManager();
                View.UpdateLayout();
                layoutManager.CurrentRow = value;
            }
        }

        public ContainerView CurrentContainerView
        {
            get { return RequireLayoutManager().CurrentContainerView; }
        }

        public int FlowRepeatCount
        {
            get { return LayoutManager == null ? 1 : LayoutManager.FlowRepeatCount; }
        }

        private void VerifyRowPresenter(RowPresenter value, string paramName, int index = -1)
        {
            if (value == null)
                throw new ArgumentNullException(GetParamName(paramName, index));

            if (value.DataPresenter != this || value.Index < 0)
                throw new ArgumentException(DiagnosticMessages.DataPresenter_InvalidRowPresenter, GetParamName(paramName, index));
        }

        private static string GetParamName(string paramName, int index)
        {
            return index == -1 ? paramName : string.Format("{0}[{1}]", paramName, index);
        }

        public void Select(RowPresenter rowPresenter, SelectionMode selectionMode, bool ensureVisible = true, Action beforeSelecting = null)
        {
            VerifyRowPresenter(rowPresenter, nameof(rowPresenter));

            var oldCurrentRow = CurrentRow;
            beforeSelecting?.Invoke();
            SuspendInvalidateView();
            CurrentRow = rowPresenter;
            RequireLayoutManager().Select(rowPresenter, selectionMode, oldCurrentRow);
            ResumeInvalidateView();
            if (ensureVisible && Scrollable != null)
                Scrollable.EnsureCurrentRowVisible();
        }

        public void Select(params RowPresenter[] rows)
        {
            if (rows != null)
            {
                for (int i = 0; i < rows.Length; i++)
                    VerifyRowPresenter(rows[i], nameof(rows), i);
            }
            SuspendInvalidateView();
            RequireLayoutManager().Select(rows);
            InvalidateView();
            ResumeInvalidateView();
        }

        public IReadOnlyCollection<RowPresenter> SelectedRows
        {
            get { return LayoutManager?.SelectedRows; }
        }

        public IEnumerable<DataRow> SelectedDataRows
        {
            get
            {
                foreach (var row in SelectedRows)
                {
                    if (row.IsVirtual)
                        continue;
                    yield return row.DataRow;
                }
            }
        }

        public RowPresenter VirtualRow
        {
            get { return LayoutManager?.VirtualRow; }
        }

        public bool IsEditing
        {
            get { return LayoutManager == null ? false : LayoutManager.IsEditing; }
        }

        public RowPresenter EditingRow
        {
            get { return CurrentRow != null && IsEditing ? CurrentRow : null; }
        }

        public bool IsInserting
        {
            get { return IsEditing && LayoutManager.CurrentRow == LayoutManager.VirtualRow; }
        }

        public RowPresenter InsertingRow
        {
            get { return IsInserting ? CurrentRow : null; }
        }

        public bool CanInsert
        {
            get { return !IsEditing && RequireLayoutManager().DataSet.EditingRow == null; }
        }

        public void BeginInsertBefore(RowPresenter row = null)
        {
            VerifyInsert(row);
            SuspendInvalidateView();
            RequireLayoutManager().BeginInsertBefore(null, row);
            ResumeInvalidateView();
        }

        public void BeginInsertAfter(RowPresenter row = null)
        {
            VerifyInsert(row);
            SuspendInvalidateView();
            RequireLayoutManager().BeginInsertAfter(null, row);
            ResumeInvalidateView();
        }

        private void VerifyInsert(RowPresenter row)
        {
            if (!CanInsert)
                throw new InvalidOperationException(DiagnosticMessages.DataPresenter_VerifyCanInsert);
            if (row != null)
            {
                if (row.DataPresenter != this || row.Parent != null)
                    throw new ArgumentException(DiagnosticMessages.DataPresenter_InvalidRowPresenter, nameof(row));
            }
        }

        public RowValidation RowValidation
        {
            get { return LayoutManager?.RowValidation; }
        }

        public IScrollable Scrollable
        {
            get { return LayoutManager as ScrollableManager; }
        }

        internal protected virtual void ToggleExpandState(RowPresenter rowPresenter)
        {
            rowPresenter.InternalToggleExpandState();
        }

        internal protected virtual bool HasChildren(RowPresenter rowPresenter)
        {
            return rowPresenter.InternalHasChildren;
        }

        internal abstract void Reload();

        internal abstract bool CanReload { get; }

        internal abstract void CancelLoading();

        internal abstract bool CanCancelLoading { get; }

        public override bool SubmitInput(bool focusToErrorInput = true)
        {
            RequireLayoutManager();

            if (IsEditing)
                CurrentRow.EndEdit();

            if (IsEditing)
                return false;

            RowValidation.Validate();
            ScalarValidation.Validate();
            if (!CanSubmitInput)
            {
                if (focusToErrorInput)
                    LayoutManager.FocusToInputError();
                return false;
            }

            return true;
        }

        protected internal virtual void OnCurrentRowChanged(RowPresenter oldValue)
        {
        }

        protected internal virtual bool QueryEndEdit()
        {
            return RequireLayoutManager().QueryEndEdit();
        }

        protected internal virtual bool QueryCancelEdit()
        {
            return true;
        }

        protected internal virtual bool ConfirmEndEdit()
        {
            return true;
        }

        #region IService

        DataPresenter IService.DataPresenter
        {
            get { return this; }
        }

        void IService.Initialize(DataPresenter dataPresenter)
        {
            throw new NotSupportedException();
        }
        #endregion

        public void Select(RowPresenter row, MouseButton mouseButton, Action beforeSelecting)
        {
            VerifyRowPresenter(row, nameof(row));

            if (EditingRow != null)
                return;

            var selectionMode = PredictSelectionMode(mouseButton, row);
            if (selectionMode.HasValue)
            {
                SuspendInvalidateView();
                Select(row, selectionMode.GetValueOrDefault(), true, beforeSelecting);
                ResumeInvalidateView();
            }
        }

        protected virtual SelectionMode? PredictSelectionMode(MouseButton mouseButton, RowPresenter row)
        {
            switch (Template.SelectionMode)
            {
                case SelectionMode.Single:
                    return SelectionMode.Single;
                case SelectionMode.Multiple:
                    return SelectionMode.Multiple;
                case SelectionMode.Extended:
                    if (mouseButton != MouseButton.Left)
                    {
                        if (mouseButton == MouseButton.Right && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == ModifierKeys.None)
                        {
                            if (row.IsSelected)
                                return null;
                            return SelectionMode.Single;
                        }
                        return null;
                    }

                    if (IsControlDown && IsShiftDown)
                        return null;

                    return IsShiftDown ? SelectionMode.Extended : (IsControlDown ? SelectionMode.Multiple : SelectionMode.Single);
            }
            return null;
        }

        private static bool IsControlDown
        {
            get { return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control; }
        }

        private static bool IsShiftDown
        {
            get { return (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift; }
        }

        protected internal virtual bool ConfirmDelete()
        {
            return true;
        }

        public virtual ColumnSerializer GetSerializer(Column column)
        {
            return ColumnSerializer.Create(column);
        }

        public RowPresenter this[DataRow dataRow]
        {
            get { return LayoutManager?[dataRow]; }
        }

        protected internal virtual void OnEdit(Column column)
        {
        }

        public bool CanMatchRow
        {
            get { return LayoutManager == null ? false : LayoutManager.CanMatchRow; }
        }

        public RowPresenter Match(IReadOnlyList<Column> columns, DataRow dataRow = null)
        {
            columns.VerifyNotNull(nameof(columns));
            if (!CanMatchRow)
                return null;

            var valueHashCode = RowMatch.GetHashCode(columns, dataRow);
            if (!valueHashCode.HasValue)
                return null;
            return LayoutManager[new RowMatch(columns, dataRow, valueHashCode.Value)];
        }

        public RowPresenter Match(RowPresenter rowPresenter, bool matchVirtual = true)
        {
            return LayoutManager?.Match(rowPresenter, matchVirtual);
        }

        internal protected virtual void OnIsSelectedChanged(RowPresenter row)
        {
        }

        public virtual T GetService<T>(bool autoCreate = true)
            where T : class, IService
        {
            return (this is T) ? (T)((object)this) : ServiceManager.GetService<T>(this, autoCreate);
        }

        public bool ExistsService<T>()
            where T : class, IService
        {
            return GetService<T>(autoCreate: false) != null;
        }
    }
}
