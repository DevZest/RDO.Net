using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    [ModelExtender(typeof(Ext))]
    public class SalesOrderDetailInfo : SalesOrderDetail
    {
        public class Ext : ModelExtender
        {
            static Ext()
            {
                RegisterChildExtender((Ext _) => _.Product);
            }

            public Product.Lookup Product { get; private set; }
        }
    }
}
