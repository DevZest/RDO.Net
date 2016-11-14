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
            var orderBy = new ColumnSort[]
            {
                salesOrder.CustomerID.Asc(),
                salesOrder.SalesOrderID.Desc()
            };

            var json = orderBy.ToJson(true);
            Assert.AreEqual(Json.OrderByJson_ToJson_Parse, json);

            var _ = new SalesOrder();
            var fromJsonOrderBy = OrderByJson.ParseJson(_, json);
            Assert.AreEqual(2, fromJsonOrderBy.Length);
            Assert.AreEqual(_.CustomerID, fromJsonOrderBy[0].Column);
            Assert.AreEqual(SortDirection.Ascending, fromJsonOrderBy[0].Direction);
            Assert.AreEqual(_.SalesOrderID, fromJsonOrderBy[1].Column);
            Assert.AreEqual(SortDirection.Descending, fromJsonOrderBy[1].Direction);
        }
    }
}
