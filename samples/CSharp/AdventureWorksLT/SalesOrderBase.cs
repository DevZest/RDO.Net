using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    [CustomValidator(nameof(VAL_LineCount))]
    [Computation(nameof(ComputeLineCount), ComputationMode.Aggregate)]
    [Computation(nameof(ComputeSubTotal), ComputationMode.Aggregate)]
    [InvisibleToDbDesigner]
    public abstract class SalesOrderBase : SalesOrderHeader
    {
        static SalesOrderBase()
        {
            RegisterColumn((SalesOrderBase _) => _.LineCount);
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

        protected abstract SalesOrderDetail GetSalesOrderDetails();

        [_Computation]
        private void ComputeLineCount()
        {
            LineCount.ComputedAs(GetSalesOrderDetails().SalesOrderDetailID.CountRows());
        }

        [_Computation]
        private void ComputeSubTotal()
        {
            SubTotal.ComputedAs(GetSalesOrderDetails().LineTotal.Sum().IfNull(0), false);
        }
    }
}
