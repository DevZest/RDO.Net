using DevZest.Data;
using DevZest.Data.Presenters;
using DevZest.Samples.AdventureWorksLT;
using System;
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

        public static Action<CustomerBox, Customer.Lookup, ColumnValueBag> RefreshAction
        {
            get { return Refresh; }
        }

        private static void Refresh(CustomerBox v, Customer.Lookup _, ColumnValueBag valueBag)
        {
            v._companyName.Text = valueBag.GetValue(_.CompanyName);
            v._contactPerson.Text = GetContactPerson(valueBag.GetValue(_.LastName), valueBag.GetValue(_.FirstName), valueBag.GetValue(_.Title));
            v._phone.Text = valueBag.GetValue(_.Phone);
            v._email.Text = valueBag.GetValue(_.EmailAddress);
        }

        private static string GetContactPerson(string lastName, string firstName, string title)
        {
            string result = string.IsNullOrEmpty(lastName) ? string.Empty : lastName.ToUpper();
            if (!string.IsNullOrEmpty(firstName))
            {
                if (result.Length > 0)
                    result += ", ";
                result += firstName;
            }
            if (!string.IsNullOrEmpty(title))
            {
                result += " (";
                result += title;
                result += ")";
            }

            return result;
        }
    }
}
