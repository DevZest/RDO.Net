using System.Windows.Controls;
using DevZest.Data.Presenters;
using System;
using System.Windows;

namespace DevZest.Samples.AdventureWorksLT
{
    public sealed class TotalAmtConditionalFormat : ScalarBindingBehavior<TextBlock>
    {
        public TotalAmtConditionalFormat(Func<decimal?> calculation)
        {
            _calculation = calculation;
        }

        private Func<decimal?> _calculation;

        protected override void Setup(TextBlock view, ScalarPresenter presenter)
        {
        }

        protected override void Refresh(TextBlock view, ScalarPresenter presenter)
        {
            view.FontWeight = _calculation() >= 10000 ? FontWeights.Bold : FontWeights.Normal;
        }

        protected override void Cleanup(TextBlock view, ScalarPresenter presenter)
        {
        }
    }
}
