using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class GridColumn : GridDefinition
    {
        internal GridColumn(DataSetView owner, int ordinal, GridLength width)
            : base(owner, ordinal, width)
        {
        }

        public GridLength Width
        {
            get { return Length; }
        }
    }
}
