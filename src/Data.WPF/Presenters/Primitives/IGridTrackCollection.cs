using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters.Primitives
{
    internal interface IGridTrackCollection : IReadOnlyList<GridTrack>
    {
        Template Template { get; }
        Orientation Orientation { get; }
        double TotalAutoLength { get; set; }
        void InvalidateOffset();
        void RefreshOffset();
        Vector ToVector(double valueMain, double valueCross);
        int FrozenHeadTracksCount { get; }
        int FrozenTailTracksCount { get; }
        GridTrack ContainerStart { get; }
        GridTrack ContainerEnd { get; }
        GridTrack RowStart { get; }
        GridTrack RowEnd { get; }
        int HeadTracksCount { get; }
        int ContainerTracksCount { get; }
        int RowTracksCount { get; }
        int TailTracksCount { get; }
        bool SizeToContent { get; }
        double AvailableLength { get; }
        double GetMeasuredLength(GridRange gridRange);
        GridSpan GetGridSpan(GridRange gridRange);
        bool VariantByContainer { get; }
        void OnResized(GridTrack gridTrack, GridLength oldValue);
    }
}
