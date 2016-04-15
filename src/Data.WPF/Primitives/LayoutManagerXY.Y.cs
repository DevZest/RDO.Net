using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    partial class LayoutManagerXY
    {
        private sealed class Y : LayoutManagerXY
        {
            public Y(Template template, DataSet dataSet)
                : base(template, dataSet)
            {
                var rowRange = template.RowRange;
                _variantAutoHeightRows = CalcVariantAutoLengthTracks(template.GridRows, rowRange.Top, rowRange.Bottom);
            }

            private readonly IConcatList<GridRow> _variantAutoHeightRows;

            protected override Point Offset(Point point, int blockDimension)
            {
                point.Offset(Template.RowRange.MeasuredWidth * blockDimension, 0);
                return point;
            }
        }
    }
}
