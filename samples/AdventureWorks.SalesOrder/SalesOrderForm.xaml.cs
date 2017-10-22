using DevZest.Data;
using DevZest.Samples.AdventureWorksLT;
using System;
using System.Windows;

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

        public void Show(DataSet<SalesOrderToEdit> data, Window ownerWindow, string windowTitle, Action action)
        {
            _presenter = new Presenter();
            _presenter.Show(_dataView, data);
            Owner = ownerWindow;
            Title = windowTitle;
            ShowDialog();
        }
    }
}
