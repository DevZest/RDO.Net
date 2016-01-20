using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace DevZest.Data.Windows
{
    partial class LayoutManager
    {
        private sealed class AutoSizeMeasurerCollection : ReadOnlyCollection<AutoSizeMeasurer>
        {
            public static AutoSizeMeasurerCollection GetOrCreate(AutoSizeMeasurerCollection oldValue, GridTemplate template, Size availableSize)
            {
                bool sizeToContentX = double.IsPositiveInfinity(availableSize.Width);
                bool sizeToContentY = double.IsPositiveInfinity(availableSize.Height);

                if (oldValue != null && oldValue._sizeToContentX == sizeToContentX && oldValue._sizeToContentY == sizeToContentY)
                    return oldValue;

                var autoSizeMeasurers = CreateAutoSizeMeasurers(template, sizeToContentX, sizeToContentY)
                    .OrderBy(x => x.TemplateUnit.AutoSizeMeasureOrder).ToArray();

                return new AutoSizeMeasurerCollection(autoSizeMeasurers, sizeToContentX, sizeToContentY);
            }

            private static IEnumerable<AutoSizeMeasurer> CreateAutoSizeMeasurers(GridTemplate template, bool sizeToContentX, bool sizeToContentY)
            {
                var scalarUnits = template.ScalarUnits;
                var listUnits = template.ListUnits;
                AutoSizeMeasurer measurer;
                for (int i = 0; i < template.NumberOfScallarUnitsBeforeRow; i++)
                {
                    var scalarUnit = template.ScalarUnits[i];
                    if (TryCreateAutoSizeMeasurer(scalarUnit, sizeToContentX, sizeToContentY, out measurer))
                        yield return measurer;
                }

                for (int i = 0; i < listUnits.Count; i++)
                {
                    var listUnit = template.ListUnits[i];
                    if (TryCreateAutoSizeMeasurer(listUnit, sizeToContentX, sizeToContentY, out measurer))
                        yield return measurer;
                }

                for (int i = template.NumberOfScallarUnitsBeforeRow; i < scalarUnits.Count; i++)
                {
                    var scalarUnit = template.ScalarUnits[i];
                    if (TryCreateAutoSizeMeasurer(scalarUnit, sizeToContentX, sizeToContentY, out measurer))
                        yield return measurer;
                }
            }

            private static bool TryCreateAutoSizeMeasurer(TemplateUnit templateUnit, bool sizeToContentX, bool sizeToContentY, out AutoSizeMeasurer result)
            {
                result = null;

                if (templateUnit.AutoSizeMeasureOrder < 0)
                    return false;

                var autoSizeDirection = GetAutoSizeDirection(templateUnit.GridRange, sizeToContentX, sizeToContentY);
                if (autoSizeDirection == AutoSizeDirection.None)
                    return false;

                result = AutoSizeMeasurer.Create(templateUnit, autoSizeDirection);
                return true;
            }

            private static AutoSizeDirection GetAutoSizeDirection(GridRange gridRange, bool sizeToContentX, bool sizeToContentY)
            {
                var result = AutoSizeDirection.None;

                for (int x = gridRange.Left.Ordinal; x <= gridRange.Right.Ordinal; x++)
                {
                    var width = gridRange.Owner.GridColumns[x].Width;
                    if (width.IsAuto || (width.IsStar && sizeToContentX))
                    {
                        result |= AutoSizeDirection.X;
                        break;
                    }
                }

                for (int y = gridRange.Top.Ordinal; y <= gridRange.Bottom.Ordinal; y++)
                {
                    var height = gridRange.Owner.GridRows[y].Height;
                    if (height.IsAuto || (height.IsStar && sizeToContentY))
                    {
                        result |= AutoSizeDirection.Y;
                        break;
                    }
                }

                return result;
            }

            private AutoSizeMeasurerCollection(IList<AutoSizeMeasurer> autoSizeMeasurers, bool sizeToContentX, bool sizeToContentY)
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
