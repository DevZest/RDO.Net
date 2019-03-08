using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    [Rule(nameof(ValidateLineCount), SourceColumns = new string[] { nameof(LineCount) })]
    [Computation(nameof(ComputeLineCount), ComputationMode.Aggregate)]
    [Computation(nameof(ComputeSubTotal), ComputationMode.Aggregate)]
    [InvisibleToDbDesigner]
    public class SalesOrder : SalesOrderHeader
    {
        static SalesOrder()
        {
            RegisterColumn((SalesOrder _) => _.LineCount);
            RegisterChildModel((SalesOrder _) => _.SalesOrderDetails, (SalesOrderDetail _) => _.FK_SalesOrderHeader, _ => _.CreateSalesOrderDetail());
        }

        public _Int32 LineCount { get; private set; }

        [_Rule]
        private DataValidationError ValidateLineCount(DataRow dataRow)
        {
            return LineCount[dataRow] > 0 ? null : new DataValidationError(UserMessages.Validation_SalesOrder_LineCount, LineCount);
        }

        [_Computation]
        private void ComputeLineCount()
        {
            LineCount.ComputedAs(SalesOrderDetails.SalesOrderDetailID.CountRows());
        }

        public SalesOrderDetail SalesOrderDetails { get; private set; }

        protected virtual SalesOrderDetail CreateSalesOrderDetail()
        {
            return new SalesOrderDetail();
        }

        [_Computation]
        private void ComputeSubTotal()
        {
            SubTotal.ComputedAs(SalesOrderDetails.LineTotal.Sum().IfNull(0), false);
        }
    }
}
