using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    partial class LayoutManager
    {
        private sealed class AutoSizeItem
        {
            private struct AutoSizeInfo
            {
                public static AutoSizeInfo Empty
                {
                    get { return new AutoSizeInfo(GridColumnSet.Empty, GridRowSet.Empty); }
                }

                public AutoSizeInfo(IGridColumnSet gridColumns, IGridRowSet gridRows)
                {
                    Debug.Assert(gridColumns != null);
                    Debug.Assert(gridRows != null);
                    GridColumns = gridColumns;
                    GridRows = gridRows;
                }

                public readonly IGridColumnSet GridColumns;

                public readonly IGridRowSet GridRows;

                public bool IsEmpty
                {
                    get { return GridColumns.Count == 0 && GridRows.Count == 0; }
                }
            }

            private AutoSizeItem(TemplateItem templateItem, IGridColumnSet autoSizeGridColumns, IGridRowSet autoSizeGridRows)
            {
                Debug.Assert(templateItem != null);
                Debug.Assert(autoSizeGridColumns.Count > 0 || autoSizeGridRows.Count > 0);

                TemplateItem = templateItem;
                AutoSizeGridColumns = autoSizeGridColumns;
                AutoSizeGridRows = autoSizeGridRows;

                _totalAbsoluteWidth = CalcTotalAbsoluteWidth();
                _totalAbsoluteHeight = CalcTotalAbsoluteHeight();
            }

            private double CalcTotalAbsoluteWidth()
            {
                var result = 0.0d;
                int autoSizeColumnIndex = 0;
                for (int i = GridRange.Left.Ordinal; i <= GridRange.Right.Ordinal; i++)
                {
                    var column = Template.GridColumns[i];
                    if (autoSizeColumnIndex < AutoSizeGridColumns.Count && column == AutoSizeGridColumns[autoSizeColumnIndex])
                    {
                        autoSizeColumnIndex++;
                        continue;
                    }
                    Debug.Assert(column.Length.IsAbsolute, "Items contain both auto and star size should have been ignored.");
                    result += column.MeasuredLength;
                }
                return result;
            }

            private double CalcTotalAbsoluteHeight()
            {
                var result = 0.0d;
                int autoSizeRowIndex = 0;
                for (int i = GridRange.Top.Ordinal; i <= GridRange.Bottom.Ordinal; i++)
                {
                    var row = Template.GridRows[i];
                    if (autoSizeRowIndex < AutoSizeGridColumns.Count && row == AutoSizeGridRows[autoSizeRowIndex])
                    {
                        autoSizeRowIndex++;
                        continue;
                    }
                    Debug.Assert(row.Length.IsAbsolute, "Items contain both auto and star size should have been ignored.");
                    result += row.MeasuredLength;
                }
                return result;
            }

            public TemplateItem TemplateItem { get; private set; }

            private GridRange GridRange
            {
                get { return TemplateItem.GridRange; }
            }

            public readonly IGridColumnSet AutoSizeGridColumns;

            public readonly IGridRowSet AutoSizeGridRows;

            private readonly double _totalAbsoluteWidth;

            private readonly double _totalAbsoluteHeight;

            private bool IsAutoX
            {
                get { return AutoSizeGridColumns.Count > 0; }
            }

            private bool IsAutoY
            {
                get { return AutoSizeGridRows.Count > 0; }
            }

            public bool IsScalar
            {
                get { return TemplateItem is ScalarItem; }
            }

            public bool IsRepeat
            {
                get { return !IsScalar; }
            }

            private GridTemplate Template
            {
                get { return TemplateItem.Owner; }
            }

            private LayoutManager LayoutManager
            {
                get { return Template.Owner.LayoutManager; }
            }

            private Size ConstraintSize
            {
                get
                {
                    var width = IsAutoX ? double.PositiveInfinity : CalcLength(Template.GridColumns, GridRange.Left, GridRange.Right);
                    var height = IsAutoY ? double.PositiveInfinity : CalcLength(Template.GridRows, GridRange.Top, GridRange.Bottom);
                    return new Size(width, height);
                }
            }

            private static double CalcLength(IReadOnlyList<GridTrack> gridTracks, GridTrack start, GridTrack end)
            {
                double result = 0;
                for (int i = start.Ordinal; i <= end.Ordinal; i++)
                    result += gridTracks[i].MeasuredLength;

                return result;
            }

            private UIElement ScalarElement
            {
                get { return LayoutManager.Elements[ScalarElementIndex]; }
            }

            private int ScalarElementIndex
            {
                get
                {
                    Debug.Assert(IsScalar);

                    var ordinal = TemplateItem.Ordinal;
                    return ordinal < Template.ScalarItemsCountBeforeRepeat ? ordinal : LayoutManager.Elements.Count - (Template.ScalarItems.Count - ordinal);
                }
            }

            public void MeasureScalar(int repeatIndex)
            {
                Debug.Assert(IsScalar);

                var uiElement = ScalarElement;
                var scalarElementsContainer = uiElement as ScalarElementsContainer;
                if (scalarElementsContainer != null)
                    uiElement = scalarElementsContainer[LayoutManager.GrowOrientation, repeatIndex];
                Measure(uiElement, repeatIndex, repeatIndex); // when repeatIndex > 0, either AutoSizeGridColumns or AutoSizeGridRows is empty.
            }

            public void MeasureRepeat(RowPresenter row)
            {
                Debug.Assert(IsRepeat);
                var uiElement = row.Form.Elements[TemplateItem.Ordinal];
                var repeatPosition = LayoutManager.GetRepeatPosition(row);
                Measure(uiElement, repeatPosition.X, repeatPosition.Y);
            }

            private void Measure(UIElement uiElement, int repeatIndexX, int repeatIndexY)
            {
                uiElement.Measure(ConstraintSize);
                var desiredSize = uiElement.DesiredSize;
                UpdateMeasuredLength(repeatIndexX, AutoSizeGridColumns, desiredSize.Width - _totalAbsoluteWidth);
                UpdateMeasuredLength(repeatIndexY, AutoSizeGridRows, desiredSize.Height - _totalAbsoluteHeight);
            }

            private void UpdateMeasuredLength(int repeatIndex, IGridTrackSet autoSizeTracks, double length)
            {
                if (autoSizeTracks.Count == 0 || length <= 0)
                    return;

                for (int i = 0; i < autoSizeTracks.Count; i++)
                {
                    var track = autoSizeTracks[i];
                    var measuredLength = LayoutManager.GetMeasuredLength(track, repeatIndex);
                    if (i < autoSizeTracks.Count - 1)   // not the last track
                    {
                        length -= measuredLength;
                        if (length <= 0)
                            return;
                    }
                    else if (length > measuredLength)
                        LayoutManager.SetMeasureLength(track, repeatIndex, length);
                }
            }

            public static IList<AutoSizeItem> GenerateList(GridTemplate template, bool sizeToContentX, bool sizeToContentY, bool reset)
            {
                IList<AutoSizeItem> result = EmptyArray<AutoSizeItem>.Singleton;

                var scalarItems = template.ScalarItems;
                var repeatItems = template.RepeatItems;
                for (int i = 0; i < template.ScalarItemsCountBeforeRepeat; i++)
                    result = GenerateList(result, scalarItems[i], sizeToContentX, sizeToContentY);

                for (int i = 0; i < repeatItems.Count; i++)
                    result = GenerateList(result, repeatItems[i], sizeToContentX, sizeToContentY);

                for (int i = template.ScalarItemsCountBeforeRepeat; i < scalarItems.Count; i++)
                    result = GenerateList(result, scalarItems[i], sizeToContentX, sizeToContentY);

                result.Sort((x, y) => Compare(x, y));

                return result;
            }

            private static int Compare(AutoSizeItem x, AutoSizeItem y)
            {
                var order1 = x.TemplateItem.AutoSizeMeasureOrder;
                var order2 = y.TemplateItem.AutoSizeMeasureOrder;
                if (order1 > order2)
                    return 1;
                else if (order1 < order2)
                    return -1;
                else
                    return 0;
            }

            private static IList<AutoSizeItem> GenerateList(IList<AutoSizeItem> autoSizeItems, TemplateItem templateItem, bool sizeToContentX, bool sizeToContentY)
            {
                var autoSizeItem = Generate(templateItem, sizeToContentX, sizeToContentY);
                if (autoSizeItem == null)
                    return autoSizeItems;

                if (autoSizeItems == EmptyArray<AutoSizeItem>.Singleton)
                    autoSizeItems = new List<AutoSizeItem>();
                autoSizeItems.Add(autoSizeItem);

                return autoSizeItems;
            }

            private static AutoSizeItem Generate(TemplateItem templateItem, bool sizeToContentX, bool sizeToContentY)
            {
                if (templateItem.AutoSizeMeasureOrder < 0)
                    return null;

                var info = GetAutoSizeInfo(templateItem.GridRange, sizeToContentX, sizeToContentY);
                return info.IsEmpty ? null : new AutoSizeItem(templateItem, info.GridColumns, info.GridRows);
            }

            private static AutoSizeInfo GetAutoSizeInfo(GridRange gridRange, bool sizeToContentX, bool sizeToContentY)
            {
                //  There is an issue with items contains both auto and star tracks.
                //  Intuitively, we expect that those items receive enough space to layout and that this space is perfectly divided into the auto / star tracks.
                //  The problem is that it is not possible to determine the size of star tracks until all auto track size determined,
                //  and that it is not possible determine missing space to include into the auto-sized tracks for those items as long as we don't know the size 
                //  of star-sized tracks.
                //  We are in a dead-end. There is basically two solutions: 
                //     1. Include all the missing size for those items into the auto tracks
                //     2. Include none of the missing size into the auto tracks and hope that the star tracks will be big enough to contain those items.
                //  Here we chose option (2), that is we ignore those elements during calculation of auto-sized tracks.
                //  The reason between this choice is that (1) will tend to increase excessively the size of auto-sized tracks (for nothing).
                //  Moreover, we consider items included both auto and star-size tracks are rare, and most of the time we want
                //  to be spread along several tracks rather than auto-sized.

                var columnSet = GridColumnSet.Empty;
                if (ContainsStarWidth(gridRange, sizeToContentX))
                {
                    for (int x = gridRange.Left.Ordinal; x <= gridRange.Right.Ordinal; x++)
                    {
                        var column = gridRange.Owner.GridColumns[x];
                        var width = column.Width;
                        if (width.IsAuto || (width.IsStar && sizeToContentX))
                            columnSet = columnSet.Merge(column);
                    }
                }

                var rowSet = GridRowSet.Empty;
                if (ContainsStarHeight(gridRange, sizeToContentY))
                {
                    for (int y = gridRange.Top.Ordinal; y <= gridRange.Bottom.Ordinal; y++)
                    {
                        var row = gridRange.Owner.GridRows[y];
                        var height = row.Height;
                        if (height.IsAuto || (height.IsStar && sizeToContentY))
                            rowSet = rowSet.Merge(row);
                    }
                }

                return new AutoSizeInfo(columnSet, rowSet);
            }

            private static bool ContainsStarWidth(GridRange gridRange, bool sizeToContentX)
            {
                if (sizeToContentX)
                    return false;

                for (int x = gridRange.Left.Ordinal; x <= gridRange.Right.Ordinal; x++)
                {
                    if (gridRange.Owner.GridColumns[x].Width.IsStar)
                        return true;
                }
                return false;
            }

            private static bool ContainsStarHeight(GridRange gridRange, bool sizeToContentY)
            {
                if (sizeToContentY)
                    return false;

                for (int y = gridRange.Top.Ordinal; y <= gridRange.Bottom.Ordinal; y++)
                {
                    if (gridRange.Owner.GridRows[y].Height.IsStar)
                        return true;
                }
                return false;
            }
        }
    }
}
