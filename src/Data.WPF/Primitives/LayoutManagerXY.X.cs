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
        }
    }
}
