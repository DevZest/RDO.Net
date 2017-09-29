using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AdventureWorks.SalesOrders
{
    /// <summary>
    /// Interaction logic for CustomerControl.xaml
    /// </summary>
    public partial class CustomerControl
    {
        public static readonly RoutedUICommand EditCommand = new RoutedUICommand();

        public static readonly DependencyProperty CustomerIDProperty = DependencyProperty.Register(nameof(CustomerID), typeof(int?), typeof(CustomerControl),
            new FrameworkPropertyMetadata(null, OnCustomerIDChanged));

        private static void OnCustomerIDChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomerControl)d).OnCustomerIDChanged();
        }

        public CustomerControl()
        {
            InitializeComponent();
        }

        private void OnCustomerIDChanged()
        {
            var customerID = CustomerID;
            _emptyView.Visibility = customerID.HasValue ? Visibility.Collapsed : Visibility.Visible;
            _nonEmptyView.Visibility = !customerID.HasValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public int? CustomerID
        {
            get { return (int?)GetValue(CustomerIDProperty); }
            set { SetValue(CustomerIDProperty, value); }
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
