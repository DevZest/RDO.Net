using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    partial class LayoutXYManager
    {
        private struct LogicalOffset
        {
            public LogicalOffset(double value)
            {
                _value = value;
            }

            public LogicalOffset(int gridOffset, double fractionOffset)
            {
                Debug.Assert(gridOffset >= 0);
                Debug.Assert(fractionOffset >= 0 && fractionOffset < 1);
                _value = gridOffset + fractionOffset;
            }

            private readonly double _value;
            public double Value
            {
                get { return _value; }
            }

            public int GridOffset
            {
                get { return (int)_value; }
            }

            public double FractionOffset
            {
                get { return _value - GridOffset; }
            }
        }
    }
}
