using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    partial class LayoutManager
    {
        private abstract class AutoSizeMeasurer
        {
            public static AutoSizeMeasurer Create(TemplateUnit templateUnit, AutoSizeDirection autoSizeDirection)
            {
                var gridRange = templateUnit.GridRange;
                bool isSpanned = gridRange.Left != gridRange.Right || gridRange.Top != gridRange.Bottom;

                return isSpanned ? new SpannedAutoSizeMeasurer(templateUnit, autoSizeDirection) : new SpannedAutoSizeMeasurer(templateUnit, autoSizeDirection);
            }

            private sealed class SingleCellAutoSizeMeasurer : AutoSizeMeasurer
            {
                public SingleCellAutoSizeMeasurer(TemplateUnit templateUnit, AutoSizeDirection autoSizeDirection)
                    : base(templateUnit, autoSizeDirection)
                {
                }

                protected override void OnMeasured(RowView row, Size desiredSize)
                {
                    throw new NotImplementedException();
                }
            }

            private sealed class SpannedAutoSizeMeasurer : AutoSizeMeasurer
            {
                public SpannedAutoSizeMeasurer(TemplateUnit templateUnit, AutoSizeDirection autoSizeDirection)
                    : base(templateUnit, autoSizeDirection)
                {
                }

                protected override void OnMeasured(RowView row, Size desiredSize)
                {
                    throw new NotImplementedException();
                }
            }

            protected AutoSizeMeasurer(TemplateUnit templateUnit, AutoSizeDirection autoSizeDirection)
            {
                Debug.Assert(templateUnit != null);
                Debug.Assert(autoSizeDirection != AutoSizeDirection.None);

                TemplateUnit = templateUnit;
                AutoSizeDirection = autoSizeDirection;
            }

            public TemplateUnit TemplateUnit { get; private set; }

            protected GridRange GridRange
            {
                get { return TemplateUnit.GridRange; }
            }

            protected AutoSizeDirection AutoSizeDirection { get; private set; }

            protected bool IsAutoX
            {
                get { return (AutoSizeDirection & AutoSizeDirection.X) == AutoSizeDirection.X; }
            }

            protected bool IsAutoY
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

            protected GridTemplate Template
            {
                get { return TemplateUnit.Owner; }
            }

            protected LayoutManager LayoutManager
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
                OnMeasured(row, uiElement.DesiredSize);
            }

            protected abstract void OnMeasured(RowView row, Size desiredSize);
        }
    }
}
