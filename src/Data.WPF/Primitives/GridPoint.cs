﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
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

        private static double GetOffset(IReadOnlyList<GridTrack> gridTracks, int gridOffset)
        {
            Debug.Assert(gridTracks.Count > 0);
            Debug.Assert(gridOffset >= 0 && gridOffset <= gridTracks.Count);

            return gridOffset == gridTracks.Count ? gridTracks[gridOffset - 1].EndOffset : gridTracks[gridOffset].StartOffset;
        }
    }
}
