using DevZest.Data;
using DevZest.Samples.AdventureWorksLT;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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

            if (!_presenter.SubmitInput())
                return;

            if (App.Execute(_presenter.SaveToDb, this, "Saving...", out var salesOrderId))
            {
                Close();
                _action?.Invoke(salesOrderId);
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

        private Action<int?> _action;
        public void Show(DataSet<SalesOrderInfo> data, Window ownerWindow, Action<int?> action)
        {
            Debug.Assert(data.Count == 1);
            _presenter = new Presenter(this, _addressLookupPopup);
            _presenter.Show(_dataView, data);
            Owner = ownerWindow;
            Title = _presenter.IsNew ? "New Sales Order" : string.Format("Sales Order: {0}", _presenter.SalesOrderId);
            _action = action;
            ShowDialog();
        }
    }
}
