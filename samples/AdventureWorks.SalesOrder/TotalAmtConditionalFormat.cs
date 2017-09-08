using DevZest.Data;
using DevZest.Data.Presenters.Plugins;
using System.Windows.Controls;
using DevZest.Data.Presenters;
using System;
using System.Windows;
using DevZest.Samples.AdventureWorksLT;
using System.Linq;

namespace AdventureWorks.SalesOrders
{
    public sealed class TotalAmtConditionalFormat : ScalarBindingPlugin<TextBlock>
    {
        public TotalAmtConditionalFormat(SalesOrder salesOrder)
        {
            _ = salesOrder;
        }

        private SalesOrder _;

        protected override void Setup(TextBlock view, ScalarPresenter presenter)
        {
        }

        protected override void Refresh(TextBlock view, ScalarPresenter presenter)
        {
            view.FontWeight = presenter.DataPresenter.Rows.Sum(x => x.GetValue(_.TotalDue)) >= 10000 ? FontWeights.Bold : FontWeights.Normal;
        }

        protected override void Cleanup(TextBlock view, ScalarPresenter presenter)
        {
        }
    }
}
