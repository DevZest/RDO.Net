using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    [CustomValidator(nameof(VAL_LineCount))]
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

        [_CustomValidator]
        private CustomValidatorEntry VAL_LineCount
        {
            get
            {
                string Validate(DataRow dataRow)
                {
                    return LineCount[dataRow] > 0 ? null : UserMessages.Validation_SalesOrder_LineCount;
                }

                IColumns GetSourceColumns()
                {
                    return LineCount;
                }

                return new CustomValidatorEntry(Validate, GetSourceColumns);
            }
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
