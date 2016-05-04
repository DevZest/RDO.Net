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
                _variantAutoHeightRows = template.InitVariantAutoHeightGridRows();
            }

            private readonly IConcatList<GridRow> _variantAutoHeightRows;
            internal override IReadOnlyList<GridTrack> VariantAutoLengthTracks
            {
                get { return _variantAutoHeightRows; }
            }

            protected override Vector BlockDimensionVector
            {
                get { return new Vector(0, Template.RowRange.MeasuredHeight); }
            }

            protected override IReadOnlyList<GridTrack> MainAxisGridTracks
            {
                get { return Template.GridRows; }
            }
        }
    }
}
