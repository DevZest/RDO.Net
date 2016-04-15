using System;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    partial class LayoutManagerXY
    {
        private sealed class X : LayoutManagerXY
        {
            public X(Template template, DataSet dataSet)
                : base(template, dataSet)
            {
                var rowRange = template.RowRange;
                _variantAutoWidthColumns = CalcVariantAutoLengthTracks(template.GridColumns, rowRange.Left, rowRange.Right);
            }

            private readonly IConcatList<GridColumn> _variantAutoWidthColumns;

            protected override Point Offset(Point point, int blockDimension)
            {
                point.Offset(Template.RowRange.MeasuredHeight * blockDimension, 0);
                return point;
            }

            protected override Size GetMeasuredSize(DataItem dataItem)
            {
                var gridRange = dataItem.GridRange;
                var width = gridRange.MeasuredWidth;
                var height = gridRange.MeasuredHeight;
                if (dataItem.BlockDimensions > 1)
                    height += Template.RowRange.MeasuredHeight * (BlockDimensions - 1);
                return new Size(width, height);
            }
        }
    }
}
