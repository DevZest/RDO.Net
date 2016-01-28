using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

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
                        autoSizeItem.MeasureRepeat(null);
                    else if (autoSizeItem.IsRepeat && CurrentRow != null)
                        autoSizeItem.MeasureRepeat(CurrentRow);
                }
            }

            public override Orientation GrowOrientation
            {
                get { return Orientation.Vertical; }
            }

            public override RowView RepeatStartRow
            {
                get { return CurrentRow; }
            }

            public override int RepeatX
            {
                get { return 1; }
            }

            public override int RepeatY
            {
                get { return 1; }
            }

            protected override double GetVariantAutoLength(GridTrack gridTrack, int repeatIndex)
            {
                return gridTrack.MeasuredLength;
            }

            protected override void SetVariantAutoLength(GridTrack gridTrack, int repeatIndex, double value)
            {
                gridTrack.MeasuredLength = value;
            }
        }
    }
}
