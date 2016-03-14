using System;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    internal struct LogicalPoint
    {
        public LogicalPoint(GridColumn gridColumn, GridRow gridRow, RowPresenter row, double offsetX, double offsetY)
        {
            Debug.Assert(gridColumn != null && gridRow != null && gridColumn.Owner == gridRow.Owner);
            Debug.Assert(row == null || row.DataPresenter.Template == gridColumn.Owner);

            GridColumn = gridColumn;
            GridRow = gridRow;
            Row = row;
            _factorX = GetGridWidth(gridColumn, row) / offsetX;
            _factorY = GetGridHeight(gridRow, row) / offsetY;
        }

        public readonly GridColumn GridColumn;
        public readonly GridRow GridRow;

        public readonly RowPresenter Row;

        private readonly double _factorX;
        public double OffsetX
        {
            get { return GetGridWidth(GridColumn, Row) * _factorX; }
        }

        private readonly double _factorY;
        public double OffsetY
        {
            get { return GetGridHeight(GridRow, Row) * _factorY; }
        }

        private static double GetGridWidth(GridColumn gridColumn, RowPresenter row)
        {
            throw new NotImplementedException();
        }

        private static double GetGridHeight(GridRow gridRow, RowPresenter row)
        {
            throw new NotImplementedException();
        }
    }
}
