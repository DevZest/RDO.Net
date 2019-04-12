using DevZest.Data;
using System.Windows.Controls;
using DevZest.Data.Presenters;
using System.Windows;

namespace DevZest.Samples.AdventureWorksLT
{
    public sealed class TotalDueConditionalFormat : RowBindingBehavior<TextBlock>
    {
        public TotalDueConditionalFormat(_Decimal totalAmt)
        {
            _totalAmt = totalAmt;
        }

        private _Decimal _totalAmt;

        protected override void Setup(TextBlock view, RowPresenter presenter)
        {
        }

        protected override void Refresh(TextBlock view, RowPresenter presenter)
        {
            view.FontWeight = presenter.GetValue(_totalAmt) >= 10000 ? FontWeights.Bold : FontWeights.Normal;
        }

        protected override void Cleanup(TextBlock view, RowPresenter presenter)
        {
        }
    }
}
