using DevZest.Data.Resources;
using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class PagedDataSetTests
    {
        private static void Verify<T>(PagedDataSet<T> pagedDataSet, int page, int pageSize, int totalCount)
            where T : Model, new()
        {
            Assert.AreEqual(page, pagedDataSet.Page);
            Assert.AreEqual(pageSize, pagedDataSet.PageSize);
            Assert.AreEqual(totalCount, pagedDataSet.TotalCount);
        }

        private static void Verify<T>(PagedDataSet<T> pagedDataSet, string json)
            where T : Model, new()
        {
            Assert.AreEqual(json, pagedDataSet.ToJsonString(true));
        }

        [TestMethod]
        public void PagedDataSet_null_DataSet()
        {
            var expectedJson =
@"[
   {
      ""Page"" : 0,
      ""PageSize"" : 0,
      ""TotalCount"" : 0,
      ""Data"" : null
   }
]";
            {
                var pagedDataSet = new PagedDataSet<SalesOrder>();
                Verify(pagedDataSet, 0, 0, 0);
                Assert.IsTrue(pagedDataSet.Data == null);

                Verify(pagedDataSet, expectedJson);
            }

            {
                var pagedDataSet = PagedDataSet<SalesOrder>.ParseJson(expectedJson);
                Verify(pagedDataSet, 0, 0, 0);
                Assert.IsTrue(pagedDataSet.Data == null);
            }
        }

        [TestMethod]
        public void PagedDataSet_not_null_DataSet()
        {
            var expectedJson =
@"[
   {
      ""Page"" : 1,
      ""PageSize"" : 3,
      ""TotalCount"" : 10,
      ""Data"" : [
         {
            ""SalesOrderID"" : 71774,
            ""RevisionNumber"" : 2,
            ""OrderDate"" : ""2008-06-01T00:00:00.000"",
            ""DueDate"" : ""2008-06-13T00:00:00.000"",
            ""ShipDate"" : ""2008-06-08T00:00:00.000"",
            ""Status"" : 5,
            ""OnlineOrderFlag"" : false,
            ""PurchaseOrderNumber"" : ""PO348186287"",
            ""AccountNumber"" : ""10-4020-000609"",
            ""CustomerID"" : 29847,
            ""ShipToAddressID"" : 1092,
            ""BillToAddressID"" : 1092,
            ""ShipMethod"" : ""CARGO TRANSPORT 5"",
            ""CreditCardApprovalCode"" : null,
            ""SubTotal"" : 880.3484,
            ""TaxAmt"" : 70.4279,
            ""Freight"" : 22.0087,
            ""Comment"" : null,
            ""RowGuid"" : ""89e42cdc-8506-48a2-b89b-eb3e64e3554e"",
            ""ModifiedDate"" : ""2008-06-08T00:00:00.000"",
            ""SalesOrderDetails"" : [
               {
                  ""SalesOrderDetailID"" : 110562,
                  ""OrderQty"" : 1,
                  ""ProductID"" : 836,
                  ""UnitPrice"" : 356.8980,
                  ""UnitPriceDiscount"" : 0,
                  ""LineTotal"" : 356.898000,
                  ""RowGuid"" : ""e3a1994c-7a68-4ce8-96a3-77fdd3bbd730"",
                  ""ModifiedDate"" : ""2008-06-01T00:00:00.000""
               },
               {
                  ""SalesOrderDetailID"" : 110563,
                  ""OrderQty"" : 1,
                  ""ProductID"" : 822,
                  ""UnitPrice"" : 356.8980,
                  ""UnitPriceDiscount"" : 0,
                  ""LineTotal"" : 356.898000,
                  ""RowGuid"" : ""5c77f557-fdb6-43ba-90b9-9a7aec55ca32"",
                  ""ModifiedDate"" : ""2008-06-01T00:00:00.000""
               }
            ]
         }
      ]
   }
]";
            {
                var dataSet = DataSet<SalesOrder>.ParseJson(Json.Sales_Order_71774);
                var pagedDataSet = new PagedDataSet<SalesOrder>()
                {
                    Page = 1,
                    PageSize = 3,
                    TotalCount = 10,
                    Data = dataSet
                };
                Verify(pagedDataSet, 1, 3, 10);
                Assert.IsTrue(pagedDataSet.Data == dataSet);
                Verify(pagedDataSet, expectedJson);
            }

            {
                var pagedDataSet = PagedDataSet<SalesOrder>.ParseJson(expectedJson);
                Verify(pagedDataSet, 1, 3, 10);
                Assert.IsTrue(pagedDataSet.Data.Count == 1);
                Verify(pagedDataSet, expectedJson);
            }
        }
    }
}
