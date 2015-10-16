using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public sealed class GridColumn : GridDefinition
    {
        internal GridColumn(DataSetControl dataSetControl, int ordinal, GridLength width)
            : base(dataSetControl, ordinal, width)
        {
        }

        public GridLength Width
        {
            get { return Length; }
        }
    }
}
