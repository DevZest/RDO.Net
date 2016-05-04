using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    internal interface IGridTrackCollection : IReadOnlyList<GridTrack>
    {
        Template Template { get; }
        Orientation Orientation { get; }
        double TotalAutoLength { get; set; }
        void InvalidateOffset();
        void RefreshOffset();
        IReadOnlyList<GridTrack> InitVariantAutoLengthTracks();
    }
}
