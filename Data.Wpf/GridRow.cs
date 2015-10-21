using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public sealed class GridRow : GridDefinition
    {
        internal GridRow(GridView owner, int ordinal, GridLength height)
            : base(owner, ordinal, height)
        {
        }

        public GridLength Height
        {
            get { return Length; }
        }
    }
}
