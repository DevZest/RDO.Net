using System.Collections.Generic;
using System.Windows;
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
        Vector ToVector(double valueMain, double valueCross);
        int FrozenHead { get; }
        int FrozenTail { get; }
        GridTrack ContainerStart { get; }
        GridTrack ContainerEnd { get; }
        int MaxFrozenHead { get; }
        int MaxFrozenTail { get; }
        bool SizeToContent { get; }
        double AvailableLength { get; }
        double GetMeasuredLength(GridRange gridRange);
        GridSpan GetGridSpan(GridRange gridRange);
        bool VariantByContainer { get; }
    }
}
