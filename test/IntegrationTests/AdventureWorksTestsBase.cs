using DevZest.Samples.AdventureWorksLT;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public abstract class AdventureWorksTestsBase
    {
        protected Db OpenDb()
        {
            return Db.Open(GetConnectionString());
        }

        protected Db OpenDb(StringBuilder log, LogCategory logCategory = LogCategory.CommandText)
        {
            return Db.Open(GetConnectionString(), db =>
            {
                db.SetLog(s => log.Append(s), logCategory);
            });
        }

        protected Task<Db> OpenDbAsync()
        {
            return Db.OpenAsync(GetConnectionString());
        }

        protected Task<Db> OpenDbAsync(StringBuilder log, LogCategory logCategory = LogCategory.CommandText)
        {
            return Db.OpenAsync(GetConnectionString(), db =>
            {
                db.SetLog(s => log.Append(s), logCategory);
            });
        }

        private static string GetConnectionString()
        {
            string mdfFilename = "AdventureWorksLT.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string attachDbFilename = Path.Combine(outputFolder, mdfFilename);
            return string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
        }

        protected DataSet<SalesOrder.Edit> GetSalesOrder(int salesOrderID)
        {
            using (var db = OpenDb())
            {
                var salesOrders = db.CreateQuery((DbQueryBuilder builder, SalesOrder.Edit _) =>
                {
                    var ext = _.GetExtension<SalesOrder.Ext>();
                    Debug.Assert(ext != null);
                    SalesOrder o;
                    Customer c;
                    Address shipTo, billTo;
                    builder.From(db.SalesOrders, out o)
                        .InnerJoin(db.Customers, o.Customer, out c)
                        .InnerJoin(db.Addresses, o.ShipToAddress, out shipTo)
                        .InnerJoin(db.Addresses, o.BillToAddress, out billTo)
                        .AutoSelect()
                        .AutoSelect(shipTo, ext.ShipToAddress)
                        .AutoSelect(billTo, ext.BillToAddress)
                        .Where(o.SalesOrderID == salesOrderID);
                });

                salesOrders.CreateChild(_ => _.SalesOrderDetails, (DbQueryBuilder builder, SalesOrderDetail _) =>
                {
                    Debug.Assert(_.GetExtension<SalesOrderDetail.Ext>() != null);
                    SalesOrderDetail d;
                    Product p;
                    builder.From(db.SalesOrderDetails, out d)
                        .InnerJoin(db.Products, d.Product, out p)
                        .AutoSelect();
                });

                return salesOrders.ToDataSet();
            }
        }
    }
}
