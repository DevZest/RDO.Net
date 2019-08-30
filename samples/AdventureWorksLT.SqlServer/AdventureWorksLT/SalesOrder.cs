using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    [InvisibleToDbDesigner]
    public class SalesOrder : SalesOrderBase<SalesOrderDetail>
    {
    }
}
