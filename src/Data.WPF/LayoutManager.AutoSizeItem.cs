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
            public AutoSizeItem(TemplateItem templateItem, IGridColumnSet autoSizeGridColumns, IGridRowSet autoSizeGridRows)
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
                    result += column.MeasuredWidth;
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
                    result += row.MeasuredHeight;
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

            public bool IsList
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
                    return ordinal < Template.ScalarItemsCountBeforeList ? ordinal : LayoutManager.Elements.Count - (Template.ScalarItems.Count - ordinal);
                }
            }

            private UIElement GetListElement(RowView row)
            {
                Debug.Assert(IsList && row.Form != null);

                return row.Form.Elements[TemplateItem.Ordinal];
            }

            public void Measure(RowView row)
            {
                var uiElement = IsScalar ? ScalarElement : GetListElement(row);
                uiElement.Measure(ConstraintSize);
                var desiredSize = uiElement.DesiredSize;
                UpdateMeasuredAutoSize(AutoSizeGridColumns, desiredSize.Width - _totalAbsoluteWidth);
                UpdateMeasuredAutoSize(AutoSizeGridRows, desiredSize.Height - _totalAbsoluteHeight);
            }

            private void UpdateMeasuredAutoSize(IGridTrackSet autoSizeTracks, double length)
            {
                if (autoSizeTracks.Count == 0 || length <= 0)
                    return;

                for (int i = 0; i < autoSizeTracks.Count; i++)
                {
                    length -= autoSizeTracks[i].MeasuredLength;
                    if (length <= 0)
                        return;
                }

                // increase MeasuredLength of last auto size track
                autoSizeTracks[autoSizeTracks.Count - 1].MeasuredLength += length;
            }

            public static IList<AutoSizeItem> Generate(LayoutManager layoutManager, bool sizeToContentX, bool sizeToContentY, bool reset)
            {
                var autoSizeItems = layoutManager._autoSizeItems;

                if (autoSizeItems != null && !reset)
                    return autoSizeItems;

                autoSizeItems = EmptyArray<AutoSizeItem>.Singleton;

                var template = layoutManager.Template;
                var scalarItems = template.ScalarItems;
                var listItems = template.ListItems;
                for (int i = 0; i < template.ScalarItemsCountBeforeList; i++)
                    autoSizeItems = GenerateAutoSizeItem(autoSizeItems, scalarItems[i], sizeToContentX, sizeToContentY);

                for (int i = 0; i < listItems.Count; i++)
                    autoSizeItems = GenerateAutoSizeItem(autoSizeItems, listItems[i], sizeToContentX, sizeToContentY);

                for (int i = template.ScalarItemsCountBeforeList; i < scalarItems.Count; i++)
                    autoSizeItems = GenerateAutoSizeItem(autoSizeItems, scalarItems[i], sizeToContentX, sizeToContentY);

                autoSizeItems.Sort((x, y) => Compare(x, y));

                return autoSizeItems;
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

            private static IList<AutoSizeItem> GenerateAutoSizeItem(IList<AutoSizeItem> autoSizeItems, TemplateItem templateItem, bool sizeToContentX, bool sizeToContentY)
            {
                var autoSizeItem = TryGenerateAutoSizeItem(templateItem, sizeToContentX, sizeToContentY);
                if (autoSizeItem == null)
                    return autoSizeItems;

                if (autoSizeItems == EmptyArray<AutoSizeItem>.Singleton)
                    autoSizeItems = new List<AutoSizeItem>();
                autoSizeItems.Add(autoSizeItem);

                return autoSizeItems;
            }

            private static AutoSizeItem TryGenerateAutoSizeItem(TemplateItem templateItem, bool sizeToContentX, bool sizeToContentY)
            {
                if (templateItem.AutoSizeMeasureOrder < 0)
                    return null;

                var entry = GetAutoSizeEntry(templateItem.GridRange, sizeToContentX, sizeToContentY);
                return entry.IsEmpty ? null : new AutoSizeItem(templateItem, entry.Columns, entry.Rows);
            }

            private static AutoSizeEntry GetAutoSizeEntry(GridRange gridRange, bool sizeToContentX, bool sizeToContentY)
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

                return new AutoSizeEntry(columnSet, rowSet);
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
