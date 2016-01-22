using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class GridTrack : IGridTrackSet
    {
        internal GridTrack(GridTemplate owner, int ordinal, GridLengthParser.Result result)
        {
            Owner = owner;
            Ordinal = ordinal;
            Length = result.Length;
            MinLength = result.MinLength;
            MaxLength = result.MaxLength;
        }

        internal GridTemplate Owner { get; private set; }

        internal int Ordinal { get; private set; }

        public GridLength Length { get; private set; }

        public double MinLength { get; private set; }

        public double MaxLength { get; private set; }

        public double MeasuredLength { get; internal set; }

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
    }
}
