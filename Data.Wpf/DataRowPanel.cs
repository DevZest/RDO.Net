using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public class DataRowPanel : Panel
    {
        DataRowGrid DataRowControl
        {
            get { return TemplatedParent as DataRowGrid; }
        }
    }
}
