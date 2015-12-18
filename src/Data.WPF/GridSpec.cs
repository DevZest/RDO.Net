using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class GridSpec
    {
        internal GridSpec(GridTemplate owner, int ordinal, GridLengthParser.Result result)
        {
            Owner = owner;
            Ordinal = ordinal;
            Length = result.Length;
            MinLength = result.MinLength;
            MaxLength = result.MaxLength;
        }

        internal GridTemplate Owner { get; private set; }

        internal int Ordinal { get; private set; }

        public GridLength Length { get; private set; }

        public double MinLength { get; private set; }

        public double MaxLength { get; private set; }

        public double ActualLength { get; internal set; }
    }
}
