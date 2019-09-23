using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents an x- and y-coordinate pair in two-dimensional layout grid space to identify a grid cell.
    /// </summary>
    public struct GridPoint
    {
        /// <summary>
        /// Gets the X-coordinate value of this <see cref="GridPoint"/> structure.
        /// </summary>
        public readonly int X;

        /// <summary>
        /// Gets the Y-coordinate value of this <see cref="GridPoint"/> structure.
        /// </summary>
        public readonly int Y;

        /// <summary>
        /// Initializes a new instance of <see cref="GridPoint"/>.
        /// </summary>
        /// <param name="x">The index of grid column.</param>
        /// <param name="y">The index of grid row.</param>
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
