using DevZest.Data;
using DevZest.Samples.AdventureWorksLT;
using System.Threading;
using System.Threading.Tasks;

namespace AdventureWorks.SalesOrders
{
    static class Data
    {
        public static Task<DataSet<SalesOrder>> GetSalesOrders(string filterText, CancellationToken ct)
        {
            using (var db = Db.Open(App.ConnectionString))
            {
                if (string.IsNullOrEmpty(filterText))
                    return db.SalesOrders.ToDataSetAsync(ct);
                else
                    return db.SalesOrders.Where(_ => _.SalesOrderNumber.Contains(filterText) | _.PurchaseOrderNumber.Contains(filterText)).ToDataSetAsync(ct);
            }
        }
    }
}
