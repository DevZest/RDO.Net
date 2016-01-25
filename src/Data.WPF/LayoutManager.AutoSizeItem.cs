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

                FilterGridColumns(out AbsoluteSizeGridColumns, out StarSizeGridColumns);
                FilterGridRows(out AbsoluteSizeGridRows, out StarSizeGridRows);
            }

            private void FilterGridColumns(out IGridColumnSet absoluteSizeGridColumns, out IGridColumnSet starSizeGridColumns)
            {
                absoluteSizeGridColumns = starSizeGridColumns = GridColumnSet.Empty;
                int autoSizeColumnIndex = 0;
                for (int i = GridRange.Left.Ordinal; i <= GridRange.Right.Ordinal; i++)
                {
                    var column = Template.GridColumns[i];
                    if (autoSizeColumnIndex < AutoSizeGridColumns.Count && column == AutoSizeGridColumns[autoSizeColumnIndex])
                    {
                        autoSizeColumnIndex++;
                        continue;
                    }
                    if (column.Length.IsAbsolute)
                        absoluteSizeGridColumns = absoluteSizeGridColumns.Merge(column);
                    else
                    {
                        Debug.Assert(column.Length.IsStar);
                        starSizeGridColumns = starSizeGridColumns.Merge(column);
                    }
                }
            }

            private void FilterGridRows(out IGridRowSet absoluteSizeGridRows, out IGridRowSet starSizeGridRows)
            {
                absoluteSizeGridRows = starSizeGridRows = GridRowSet.Empty;
                int autoSizeRowIndex = 0;
                for (int i = GridRange.Top.Ordinal; i <= GridRange.Bottom.Ordinal; i++)
                {
                    var row = Template.GridRows[i];
                    if (autoSizeRowIndex < AutoSizeGridRows.Count && row == AutoSizeGridRows[autoSizeRowIndex])
                    {
                        autoSizeRowIndex++;
                        continue;
                    }
                    if (row.Length.IsAbsolute)
                        absoluteSizeGridRows = absoluteSizeGridRows.Merge(row);
                    else
                    {
                        Debug.Assert(row.Length.IsStar);
                        starSizeGridRows = starSizeGridRows.Merge(row);
                    }
                }
            }

            public TemplateItem TemplateItem { get; private set; }

            private GridRange GridRange
            {
                get { return TemplateItem.GridRange; }
            }

            public readonly IGridColumnSet AutoSizeGridColumns;

            public readonly IGridRowSet AutoSizeGridRows;

            public readonly IGridColumnSet AbsoluteSizeGridColumns;

            public readonly IGridRowSet AbsoluteSizeGridRows;

            public readonly IGridColumnSet StarSizeGridColumns;

            public readonly IGridRowSet StarSizeGridRows;

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
                    var width = IsAutoX ? double.PositiveInfinity : GetLength(Template.GridColumns, GridRange.Left, GridRange.Right);
                    var height = IsAutoY ? double.PositiveInfinity : GetLength(Template.GridRows, GridRange.Top, GridRange.Bottom);
                    return new Size(width, height);
                }
            }

            private static double GetLength(IReadOnlyList<GridTrack> gridTracks, GridTrack start, GridTrack end)
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

            public Size Measure(RowView row)
            {
                var uiElement = IsScalar ? ScalarElement : GetListElement(row);
                uiElement.Measure(ConstraintSize);
                return uiElement.DesiredSize;
            }
        }
    }
}
