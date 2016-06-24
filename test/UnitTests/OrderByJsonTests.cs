using DevZest.Data.Resources;
using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class OrderByJsonTests
    {
        [TestMethod]
        public void OrderByJson_ToJson_Parse()
        {
            var salesOrder = new SalesOrder();
            var orderByList = new OrderBy[]
            {
                salesOrder.CustomerID.Asc(),
                salesOrder.SalesOrderID.Desc()
            };

            var json = orderByList.ToJson(true);
            Assert.AreEqual(Json.OrderByJson_ToJson_Parse, json);

            var _ = new SalesOrder();
            var fromJsonList = OrderByList.ParseJson(_, json);
            Assert.AreEqual(2, fromJsonList.Count);
            Assert.AreEqual(_.CustomerID, fromJsonList[0].Column);
            Assert.AreEqual(SortDirection.Ascending, fromJsonList[0].Direction);
            Assert.AreEqual(_.SalesOrderID, fromJsonList[1].Column);
            Assert.AreEqual(SortDirection.Descending, fromJsonList[1].Direction);
        }
    }
}
