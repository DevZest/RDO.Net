using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    partial class LayoutManager
    {
        private sealed class LayoutZ : LayoutManager
        {
            public LayoutZ(DataView view)
                : base(view)
            {
            }

            private RowView _currentRow;
            private RowView CurrentRow
            {
                get { return _currentRow; }
                set
                {
                    if (_currentRow == value)
                        return;

                    if (_currentRow != null)
                        _realizedRows.RemoveAll();

                    _currentRow = value;

                    if (_currentRow != null)
                        _realizedRows.Add(_currentRow);
                }
            }

            public override void OnCurrentRowChanged()
            {
                Invalidate();
            }

            protected override int RepeatXCount
            {
                get { return 1; }
            }

            protected override int RepeatYCount
            {
                get { return 1; }
            }

            protected override double GetGridWidth(GridColumn gridColumn, int repeatXIndex)
            {
                Debug.Assert(repeatXIndex == 0);
                return gridColumn.MeasuredWidth;
            }

            protected override double GetGridHeight(GridRow gridRow, int repeatYIndex)
            {
                Debug.Assert(repeatYIndex == 0);
                return gridRow.MeasuredHeight;
            }

            protected override void MeasureOverride(Size availableSize)
            {
                CurrentRow = _view.CurrentRow;
            }
        }
    }
}
