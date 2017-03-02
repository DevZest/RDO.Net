using DevZest.Windows.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Windows.Data
{
    public struct GridPoint
    {
        public readonly int X;
        public readonly int Y;

        public GridPoint(int x, int y)
        {
            if (x < 0)
                throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0)
                throw new ArgumentOutOfRangeException(nameof(y));

            X = x;
            Y = y;
        }

        internal Point ToPoint(Template template)
        {
            var offsetX = GetOffset(template.GridColumns, X);
            var offsetY = GetOffset(template.GridRows, Y);
            return new Point(offsetX, offsetY);
        }

        private static double GetOffset(IReadOnlyList<GridTrack> gridTracks, int gridPoint)
        {
            Debug.Assert(gridTracks.Count > 0);
            Debug.Assert(gridPoint >= 0 && gridPoint <= gridTracks.Count);

            return gridPoint == gridTracks.Count ? gridTracks[gridPoint - 1].EndOffset : gridTracks[gridPoint].StartOffset;
        }
    }
}
