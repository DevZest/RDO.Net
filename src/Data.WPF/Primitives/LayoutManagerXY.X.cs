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
                : base(template.InternalGridColumns, dataSet)
            {
            }

            protected override Vector BlockDimensionVector
            {
                get { return new Vector(Template.RowRange.MeasuredWidth, 0); }
            }
        }
    }
}
