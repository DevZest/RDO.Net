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
            }

            protected override IGridTrackCollection MainAxisGridTracks
            {
                get { return Template.InternalGridRows; }
            }

            protected override IGridTrackCollection CrossAxisGridTracks
            {
                get { return Template.InternalGridColumns; }
            }
        }
    }
}
