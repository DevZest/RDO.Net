using System;
using System.Collections.Generic;
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
                _variantAutoHeightRows = CalcVariantAutoLengthTracks(template.InternalGridRows, rowRange.Top, rowRange.Bottom);
            }

            private readonly IConcatList<GridRow> _variantAutoHeightRows;
            protected override IReadOnlyList<GridTrack> VariantAutoLengthTracks
            {
                get { return _variantAutoHeightRows; }
            }

            protected override Point Offset(Point point, int blockDimension)
            {
                point.Offset(Template.RowRange.MeasuredWidth * blockDimension, 0);
                return point;
            }

            protected override Size GetMeasuredSize(DataItem dataItem)
            {
                var gridRange = dataItem.GridRange;
                var width = gridRange.MeasuredWidth;
                var height = gridRange.MeasuredHeight;
                if (dataItem.BlockDimensions > 1)
                    width += Template.RowRange.MeasuredWidth * (BlockDimensions - 1);
                return new Size(width, height);
            }
        }
    }
}
