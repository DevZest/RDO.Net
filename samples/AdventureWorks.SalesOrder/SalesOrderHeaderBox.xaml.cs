using DevZest.Data.Views;
using System.Windows.Controls;

namespace AdventureWorks.SalesOrders
{
    /// <summary>
    /// Interaction logic for SalesOrderHeaderBox.xaml
    /// </summary>
    public partial class SalesOrderHeaderBox
    {
        public SalesOrderHeaderBox()
        {
            InitializeComponent();
        }

        public ForeignKeyBox Customer
        {
            get { return _customer; }
        }

        public ForeignKeyBox ShipTo
        {
            get { return _shipTo; }
        }

        public ForeignKeyBox BillTo
        {
            get { return _billTo; }
        }

        public DatePicker OrderDate
        {
            get { return _orderDate; }
        }

        public DatePicker ShipDate
        {
            get { return _shipDate; }
        }

        public DatePicker DueDate
        {
            get { return _dueDate; }
        }

        public TextBox SalesOrderNumber
        {
            get { return _salesOrderNumber; }
        }

        public TextBox PurchaseOrderNumber
        {
            get { return _purchaseOrderNumber; }
        }

        public TextBox AccountNumber
        {
            get { return _accountNumber; }
        }

        public ComboBox Status
        {
            get { return _status; }
        }

        public TextBox ShipMethod
        {
            get { return _shipMethod; }
        }

        public TextBox CreditCardApprovalCode
        {
            get { return _creditCardApprovalCode; }
        }

        public CheckBox OnlineOrderFlag
        {
            get { return _onlineOrderFlag; }
        }

        public TextBox Comment
        {
            get { return _comment; }
        }
    }
}
