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

            protected override void InitMeasure()
            {
                CurrentRow = _view.CurrentRow;
                foreach (var autoSizeItem in _autoSizeItems)
                {
                    if (autoSizeItem.IsScalar)
                        autoSizeItem.Measure(null);
                    else if (autoSizeItem.IsList && CurrentRow != null)
                        autoSizeItem.Measure(CurrentRow);
                }
            }
        }
    }
}
