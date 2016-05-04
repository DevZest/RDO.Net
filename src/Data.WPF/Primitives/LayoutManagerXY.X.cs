using System;
using System.Collections.Generic;
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
                _variantAutoWidthColumns = template.InitVariantAutoWidthGridColumns();
            }

            private readonly IConcatList<GridColumn> _variantAutoWidthColumns;
            internal override IReadOnlyList<GridTrack> VariantAutoLengthTracks
            {
                get { return _variantAutoWidthColumns; }
            }

            protected override Vector BlockDimensionVector
            {
                get { return new Vector(Template.RowRange.MeasuredWidth, 0); }
            }

            protected override IReadOnlyList<GridTrack> MainAxisGridTracks
            {
                get { return Template.GridColumns; }
            }
        }
    }
}
