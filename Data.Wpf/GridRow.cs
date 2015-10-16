using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public sealed class GridRow : GridDefinition
    {
        public GridRow(DataSetControl dataSetControl, int ordinal, GridLength height)
            : base(dataSetControl, ordinal, height)
        {
        }

        public GridLength Height
        {
            get { return Length; }
        }
    }
}
