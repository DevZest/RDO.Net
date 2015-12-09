using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public class DataRowPanel : Panel
    {
        DataRowView DataRowGrid
        {
            get { return TemplatedParent as DataRowView; }
        }
    }
}
