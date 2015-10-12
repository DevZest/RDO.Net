using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public sealed class GridRow : GridDefinition
    {
        public GridRow(DataSetControl dataSetControl, int ordinal, DataGridLength height)
            : base(dataSetControl, ordinal, height)
        {
        }

        public DataGridLength Height
        {
            get { return Length; }
        }
    }
}
