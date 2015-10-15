using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public class DataRowPanel : Panel
    {
        DataRowControl DataRowControl
        {
            get { return TemplatedParent as DataRowControl; }
        }
    }
}
