using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    partial class LayoutManager
    {
        private abstract class RepeatLayout : LayoutManager
        {
            public static new LayoutManager Create(DataView view)
            {
                var orientation = view.Template.RepeatOrientation;
                if (orientation == RepeatOrientation.Y)
                    return new LayoutY(view);
                else if (orientation == RepeatOrientation.XY)
                    return new LayoutXY(view);
                else if (orientation == RepeatOrientation.X)
                    return new LayoutX(view);
                else
                {
                    Debug.Assert(orientation == RepeatOrientation.YX);
                    return new LayoutYX(view);
                }
            }

            private sealed class LayoutXY : RepeatLayout
            {
                public LayoutXY(DataView view)
                    : base(view)
                {
                }

                public override int GetFlowCount()
                {
                    return (int)(Template.GridColumns.Sum(x => x.MeasuredLength) / _availableSize.Width);
                }
            }

            private sealed class LayoutY : RepeatLayout
            {
                public LayoutY(DataView view)
                    : base(view)
                {
                }
            }

            private sealed class LayoutYX : RepeatLayout
            {
                public LayoutYX(DataView view)
                    : base(view)
                {
                }

                public override Orientation GrowOrientation
                {
                    get { return Orientation.Horizontal; }
                }

                public override int GetFlowCount()
                {
                    return (int)(Template.GridRows.Sum(x => x.MeasuredLength) / _availableSize.Height);
                }
            }

            private sealed class LayoutX : RepeatLayout
            {
                public LayoutX(DataView view)
                    : base(view)
                {
                }

                public override Orientation GrowOrientation
                {
                    get { return Orientation.Horizontal; }
                }
            }

            public RepeatLayout(DataView view)
                : base(view)
            {
            }

            protected override void InitMeasure()
            {
                throw new NotImplementedException();
            }

            protected override double GetVariantAutoLength(GridTrack gridTrack, int repeatIndex)
            {
                throw new NotImplementedException();
            }

            protected override void SetVariantAutoLength(GridTrack gridTrack, int repeatIndex, double value)
            {
                throw new NotImplementedException();
            }
        }
    }
}
