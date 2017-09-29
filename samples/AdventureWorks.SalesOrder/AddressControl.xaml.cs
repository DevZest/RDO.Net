namespace AdventureWorks.SalesOrders
{
    /// <summary>
    /// Interaction logic for AddressControl.xaml
    /// </summary>
    public partial class AddressControl
    {
        public AddressControl()
        {
            InitializeComponent();
        }

        public string AddressLine1
        {
            get { return _addressLine1.Text; }
            set { _addressLine1.Text = value; }
        }

        public string AddressLine2
        {
            get { return _addressLine2.Text; }
            set { _addressLine2.Text = value; }
        }

        public string City
        {
            get { return _city.Text; }
            set { _city.Text = value; }
        }

        public string StateProvince
        {
            get { return _stateProvince.Text; }
            set { _stateProvince.Text = value; }
        }

        public string CountryRegion
        {
            get { return _countryRegion.Text; }
            set { _countryRegion.Text = value; }
        }

        public string PostalCode
        {
            get { return _postalCode.Text; }
            set { _postalCode.Text = value; }
        }
    }
}
