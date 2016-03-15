using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public abstract class GridTrack : IGridTrackSet
    {
        internal GridTrack(Template owner, int ordinal, GridLengthParser.Result result)
        {
            Owner = owner;
            Ordinal = ordinal;
            Length = result.Length;
            MinLength = result.MinLength;
            MaxLength = result.MaxLength;
        }

        internal Template Owner { get; private set; }

        internal int Ordinal { get; private set; }

        public GridLength Length { get; private set; }

        public double MinLength { get; private set; }

        public double MaxLength { get; private set; }

        internal int AutoLengthIndex { get; set; }

        internal double MeasuredLength { get; set; }

        internal double AccumulatedLength { get; set; }

        int IGridTrackSet.Count
        {
            get { return 1; }
        }

        GridTrack IGridTrackSet.this[int index]
        {
            get
            {
                Debug.Assert(index == 0);
                return this;
            }
        }

        public abstract Orientation Orientation { get; }

        internal IReadOnlyList<GridTrack> GridTracks
        {
            get
            {
                if (Orientation == Orientation.Horizontal)
                    return Owner.GridColumns;
                else
                    return Owner.GridRows;
            }
        }
    }
}
