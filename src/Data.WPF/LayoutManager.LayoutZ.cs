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
            public LayoutZ(DataPresenter presenter)
                : base(presenter)
            {
            }

            private RowPresenter _currentRow;
            private RowPresenter CurrentRow
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
                CurrentRow = _presenter.CurrentRow;
                foreach (var autoSizeItem in _autoSizeItems)
                {
                    if (autoSizeItem.IsScalar)
                        autoSizeItem.MeasureRepeat(null);
                    else if (autoSizeItem.IsRepeat && CurrentRow != null)
                        autoSizeItem.MeasureRepeat(CurrentRow);
                }
            }

            protected override double GetMeasuredLength(GridTrack gridTrack, int repeatIndex)
            {
                Debug.Assert(repeatIndex == 0);
                return gridTrack.MeasuredLength;
            }

            protected override void SetMeasureLength(GridTrack gridTrack, int repeatIndex, double value)
            {
                Debug.Assert(repeatIndex == 0);
                gridTrack.MeasuredLength = value;
            }
        }
    }
}
