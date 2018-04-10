using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    public class SalesOrder : SalesOrderHeader
    {
        static SalesOrder()
        {
            RegisterColumn((SalesOrder _) => _.LineCount);
            RegisterChildModel((SalesOrder _) => _.SalesOrderDetails, (SalesOrderDetail _) => _.SalesOrderHeader, null, _ => _.CreateSalesOrderDetail());
        }

        public _Int32 LineCount { get; private set; }

        [ModelValidator]
        private DataValidationError ValidateLineCount(DataRow dataRow)
        {
            return LineCount[dataRow] > 0 ? null : new DataValidationError(UserMessages.Validation_SalesOrder_LineCount, LineCount);
        }

        [Computation(IsAggregate = true)]
        private void ComputeLineCount()
        {
            LineCount.ComputedAs(SalesOrderDetails.SalesOrderDetailID.CountRows());
        }

        public SalesOrderDetail SalesOrderDetails { get; private set; }

        protected virtual SalesOrderDetail CreateSalesOrderDetail()
        {
            return new SalesOrderDetail();
        }

        [Computation(IsAggregate = true)]
        private void ComputeSubTotal()
        {
            SubTotal.ComputedAs(SalesOrderDetails.LineTotal.Sum().IfNull(0), false);
        }
    }
}
