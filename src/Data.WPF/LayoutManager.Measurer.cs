using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    partial class LayoutManager
    {
        private sealed class Measurer
        {
            public static Measurer GetOrCreate(Measurer oldValue, GridTemplate template, Size availableSize)
            {
                bool sizeToContentX = double.IsPositiveInfinity(availableSize.Width);
                bool sizeToContentY = double.IsPositiveInfinity(availableSize.Height);

                return oldValue == null || oldValue.ShouldRecreate(sizeToContentX, sizeToContentY)
                    ? new Measurer(template, sizeToContentX, sizeToContentY) : oldValue;
            }

            private Measurer(GridTemplate template, bool sizeToContentX, bool sizeToContentY)
            {
                Template = template;
                _sizeToContentX = sizeToContentX;
                _sizeToContentY = sizeToContentY;
                _autoSizeUnits = GetAutoSizeMeasurers(template, sizeToContentX, sizeToContentY).OrderBy(x => x.TemplateUnit.AutoSizeMeasureOrder).ToArray();
            }

            private static IEnumerable<AutoSizeMeasurer> GetAutoSizeMeasurers(GridTemplate template, bool sizeToContentX, bool sizeToContentY)
            {
                var scalarUnits = template.ScalarUnits;
                var listUnits = template.ListUnits;
                AutoSizeMeasurer autoSizeUnit;
                for (int i = 0; i < template.NumberOfScalarUnitsBeforeRow; i++)
                {
                    var scalarUnit = template.ScalarUnits[i];
                    if (TryGetAutoSizeMeasurer(scalarUnit, sizeToContentX, sizeToContentY, out autoSizeUnit))
                        yield return autoSizeUnit;
                }

                for (int i = 0; i < listUnits.Count; i++)
                {
                    var listUnit = template.ListUnits[i];
                    if (TryGetAutoSizeMeasurer(listUnit, sizeToContentX, sizeToContentY, out autoSizeUnit))
                        yield return autoSizeUnit;
                }

                for (int i = template.NumberOfScalarUnitsBeforeRow; i < scalarUnits.Count; i++)
                {
                    var scalarUnit = template.ScalarUnits[i];
                    if (TryGetAutoSizeMeasurer(scalarUnit, sizeToContentX, sizeToContentY, out autoSizeUnit))
                        yield return autoSizeUnit;
                }
            }

            private static bool TryGetAutoSizeMeasurer(TemplateUnit templateUnit, bool sizeToContentX, bool sizeToContentY, out AutoSizeMeasurer result)
            {
                result = null;

                if (templateUnit.AutoSizeMeasureOrder < 0)
                    return false;

                var autoSizeTracks = GetAutoSizeTracks(templateUnit.GridRange, sizeToContentX, sizeToContentY);
                if (autoSizeTracks.IsAutoX || autoSizeTracks.IsAutoY)
                {
                    result = new AutoSizeMeasurer(templateUnit, autoSizeTracks.Columns, autoSizeTracks.Rows);
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

            public readonly GridTemplate Template;

            private bool _sizeToContentX;

            private bool _sizeToContentY;

            private bool ShouldRecreate(bool sizeToContentX, bool sizeToContentY)
            {
                return (Template.StarColumnsCount > 0 && _sizeToContentX != sizeToContentX) ||
                    (Template.StarRowsCount > 0 && _sizeToContentY != sizeToContentY);
            }

            private AutoSizeMeasurer[] _autoSizeUnits;

        }
    }
}
