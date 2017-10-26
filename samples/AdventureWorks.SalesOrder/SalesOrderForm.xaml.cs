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
    public partial class SalesOrderForm : Window, ForeignKeyBox.ILookupService
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

        DataPresenter IService.DataPresenter
        {
            get { return _presenter; }
        }

        void IService.Initialize(DataPresenter dataPresenter)
        {
            Debug.Assert(dataPresenter == _presenter);
        }

        bool ForeignKeyBox.ILookupService.CanLookup(KeyBase foreignKey)
        {
            if (foreignKey == _.Customer)
                return true;
            else if (foreignKey == _.BillToAddress)
                return true;
            else if (foreignKey == _.ShipToAddress)
                return true;
            else
                return false;
        }

        private RowPresenter CurrentRow
        {
            get { return _presenter.CurrentRow; }
        }

        ColumnValueBag ForeignKeyBox.ILookupService.Lookup(KeyBase foreignKey)
        {
            if (foreignKey == _.Customer)
            {
                var dialogWindow = new CustomerLookupWindow();
                dialogWindow.Show(this, _presenter.CurrentRow.GetValue(_.CustomerID), _.Customer, _.GetExtension<SalesOrderToEdit.Ext>().Customer);
                return dialogWindow.Result;
            }
            else if (foreignKey == _.ShipToAddress)
            {
                var dialogWindow = new AddressLookupWindow();
                dialogWindow.Show(this, CurrentRow.GetValue(_.ShipToAddressID), CurrentRow.GetValue(_.CustomerID).Value, _.ShipToAddress, _.GetExtension<SalesOrderToEdit.Ext>().ShipToAddress);
                return dialogWindow.Result;
            }
            else if (foreignKey == _.BillToAddress)
            {
                var dialogWindow = new AddressLookupWindow();
                dialogWindow.Show(this, CurrentRow.GetValue(_.BillToAddressID), CurrentRow.GetValue(_.CustomerID).Value, _.BillToAddress, _.GetExtension<SalesOrderToEdit.Ext>().BillToAddress);
                return dialogWindow.Result;
            }
            else
                throw new NotSupportedException();
        }

        public void Show(DataSet<SalesOrderToEdit> data, Window ownerWindow, string windowTitle, Action action)
        {
            _presenter = new Presenter(this);
            _presenter.Show(_dataView, data);
            Owner = ownerWindow;
            Title = windowTitle;
            ShowDialog();
        }
    }
}
