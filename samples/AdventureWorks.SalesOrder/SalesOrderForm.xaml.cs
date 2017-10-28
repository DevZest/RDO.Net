using DevZest.Data;
using DevZest.Data.Views;
using DevZest.Samples.AdventureWorksLT;
using System;
using System.Windows;
using DevZest.Data.Presenters;
using System.Diagnostics;

namespace AdventureWorks.SalesOrders
{
    /// <summary>
    /// Interaction logic for SalesOrderForm.xaml
    /// </summary>
    public partial class SalesOrderForm : Window
    {
        public SalesOrderForm()
        {
            InitializeComponent();
        }

        private Presenter _presenter;

        private SalesOrderToEdit _
        {
            get { return _presenter?._; }
        }

        public void Show(DataSet<SalesOrderToEdit> data, Window ownerWindow, string windowTitle, Action action)
        {
            _presenter = new Presenter(this, _addressLookupPopup);
            _presenter.Show(_dataView, data);
            Owner = ownerWindow;
            Title = windowTitle;
            ShowDialog();
        }
    }
}
