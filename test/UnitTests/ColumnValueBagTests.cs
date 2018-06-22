using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevZest.Samples.AdventureWorksLT;

namespace DevZest.Data
{
    [TestClass]
    public class ColumnValueBagTests
    {
        [TestMethod]
        public void ColumnValueBag_SetValue()
        {
            var salesOrders = DataSet<SalesOrderInfo>.New();
            var _ = salesOrders._;
            var customer = _.LK_Customer;

            var valueBag = new ColumnValueBag();
            valueBag.SetValue(_.CustomerID, 2);
            valueBag.SetValue(customer.FirstName, "FirstName");
            Assert.AreEqual(2, valueBag.GetValue(_.CustomerID));
            Assert.AreEqual(2, valueBag[_.CustomerID]);
            Assert.AreEqual("FirstName", valueBag.GetValue(customer.FirstName));
            Assert.AreEqual("FirstName", valueBag[customer.FirstName]);
        }

        [TestMethod]
        public void ColumnValueBag_SetValue_with_DataRow()
        {
            var salesOrders = DataSet<SalesOrderInfo>.New();
            var _ = salesOrders._;
            var customer = _.LK_Customer;
            salesOrders.Add(new DataRow(), (dataRow) =>
            {
                _.CustomerID[dataRow] = 5;
                customer.Title[dataRow] = "Mr.";
                customer.FirstName[dataRow] = "John";
                customer.MiddleName[dataRow] = "K";
                customer.LastName[dataRow] = "Smith";
                customer.CompanyName[dataRow] = "Good Toys";
                customer.EmailAddress[dataRow] = @"john.smith@goodtoys.com";
                customer.Phone[dataRow] = "555-123-4567";
            });

            {
                var valueBag = new ColumnValueBag();
                valueBag.SetValue(_.CustomerID, salesOrders[0]);
                valueBag.SetValue(customer.FirstName, salesOrders[0]);
                Assert.AreEqual(5, valueBag.GetValue(_.CustomerID));
                Assert.AreEqual(5, valueBag[_.CustomerID]);
                Assert.AreEqual("John", valueBag.GetValue(customer.FirstName));
                Assert.AreEqual("John", valueBag[customer.FirstName]);
            }

            {
                var salesOrder = DataSet<SalesOrderInfo>.New().EnsureInitialized()._;
                var customerKey = salesOrder.FK_Customer;
                var customerLookup = salesOrder.LK_Customer;
                var valueBag = new ColumnValueBag();
                valueBag.AutoSelect(customerKey, salesOrders[0]);
                valueBag.AutoSelect(customerLookup, salesOrders[0]);
                Assert.AreEqual(5, valueBag.GetValue(customerKey.CustomerID));
                Assert.AreEqual(5, valueBag[customerKey.CustomerID]);
                Assert.AreEqual("Mr.", valueBag.GetValue(customerLookup.Title));
                Assert.AreEqual("John", valueBag.GetValue(customerLookup.FirstName));
                Assert.AreEqual("K", valueBag.GetValue(customerLookup.MiddleName));
                Assert.AreEqual("Smith", valueBag.GetValue(customerLookup.LastName));
                Assert.AreEqual("Good Toys", valueBag.GetValue(customerLookup.CompanyName));
                Assert.AreEqual(@"john.smith@goodtoys.com", valueBag.GetValue(customerLookup.EmailAddress));
                Assert.AreEqual("555-123-4567", valueBag.GetValue(customerLookup.Phone));
                Assert.AreEqual("Mr.", valueBag[customerLookup.Title]);
                Assert.AreEqual("John", valueBag[customerLookup.FirstName]);
                Assert.AreEqual("K", valueBag[customerLookup.MiddleName]);
                Assert.AreEqual("Smith", valueBag[customerLookup.LastName]);
                Assert.AreEqual("Good Toys", valueBag[customerLookup.CompanyName]);
                Assert.AreEqual(@"john.smith@goodtoys.com", valueBag[customerLookup.EmailAddress]);
                Assert.AreEqual("555-123-4567", valueBag[customerLookup.Phone]);
            }
        }

        [TestMethod]
        public void ColumnValueBag_Clone()
        {
            var salesOrders = DataSet<SalesOrderInfo>.New();
            var _ = salesOrders._;
            var customer = _.LK_Customer;

            var valueBag = new ColumnValueBag();
            valueBag.SetValue(_.CustomerID, 2);
            valueBag.SetValue(customer.FirstName, "FirstName");

            var clone = valueBag.Clone();
            Assert.AreEqual(2, clone.GetValue(_.CustomerID));
            Assert.AreEqual(2, clone[_.CustomerID]);
            Assert.AreEqual("FirstName", clone.GetValue(customer.FirstName));
            Assert.AreEqual("FirstName", clone[customer.FirstName]);
        }

        [TestMethod]
        public void ColumnValueBag_ClearValues()
        {
            var salesOrders = DataSet<SalesOrderInfo>.New();
            var _ = salesOrders._;
            var customer = _.LK_Customer;

            var valueBag = new ColumnValueBag();
            valueBag.SetValue(_.CustomerID, 2);
            valueBag.SetValue(customer.FirstName, "FirstName");
            valueBag.ResetValues();

            Assert.AreEqual(new int?(), valueBag.GetValue(_.CustomerID));
            Assert.AreEqual(new int?(), valueBag[_.CustomerID]);
            Assert.AreEqual(null, valueBag.GetValue(customer.FirstName));
            Assert.AreEqual(null, valueBag[customer.FirstName]);
        }
    }
}
