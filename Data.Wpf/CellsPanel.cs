using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public class CellsPanel : Panel
    {
        RowControl RowControl
        {
            get { return TemplatedParent as RowControl; }
        }
    }
}
