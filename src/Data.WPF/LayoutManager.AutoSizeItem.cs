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
        }
    }
}
