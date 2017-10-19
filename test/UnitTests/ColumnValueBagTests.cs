using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevZest.Samples.AdventureWorksLT;

namespace DevZest.Data
{
    [TestClass]
    public class ColumnValueBagTests
    {
        [TestMethod]
        public void ColumnValueBag()
        {
            var salesOrders = DataSet<SalesOrder.Edit>.New();
            var _ = salesOrders._;
            var ext = _.GetExtension<SalesOrder.Ext>();
            var customer = ext.Customer;
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
                valueBag.SetValue(_.CustomerID, 2);
                valueBag.SetValue(customer.FirstName, "FirstName");
                Assert.AreEqual(2, valueBag.GetValue(_.CustomerID));
                Assert.AreEqual(2, valueBag[_.CustomerID]);
                Assert.AreEqual("FirstName", valueBag.GetValue(customer.FirstName));
                Assert.AreEqual("FirstName", valueBag[customer.FirstName]);
            }

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
                var salesOrder = DataSet<SalesOrder.Edit>.New().EnsureInitialized()._;
                var customerKey = salesOrder.Customer;
                var customerExt = salesOrder.GetExtension<SalesOrder.Ext>().Customer;
                var valueBag = new ColumnValueBag();
                valueBag.AutoSelect(customerKey, salesOrders[0]);
                valueBag.AutoSelect(customerExt, salesOrders[0]);
                Assert.AreEqual(5, valueBag.GetValue(customerKey.CustomerID));
                Assert.AreEqual(5, valueBag[customerKey.CustomerID]);
                Assert.AreEqual("Mr.", valueBag.GetValue(customerExt.Title));
                Assert.AreEqual("John", valueBag.GetValue(customerExt.FirstName));
                Assert.AreEqual("K", valueBag.GetValue(customerExt.MiddleName));
                Assert.AreEqual("Smith", valueBag.GetValue(customerExt.LastName));
                Assert.AreEqual("Good Toys", valueBag.GetValue(customerExt.CompanyName));
                Assert.AreEqual(@"john.smith@goodtoys.com", valueBag.GetValue(customerExt.EmailAddress));
                Assert.AreEqual("555-123-4567", valueBag.GetValue(customerExt.Phone));
                Assert.AreEqual("Mr.", valueBag[customerExt.Title]);
                Assert.AreEqual("John", valueBag[customerExt.FirstName]);
                Assert.AreEqual("K", valueBag[customerExt.MiddleName]);
                Assert.AreEqual("Smith", valueBag[customerExt.LastName]);
                Assert.AreEqual("Good Toys", valueBag[customerExt.CompanyName]);
                Assert.AreEqual(@"john.smith@goodtoys.com", valueBag[customerExt.EmailAddress]);
                Assert.AreEqual("555-123-4567", valueBag[customerExt.Phone]);
            }
        }
    }
}
