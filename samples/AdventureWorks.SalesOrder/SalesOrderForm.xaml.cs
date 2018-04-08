using DevZest.Data;
using DevZest.Samples.AdventureWorksLT;
using System;
using System.Windows;
using System.Windows.Input;

namespace AdventureWorks.SalesOrders
{
    /// <summary>
    /// Interaction logic for SalesOrderForm.xaml
    /// </summary>
    public partial class SalesOrderForm : Window
    {
        public static class Commands
        {
            public static readonly RoutedCommand Submit = new RoutedCommand(nameof(Submit), typeof(SalesOrderForm));
        }

        public SalesOrderForm()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(Commands.Submit, ExecSubmit, CanExecSubmit));
        }

        private void ExecSubmit(object sender, ExecutedRoutedEventArgs e)
        {
            if (_presenter.IsEditing)
                _presenter.CurrentRow.EndEdit();
            if (CurrentRowDetailPresenter.IsEditing)
                CurrentRowDetailPresenter.CurrentRow.EndEdit();

            if (App.Execute((ct) => Data.UpdateSalesOrder(_presenter.DataSet, ct), this, "Saving..."))
            {
                DialogResult = true;
                Close();
            }
        }

        private void CanExecSubmit(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _presenter.CanSubmitInput && CurrentRowDetailPresenter.CanSubmitInput;
        }

        private Presenter _presenter;

        private DataSet<SalesOrderInfo> DataSet
        {
            get { return _presenter.DataSet; }
        }

        private DetailPresenter CurrentRowDetailPresenter
        {
            get { return _presenter.CurrentRowDetailPresenter; }
        }

        public void Show(DataSet<SalesOrderInfo> data, Window ownerWindow, string windowTitle, Action action)
        {
            _presenter = new Presenter(this, _addressLookupPopup);
            _presenter.Show(_dataView, data);
            Owner = ownerWindow;
            Title = windowTitle;
            ShowDialog();
        }
    }
}
