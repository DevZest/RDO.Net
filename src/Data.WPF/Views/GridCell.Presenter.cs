using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using DevZest.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.Views
{
    partial class GridCell
    {
        public sealed class Presenter : IService
        {
            public DataPresenter DataPresenter { get; private set; }

            void IService.Initialize(DataPresenter dataPresenter)
            {
                if (!dataPresenter.IsMounted)
                    throw new InvalidOperationException(DiagnosticMessages.DataPresenter_NotMounted);
                DataPresenter = dataPresenter;
                _gridCellBindings = Template.RowBindings.Where(x => typeof(GridCell).IsAssignableFrom(x.ViewType)).ToArray();
                CurrentBindingIndex = GridCellBindings.Count > 0 ? 0 : -1;
            }

            private Template Template
            {
                get { return DataPresenter.Template; }
            }

            public IReadOnlyCollection<RowPresenter> SelectedFullRows
            {
                get { return DataPresenter.SelectedRows; }
            }

            private RowBinding[] _gridCellBindings;
            public IReadOnlyList<RowBinding> GridCellBindings
            {
                get { return _gridCellBindings; }
            }

            private GridCellMode _mode = GridCellMode.Select;
            [DefaultValue(GridCellMode.Select)]
            public GridCellMode Mode
            {
                get { return _mode; }
                set
                {
                    if (_mode == value)
                        return;

                    SuspendInvalidateView();
                    if (_mode == GridCellMode.Edit && DataPresenter.IsEditing)
                        DataPresenter.EditingRow.CancelEdit();
                    _mode = value;
                    DeselectAllRows();
                    ClearExtendedSelection();
                    DataPresenter.InvalidateView();
                    ResumeInvalidateView();
                }
            }

            public bool CanToggleMode(GridCell gridCell)
            {
                VerifyGridCell(gridCell, nameof(gridCell));
                return Mode == GridCellMode.Edit ? true : gridCell.IsEditable;
            }

            public void ToggleMode(GridCell gridCell)
            {
                VerifyGridCell(gridCell, nameof(gridCell));

                bool isKeyboardFocusWithin = gridCell.IsKeyboardFocusWithin;
                var mode = Mode;
                Mode = mode == GridCellMode.Edit ? GridCellMode.Select : GridCellMode.Edit;
                gridCell.Refresh();
                if (isKeyboardFocusWithin && !gridCell.ContainsKeyboardFocus())
                    gridCell.Focus();
            }

            private bool _selectFullRowInEditMode = true;
            [DefaultValue(true)]
            public bool SelectFullRowInEditMode
            {
                get { return _selectFullRowInEditMode; }
                set
                {
                    if (_selectFullRowInEditMode == value)
                        return;

                    _selectFullRowInEditMode = value;
                    DataPresenter.InvalidateView();
                }
            }

            public int CurrentBindingIndex { get; private set; }
            private int _extendedBindingSelection;

            public int StartSelectedBindingIndex
            {
                get { return Math.Min(CurrentBindingIndex, CurrentBindingIndex + _extendedBindingSelection); }
            }

            public int EndSelectedBindingIndex
            {
                get { return Math.Max(CurrentBindingIndex, CurrentBindingIndex + _extendedBindingSelection); }
            }

            public RowPresenter CurrentRow
            {
                get { return DataPresenter.CurrentRow; }
                private set
                {
                    DataPresenter.CurrentRow = value;
                    DataPresenter.Scrollable.EnsureCurrentRowVisible();
                }
            }

            private int _extendedRowSelection;

            public int CurrentRowIndex
            {
                get { return CurrentRow == null ? -1 : CurrentRow.Index; }
            }

            public int StartSelectedRowIndex
            {
                get { return Math.Min(CurrentRowIndex, CurrentRowIndex + _extendedRowSelection); }
            }

            public int EndSelectedRowIndex
            {
                get { return Math.Max(CurrentRowIndex, CurrentRowIndex + _extendedRowSelection); }
            }

            public void ClearExtendedSelection()
            {
                _extendedBindingSelection = _extendedRowSelection = 0;
                InvalidateView();
            }

            private int IndexOf(GridCell gridCell)
            {
                var binding = gridCell.GetBinding();
                return IndexOf(binding);
            }

            private int IndexOf(Binding binding)
            {
                if (binding == null)
                    return -1;
                if (binding.Template != Template)
                    return -1;
                return Array.IndexOf(_gridCellBindings, binding);
            }

            private int VerifyGridCell(GridCell gridCell, string paramName)
            {
                Check.NotNull(gridCell, paramName);
                var index = IndexOf(gridCell);
                if (index < 0)
                    throw new ArgumentException(DiagnosticMessages.GridCell_Presenter_VerifyGridCell, paramName);
                return index;
            }

            public bool IsSelected(GridCell gridCell)
            {
                if (DataPresenter.SelectedRows.Count > 0)
                    return gridCell.GetRowPresenter().IsSelected;

                if (Mode == GridCellMode.Edit)
                    return SelectFullRowInEditMode && gridCell.GetRowPresenter().IsCurrent;

                if (CurrentBindingIndex < 0)
                    return false;

                var bindingIndex = VerifyGridCell(gridCell, nameof(gridCell));
                return IsBindingSelected(bindingIndex) && IsRowSelected(gridCell.GetRowPresenter().Index);
            }

            private bool IsBindingSelected(int bindingIndex)
            {
                return bindingIndex >= StartSelectedBindingIndex && bindingIndex <= EndSelectedBindingIndex;
            }

            private bool IsRowSelected(int rowIndex)
            {
                return rowIndex >= StartSelectedRowIndex && rowIndex <= EndSelectedRowIndex;
            }

            public bool IsCurrent(GridCell gridCell)
            {
                var index = VerifyGridCell(gridCell, nameof(gridCell));
                if (!gridCell.IsKeyboardFocusWithin && DataPresenter.View.IsKeyboardFocusWithin)    // Another element in DataView has keyboard focus
                    return false;
                return CurrentBindingIndex == index && gridCell.GetRowPresenter() == CurrentRow;
            }

            public void Select(RowPresenter rowPresenter, bool isExtended)
            {
                Select(rowPresenter, CurrentBindingIndex, isExtended);
            }

            public void Select(RowPresenter rowPresenter, int bindingIndex, bool isExtended)
            {
                Check.NotNull(rowPresenter, nameof(rowPresenter));
                if (bindingIndex < 0 || bindingIndex >= GridCellBindings.Count)
                    throw new ArgumentOutOfRangeException(nameof(bindingIndex));

                if (isExtended)
                    _extendedRowSelection += CurrentRow.Index - rowPresenter.Index; ;

                CurrentRow = rowPresenter;
                var gridCell = (GridCell)GridCellBindings[bindingIndex][CurrentRow];
                Select(gridCell, isExtended);
            }

            public void Select(GridCell gridCell, bool isExtended)
            {
                if (Mode != GridCellMode.Select)
                    throw new InvalidOperationException(DiagnosticMessages.GridCell_Presenter_SelectionNotAllowedInEditMode);

                var bindingIndex = VerifyGridCell(gridCell, nameof(gridCell));

                SuspendInvalidateView();

                DeselectAllRows();

                if (CurrentBindingIndex == -1)
                    CurrentBindingIndex = bindingIndex;

                if (isExtended)
                {
                    _extendedBindingSelection += CurrentBindingIndex - bindingIndex;
                    _extendedRowSelection += CurrentRow.Index - gridCell.GetRowPresenter().Index;
                }
                else
                    ClearExtendedSelection();

                if (!gridCell.IsKeyboardFocusWithin)
                {
                    _isExtendedSelectionLocked = true;
                    try
                    {
                        gridCell.Focus();
                    }
                    finally
                    {
                        _isExtendedSelectionLocked = false;
                    }
                }

                Debug.Assert(CurrentBindingIndex == bindingIndex);
                InvalidateView();
                ResumeInvalidateView();
            }

            private void DeselectAllRows()
            {
                var rows = DataPresenter.SelectedRows;
                if (rows.Count == 0)
                    return;

                foreach (var row in rows.ToArray())
                    row.IsSelected = false;
            }

            private void InvalidateView()
            {
                DataPresenter.InvalidateView();
            }

            private void SuspendInvalidateView()
            {
                DataPresenter.SuspendInvalidateView();
            }

            private void ResumeInvalidateView()
            {
                DataPresenter.ResumeInvalidateView();
            }

            private bool _isExtendedSelectionLocked;
            internal void OnFocused(GridCell gridCell)
            {
                SuspendInvalidateView();
                DeselectAllRows();

                var index = IndexOf(gridCell);
                Debug.Assert(index >= 0 && index < _gridCellBindings.Length);
                CurrentBindingIndex = index;
                if (!_isExtendedSelectionLocked)
                    ClearExtendedSelection();

                InvalidateView();
                ResumeInvalidateView();
            }

            public GridCellMode? PredictActivate(GridCell gridCell)
            {
                VerifyGridCell(gridCell, nameof(gridCell));

                if (Mode == GridCellMode.Edit)
                    return null;

                if (SelectedFullRows.Count > 0 || !gridCell.IsKeyboardFocusWithin || gridCell.Mode != GridCellMode.Select)
                    return GridCellMode.Select;

                if (StartSelectedBindingIndex == EndSelectedBindingIndex && StartSelectedRowIndex == EndSelectedRowIndex)
                {
                    if (gridCell.IsEditable)
                        return GridCellMode.Edit;
                    else
                        return null;
                }

                return GridCellMode.Select;
            }

            public void Activate(GridCell gridCell)
            {
                var predict = PredictActivate(gridCell);

                if (predict == GridCellMode.Select)
                    Select(gridCell, false);
                else if (predict == GridCellMode.Edit)
                    ToggleMode(gridCell);
            }

            public void SelectAll()
            {
                SuspendInvalidateView();

                foreach (var row in DataPresenter.Rows)
                    row.IsSelected = true;
                ClearExtendedSelection();

                ResumeInvalidateView();
            }
        }
    }
}
