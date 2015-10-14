using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public class CellsPanel : Panel
    {
        DataRowControl DataRowControl
        {
            get { return TemplatedParent as DataRowControl; }
        }
    }
}
