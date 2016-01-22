using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace DevZest.Data.Windows
{
    partial class LayoutManager
    {
        private sealed class AutoSizeMeasurer : ReadOnlyCollection<AutoSizeUnit>
        {
            public static AutoSizeMeasurer GetOrCreate(AutoSizeMeasurer oldValue, GridTemplate template, Size availableSize)
            {
                bool sizeToContentX = double.IsPositiveInfinity(availableSize.Width);
                bool sizeToContentY = double.IsPositiveInfinity(availableSize.Height);

                if (oldValue != null && oldValue._sizeToContentX == sizeToContentX && oldValue._sizeToContentY == sizeToContentY)
                    return oldValue;

                var autoSizeUnits = CreateAutoSizeUnits(template, sizeToContentX, sizeToContentY)
                    .OrderBy(x => x.TemplateUnit.AutoSizeMeasureOrder).ToArray();

                return new AutoSizeMeasurer(autoSizeUnits, sizeToContentX, sizeToContentY);
            }

            private static IEnumerable<AutoSizeUnit> CreateAutoSizeUnits(GridTemplate template, bool sizeToContentX, bool sizeToContentY)
            {
                var scalarUnits = template.ScalarUnits;
                var listUnits = template.ListUnits;
                AutoSizeUnit measurer;
                for (int i = 0; i < template.NumberOfScallarUnitsBeforeRow; i++)
                {
                    var scalarUnit = template.ScalarUnits[i];
                    if (TryCreateAutoSizeUnit(scalarUnit, sizeToContentX, sizeToContentY, out measurer))
                        yield return measurer;
                }

                for (int i = 0; i < listUnits.Count; i++)
                {
                    var listUnit = template.ListUnits[i];
                    if (TryCreateAutoSizeUnit(listUnit, sizeToContentX, sizeToContentY, out measurer))
                        yield return measurer;
                }

                for (int i = template.NumberOfScallarUnitsBeforeRow; i < scalarUnits.Count; i++)
                {
                    var scalarUnit = template.ScalarUnits[i];
                    if (TryCreateAutoSizeUnit(scalarUnit, sizeToContentX, sizeToContentY, out measurer))
                        yield return measurer;
                }
            }

            private static bool TryCreateAutoSizeUnit(TemplateUnit templateUnit, bool sizeToContentX, bool sizeToContentY, out AutoSizeUnit result)
            {
                result = null;

                if (templateUnit.AutoSizeMeasureOrder < 0)
                    return false;

                var autoSizeTracks = GetAutoSizeTracks(templateUnit.GridRange, sizeToContentX, sizeToContentY);
                if (autoSizeTracks.IsAutoX || autoSizeTracks.IsAutoY)
                {
                    result = new AutoSizeUnit(templateUnit, autoSizeTracks.Columns, autoSizeTracks.Rows);
                    return true;
                }
                return false;
            }

            private static AutoSizeTracks GetAutoSizeTracks(GridRange gridRange, bool sizeToContentX, bool sizeToContentY)
            {
                var columns = GridColumnSet.Empty;
                for (int x = gridRange.Left.Ordinal; x <= gridRange.Right.Ordinal; x++)
                {
                    var column = gridRange.Owner.GridColumns[x];
                    var width = column.Width;
                    if (width.IsAuto || (width.IsStar && sizeToContentX))
                        columns = columns.Merge(column);
                }

                var rows = GridRowSet.Empty;
                for (int y = gridRange.Top.Ordinal; y <= gridRange.Bottom.Ordinal; y++)
                {
                    var row = gridRange.Owner.GridRows[y];
                    var height = row.Height;
                    if (height.IsAuto || (height.IsStar && sizeToContentY))
                        rows = rows.Merge(row);
                }

                return new AutoSizeTracks(columns, rows);
            }

            private AutoSizeMeasurer(IList<AutoSizeUnit> autoSizeMeasurers, bool sizeToContentX, bool sizeToContentY)
                : base(autoSizeMeasurers)
            {
                _sizeToContentX = sizeToContentX;
                _sizeToContentY = sizeToContentY;
            }

            private bool _sizeToContentX;

            private bool _sizeToContentY;
        }
    }
}
