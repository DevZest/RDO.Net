using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    partial class LayoutManager
    {
        private sealed class AutoSizeUnit
        {
            public AutoSizeUnit(TemplateUnit templateUnit, AutoSizeDirection autoSizeDirection)
            {
                Debug.Assert(templateUnit != null);
                Debug.Assert(autoSizeDirection != AutoSizeDirection.None);

                TemplateUnit = templateUnit;
                AutoSizeDirection = autoSizeDirection;
            }

            public TemplateUnit TemplateUnit { get; private set; }

            private GridRange GridRange
            {
                get { return TemplateUnit.GridRange; }
            }

            private AutoSizeDirection AutoSizeDirection { get; set; }

            private bool IsAutoX
            {
                get { return (AutoSizeDirection & AutoSizeDirection.X) == AutoSizeDirection.X; }
            }

            private bool IsAutoY
            {
                get { return (AutoSizeDirection & AutoSizeDirection.Y) == AutoSizeDirection.Y; }
            }

            public bool IsScalar
            {
                get { return TemplateUnit is ScalarUnit; }
            }

            public bool IsList
            {
                get { return !IsScalar; }
            }

            private GridTemplate Template
            {
                get { return TemplateUnit.Owner; }
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
                    result += gridTracks[i].ActualLength;

                return result;
            }

            private UIElement GetUIElmenet(RowView row)
            {
                return row == null ? ScalarElement : GetListElement(row);
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

                    var ordinal = TemplateUnit.Ordinal;
                    return ordinal < Template.NumberOfScallarUnitsBeforeRow ? ordinal : LayoutManager.Elements.Count - (Template.ScalarUnits.Count - ordinal);
                }
            }

            private UIElement GetListElement(RowView row)
            {
                Debug.Assert(IsList && row.Form != null);

                return row.Form.Elements[TemplateUnit.Ordinal];
            }

            public void Measure(RowView row)
            {
                var uiElement = GetUIElmenet(row);
                uiElement.Measure(ConstraintSize);
                //OnMeasured(row, uiElement.DesiredSize);
            }
        }
    }
}
