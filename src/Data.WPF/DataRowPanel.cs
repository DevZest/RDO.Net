using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public class DataRowPanel : Panel
    {
        DataRowControl DataRowGrid
        {
            get { return TemplatedParent as DataRowControl; }
        }
    }
}
