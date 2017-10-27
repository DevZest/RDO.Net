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

        void ForeignKeyBox.ILookupService.BeginLookup(ForeignKeyBox foreignKeyBox)
        {
            if (foreignKeyBox.ForeignKey == _.Customer)
            {
                var dialogWindow = new CustomerLookupWindow();
                dialogWindow.Show(this, foreignKeyBox, _presenter.CurrentRow.GetValue(_.CustomerID), null, null);
            }
            else if (foreignKeyBox.ForeignKey == _.ShipToAddress || foreignKeyBox.ForeignKey == _.BillToAddress)
                BeginLookupAddress(foreignKeyBox);
            else
                throw new NotSupportedException();
        }

        private void BeginLookupAddress(ForeignKeyBox foreignKeyBox)
        {
            var foreignKey = (Address.Key)foreignKeyBox.ForeignKey;
            if (_addressLookupPopup.Key == foreignKey)
                _addressLookupPopup.IsOpen = false;
            else
                _addressLookupPopup.Show(foreignKeyBox, CurrentRow.GetValue(foreignKey.AddressID), CurrentRow.GetValue(_.CustomerID).Value);
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
