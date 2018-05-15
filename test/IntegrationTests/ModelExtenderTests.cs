using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class ModelExtenderTests : AdventureWorksTestsBase
    {
        [TestMethod]
        public void ModelExtender_sales_order_detail()
        {
            using (var db = OpenDbAsync().Result)
            {
                var salesOrderDetails = db.CreateQuery(_ => _.SetExtender<SalesOrderDetail.ForeignKeyLookup.Ext>(),
                    (DbQueryBuilder builder, SalesOrderDetail _) =>
                    {
                        SalesOrderDetail d;
                        Product p;
                        builder.From(db.SalesOrderDetails, out d)
                            .InnerJoin(db.Products, d.Product, out p)
                            .AutoSelect()
                            .Where(d.SalesOrderID == _Int32.Const(71774));
                    });

                var json = salesOrderDetails.ToDataSetAsync().Result.ToJsonString(true);
                var expectedJson = Strings.ExpectedJSON_SalesOrderDetail_71774_with_ext.Trim();
                Assert.AreEqual(expectedJson, json);

                var dataSet = DataSet<SalesOrderDetail>.ParseJson(_ => _.SetExtender<SalesOrderDetail.ForeignKeyLookup.Ext>(), json);
                Assert.AreEqual(expectedJson, dataSet.ToJsonString(true));
            }
        }

        [TestMethod]
        public void ModelExtender_sales_order()
        {
            using (var db = OpenDbAsync().Result)
            {
                var salesOrders = db.CreateQuery(_ => _.SetExtender<SalesOrderHeader.ForeignKeyLookup.Ext>(),
                    (DbQueryBuilder builder, SalesOrder _) =>
                    {
                        var ext = _.GetExtender<SalesOrderHeader.ForeignKeyLookup.Ext>();
                        builder.From(db.SalesOrderHeaders, out var o)
                            .InnerJoin(db.Customers, o.FK_Customer, out var c)
                            .InnerJoin(db.Addresses, o.FK_ShipToAddress, out var shipTo)
                            .InnerJoin(db.Addresses, o.FK_BillToAddress, out var billTo)
                            .AutoSelect()
                            .AutoSelect(shipTo, ext.ShipToAddress)
                            .AutoSelect(billTo, ext.BillToAddress)
                            .Where(o.SalesOrderID == _Int32.Const(71774));
                    });

                var json = salesOrders.ToDataSetAsync().Result.ToJsonString(true);
                var expectedJson = Strings.ExpectedJSON_SalesOrder_71774_with_ext;
                Assert.AreEqual(expectedJson, json);

                var dataSet = DataSet<SalesOrder>.ParseJson(_ => _.SetExtender<SalesOrderHeader.ForeignKeyLookup.Ext>(), json);
                Assert.AreEqual(expectedJson, dataSet.ToJsonString(true));
            }
        }

        [TestMethod]
        public void ModelExtender_sales_order_with_details()
        {
            var json = GetSalesOrderInfo(71774).ToJsonString(true);
            var expectedJson = Strings.ExpectedJSON_SalesOrderInfo_71774;
            Assert.AreEqual(expectedJson, json);

            var dataSet = DataSet<SalesOrderInfo>.ParseJson(json);
            Assert.AreEqual(expectedJson, dataSet.ToJsonString(true));
        }
    }
}
