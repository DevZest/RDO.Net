using System.Windows.Controls;

namespace AdventureWorks.SalesOrders
{
    /// <summary>
    /// Interaction logic for SalesOrderFooterBox.xaml
    /// </summary>
    public partial class SalesOrderFooterBox
    {
        public SalesOrderFooterBox()
        {
            InitializeComponent();
        }

        public TextBlock SubTotal
        {
            get { return _subTotal; }
        }

        public TextBox Freight
        {
            get { return _freight; }
        }

        public TextBox TaxAmt
        {
            get { return _taxAmt; }
        }

        public TextBlock TotalDue
        {
            get { return _totalDue; }
        }
    }
}
