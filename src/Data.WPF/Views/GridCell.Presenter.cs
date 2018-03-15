using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System;
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
            }

            private Template Template
            {
                get { return DataPresenter.Template; }
            }

            private RowBinding[] _gridCellBindings;

            private GridCellMode _mode = GridCellMode.Edit;
            public GridCellMode Mode
            {
                get { return _mode; }
                set
                {
                    if (_mode == value)
                        return;

                    if (_mode == GridCellMode.Edit && DataPresenter.IsEditing)
                        DataPresenter.EditingRow.CancelEdit();
                    _mode = value;
                    _extendedBindingSelection = _extendedRowSelection = 0;
                    DataPresenter.InvalidateView();
                }
            }

            private int _currentBinding = -1;
            private int _extendedBindingSelection;
            private RowPresenter _currentRow;
            private int _extendedRowSelection;

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
                    return false;

                if (_currentBinding < 0)
                    return false;

                var bindingIndex = VerifyGridCell(gridCell, nameof(gridCell));
                return IsBindingSelected(bindingIndex) && IsRowSelected(gridCell.GetRowPresenter().Index);
            }

            private bool IsBindingSelected(int bindingIndex)
            {
                var extended = _currentBinding + _extendedBindingSelection;
                var startIndex = Math.Min(_currentBinding, extended);
                var endIndex = Math.Max(_currentBinding, extended);
                return bindingIndex >= startIndex && bindingIndex <= endIndex;
            }

            private bool IsRowSelected(int rowIndex)
            {
                if (_currentRow == null)
                    return false;
                var currentRowIndex = _currentRow.Index;
                var extended = currentRowIndex + _extendedRowSelection;
                var startIndex = Math.Min(currentRowIndex, extended);
                var endIndex = Math.Max(currentRowIndex, extended);
                return rowIndex >= startIndex && rowIndex <= endIndex;
            }

            public bool IsCurrent(GridCell gridCell)
            {
                var index = VerifyGridCell(gridCell, nameof(gridCell));
                if (!gridCell.IsKeyboardFocusWithin && DataPresenter.View.IsKeyboardFocusWithin)    // Another element in DataView has keyboard focus
                    return false;
                return _currentBinding == index && gridCell.GetRowPresenter() == _currentRow;
            }

            public void Select(GridCell gridCell, bool isExtended)
            {
                if (Mode != GridCellMode.Select)
                    throw new InvalidOperationException(DiagnosticMessages.GridCell_Presenter_SelectionNotAllowedInEditMode);

                var bindingIndex = VerifyGridCell(gridCell, nameof(gridCell));

                SuspendInvalidateView();

                DeselectAllRows();

                if (_currentBinding == -1)
                    _currentBinding = bindingIndex;
                if (_currentRow == null)
                    _currentRow = gridCell.GetRowPresenter();

                if (isExtended)
                    _extendedBindingSelection += _currentBinding - bindingIndex;
                else
                    _extendedBindingSelection = 0;

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

                Debug.Assert(_currentBinding == bindingIndex);
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
                if (Template.SelectionMode == null)
                    DeselectAllRows();

                var index = IndexOf(gridCell);
                Debug.Assert(index >= 0 && index < _gridCellBindings.Length);
                _currentBinding = index;
                _currentRow = gridCell.GetRowPresenter();
                if (!_isExtendedSelectionLocked)
                    _extendedBindingSelection = _extendedRowSelection = 0;

                InvalidateView();
            }
        }
    }
}
