using DevZest.Data;
using System;

namespace DevZest.Samples.AdventureWorksLT
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

        public static Action<CustomerBox, ColumnValueBag, Customer.Lookup> RefreshAction
        {
            get { return Refresh; }
        }

        private static void Refresh(CustomerBox v, ColumnValueBag valueBag, Customer.Lookup _)
        {
            v._companyName.Text = valueBag.GetValue(_.CompanyName);
            v._contactPerson.Text = Customer.GetContactPerson(valueBag.GetValue(_.LastName), valueBag.GetValue(_.FirstName), valueBag.GetValue(_.Title));
            v._phone.Text = valueBag.GetValue(_.Phone);
            v._email.Text = valueBag.GetValue(_.EmailAddress);
        }
    }
}
