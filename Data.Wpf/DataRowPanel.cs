using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public class DataRowPanel : Panel
    {
        DataRowGrid DataRowControl
        {
            get { return TemplatedParent as DataRowGrid; }
        }
    }
}
