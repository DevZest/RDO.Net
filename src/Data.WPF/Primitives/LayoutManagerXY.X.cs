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
            }

            protected override IGridTrackCollection MainAxisGridTracks
            {
                get { return Template.InternalGridColumns; }
            }

            protected override IGridTrackCollection CrossAxisGridTracks
            {
                get { return Template.InternalGridRows; }
            }
        }
    }
}
