using DevZest.Data.Windows.Primitives;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class GridRow : GridDefinition
    {
        internal GridRow(GridTemplate owner, int ordinal, GridLengthParser.Result result)
            : base(owner, ordinal, result)
        {
        }

        public GridLength Height
        {
            get { return Length; }
        }

        public double MinHeight
        {
            get { return MinLength; }
        }

        public double MaxHeight
        {
            get { return MaxLength; }
        }

        public double ActualHeight
        {
            get { return ActualLength; }
            internal set { ActualLength = value; }
        }
    }
}