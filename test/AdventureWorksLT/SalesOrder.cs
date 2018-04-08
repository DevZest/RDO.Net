using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    public class SalesOrder : SalesOrderHeader
    {
        static SalesOrder()
        {
            RegisterColumn((SalesOrder _) => _.LineCount);
            RegisterChildModel((SalesOrder x) => x.SalesOrderDetails, (SalesOrderDetail x) => x.SalesOrderHeader);
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

        public virtual SalesOrderDetail SalesOrderDetails { get; private set; }

        [Computation(IsAggregate = true)]
        private void ComputeSubTotal()
        {
            SubTotal.ComputedAs(SalesOrderDetails.LineTotal.Sum().IfNull(0), false);
        }
    }
}
