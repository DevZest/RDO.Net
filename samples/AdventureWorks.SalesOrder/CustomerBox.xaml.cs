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

        public static Action<CustomerBox, Customer.Lookup, RowPresenter> RefreshAction
        {
            get { return Refresh; }
        }

        private static void Refresh(CustomerBox v, Customer.Lookup _, RowPresenter p)
        {
            v._companyName.Text = p.GetValue(_.CompanyName);
            v._contactPerson.Text = GetContactPerson(p.GetValue(_.LastName), p.GetValue(_.FirstName), p.GetValue(_.Title));
            v._phone.Text = p.GetValue(_.Phone);
            v._email.Text = p.GetValue(_.EmailAddress);
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
