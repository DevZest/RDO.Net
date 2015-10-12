
using System.Windows;

namespace DevZest.Data.Wpf
{
    public struct GridRange
    {
        internal GridRange(GridColumn left, GridRow top)
            : this(left, top, left, top)
        {
        }

        internal GridRange(GridColumn left, GridRow top, GridColumn right, GridRow bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public readonly GridColumn Left;
        public readonly GridRow Top;
        public readonly GridColumn Right;
        public readonly GridRow Bottom;

        public DataSetControl DataSetControl
        {
            get { return Left == null ? null : Left.DataSetControl; }
        }

        public bool IsEmpty
        {
            get { return DataSetControl == null; }
        }

        public bool Contains(GridColumn gridColumn)
        {
            if (IsEmpty || DataSetControl != gridColumn.DataSetControl)
                return false;

            return Left.Ordinal <= gridColumn.Ordinal && Right.Ordinal >= gridColumn.Ordinal;
        }

        public bool Contains(GridRow gridRow)
        {
            if (IsEmpty || DataSetControl != gridRow.DataSetControl)
                return false;

            return Top.Ordinal <= gridRow.Ordinal && Bottom.Ordinal >= gridRow.Ordinal;
        }
    }
}
