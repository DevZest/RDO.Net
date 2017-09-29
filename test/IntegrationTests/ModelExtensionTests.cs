using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class ModelExtensionTests : AdventureWorksTestsBase
    {
        [TestMethod]
        public void ModelExtension_sales_order_detail()
        {
            using (var db = OpenDb())
            {
                var salesOrderDetails = db.CreateQuery((DbQueryBuilder builder, SalesOrderDetail _) =>
                {
                    SalesOrderDetail d;
                    Product p;
                    builder.From(db.SalesOrderDetails, out d)
                        .InnerJoin(db.Products, d.Product, product => product.PrimaryKey, out p)
                        .AutoSelect()
                        .Where(d.SalesOrderID == _Int32.Const(71774));
                }, null, _ => _.SetExtension<SalesOrderDetail.Ext>());

                var json = salesOrderDetails.ToDataSet().ToJsonString(true);
                var expectedJson = Strings.ExpectedJSON_SalesOrderDetail_71774_with_ext.Trim();
                Assert.AreEqual(expectedJson, json);

                var dataSet = DataSet<SalesOrderDetail>.ParseJson(json, _ => _.SetExtension<SalesOrderDetail.Ext>());
                Assert.AreEqual(expectedJson, dataSet.ToJsonString(true));
            }
        }
    }
}
