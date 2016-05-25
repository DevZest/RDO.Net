using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public struct GridPoint
    {
        public readonly int OffsetX;
        public readonly int OffsetY;

        public GridPoint(int offsetX, int offsetY)
        {
            if (offsetX < 0)
                throw new ArgumentOutOfRangeException(nameof(offsetX));
            if (offsetY < 0)
                throw new ArgumentOutOfRangeException(nameof(offsetY));

            OffsetX = offsetX;
            OffsetY = offsetY;
        }

        internal Point ToPoint(Template template)
        {
            var offsetX = GetOffset(template.GridColumns, OffsetX);
            var offsetY = GetOffset(template.GridRows, OffsetY);
            return new Point(offsetX, offsetY);
        }

        private static double GetOffset(IReadOnlyList<GridTrack> gridTracks, int gridOffset)
        {
            Debug.Assert(gridTracks.Count > 0);
            Debug.Assert(gridOffset >= 0 && gridOffset <= gridTracks.Count);

            return gridOffset == gridTracks.Count ? gridTracks[gridOffset - 1].EndOffset : gridTracks[gridOffset].StartOffset;
        }
    }
}
