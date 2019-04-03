using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data
{
    [TestClass]
    public class DataSetTests : AdventureWorksTestsBase
    {
        [TestMethod]
        public async Task DataSet_recursive_fill_and_serialize_async()
        {
            var log = new StringBuilder();
            using (var db = await OpenDbAsync(log))
            {
                var productCategories = db.ProductCategory.Where(x => x.ParentProductCategoryID.IsNull()).OrderBy(x => x.ProductCategoryID);
                var children = productCategories;
                while (children != null)
                    children = await children.CreateChildAsync(x => x.SubCategories, db.ProductCategory.OrderBy(x => x.ProductCategoryID));

                var result = await productCategories.ToDataSetAsync();
                var childModel = result._.SubCategories;
                Assert.AreEqual(4, result.Count);
                Assert.AreEqual(3, result[0].GetChildDataSet(childModel).Count);
                Assert.AreEqual(14, result[1].GetChildDataSet(childModel).Count);
                Assert.AreEqual(8, result[2].GetChildDataSet(childModel).Count);
                Assert.AreEqual(12, result[3].GetChildDataSet(childModel).Count);

                Assert.AreEqual(Strings.ExpectedJSON_ProductCategories.Trim(), result.ToString().Trim());
            }
        }

        private sealed class SalesOrderEx : SalesOrder
        {
            public static Mounter<_Int32> SalesOrderIDMounter
            {
                get { return SalesOrder._SalesOrderID; }
            }

            public static Mounter<_String> SalesOrderNumberMounter
            {
                get { return _SalesOrderNumber; }
            }
        }

        [TestMethod]
        public void DataSet_DynamicModel_serialize_deserialize()
        {
            var log = new StringBuilder();
            using (var db = OpenDbAsync(log).Result)
            {
                var json = db.CreateQuery((DbQueryBuilder builder, Adhoc adhoc) =>
                {
                    builder.From(db.SalesOrderHeader, out var o)
                        .Select(o.SalesOrderID, adhoc)
                        .Select(o.SalesOrderNumber, adhoc)
                        .Where(o.SalesOrderID == 71774 | o.SalesOrderID == 71776)
                        .OrderBy(o.SalesOrderID);
                }).ToDataSetAsync().Result.ToString();

                Assert.AreEqual(Strings.ExpectedJSON_SalesOrderDynamicModel, json.Trim());

                var dataSet = DataSet<Adhoc>.ParseJson(_ =>
                {
                    _.AddColumn(SalesOrderEx.SalesOrderIDMounter);
                    _.AddColumn(SalesOrderEx.SalesOrderNumberMounter);
                }, json);

                Assert.AreEqual(json.Trim(), dataSet.ToString().Trim());
            }
        }

        [TestMethod]
        public async Task DataSet_CreateChildAsync()
        {
            var log = new StringBuilder();
            using (var db = await OpenDbAsync(log))
            {
                var salesOrders = await db.SalesOrderHeader.ToDbQuery<SalesOrder>().Where(x => x.SalesOrderID == 71774).ToDataSetAsync();
                Assert.IsTrue(salesOrders.Count == 1);
                await salesOrders.FillAsync(0, x => x.SalesOrderDetails, db.SalesOrderDetail);
                Assert.AreEqual(Strings.ExpectedJSON_SalesOrder_71774, salesOrders.ToString());
            }
        }
    }
}
