using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AdventureWorks.SalesOrders
{
    /// <summary>
    /// Interaction logic for CustomerBox.xaml
    /// </summary>
    public partial class CustomerBox
    {
        public CustomerBox()
        {
            InitializeComponent();
        }

        public string CompanyName
        {
            get { return _companyName.Text; }
            set { _companyName.Text = value; }
        }

        public string ContactPerson
        {
            get { return _contactPerson.Text; }
            set { _contactPerson.Text = value; }
        }

        public string Phone
        {
            get { return _phone.Text; }
            set { _phone.Text = value; }
        }

        public string Email
        {
            get { return _email.Text; }
            set { _email.Text = value; }
        }
    }
}
