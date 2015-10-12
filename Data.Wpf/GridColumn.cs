using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public sealed class GridColumn : GridDefinition
    {
        internal GridColumn(DataSetControl dataSetControl, int ordinal, DataGridLength width)
            : base(dataSetControl, ordinal, width)
        {
        }

        public DataGridLength Width
        {
            get { return Length; }
        }
    }
}
