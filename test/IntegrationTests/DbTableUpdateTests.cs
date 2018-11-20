using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data
{
    [TestClass]
    public class DbTableUpdateTests : AdventureWorksTestsBase
    {
        [TestMethod]
        public void DbTable_Update_without_source()
        {
            var log = new StringBuilder();
            using (var db = new ProductCategoryMockDb().InitializeAsync(OpenDbAsync(log).Result).Result)
            {
                var count = db.ProductCategory.Where(x => x.ProductCategoryID > 2).CountAsync().Result;
                Assert.IsTrue(count > 0);
                _DateTime newModifiedDate = new DateTime(2015, 11, 19);
                db.ProductCategory.Update((builder, productCategory) =>
                {
                    builder.Select(newModifiedDate, productCategory.ModifiedDate);
                }, x => x.ProductCategoryID > 2).ExecuteAsync().Wait();
                Assert.AreEqual(count, db.ProductCategory.Where(x => x.ModifiedDate == newModifiedDate).CountAsync().Result);
            }
        }

        [TestMethod]
        public async Task DbTable_UpdateAsync_without_source()
        {
            var log = new StringBuilder();
            using (var db = new ProductCategoryMockDb().InitializeAsync(OpenDbAsync(log).Result).Result)
            {
                var count = await db.ProductCategory.Where(x => x.ProductCategoryID > 2).CountAsync();
                Assert.IsTrue(count > 0);
                _DateTime newModifiedDate = new DateTime(2015, 11, 19);
                await db.ProductCategory.Update((builder, productCategory) =>
                {
                    builder.Select(newModifiedDate, productCategory.ModifiedDate);
                }, x => x.ProductCategoryID > 2).ExecuteAsync();
                Assert.AreEqual(count, await db.ProductCategory.Where(x => x.ModifiedDate == newModifiedDate).CountAsync());
            }
        }

        [TestMethod]
        public void DbTable_Update_from_Scalar()
        {
            var log = new StringBuilder();
            using (var db = new ProductCategoryMockDb().InitializeAsync(OpenDbAsync(log).Result).Result)
            {
                var dataSet = db.ProductCategory.ToDataSetAsync().Result;
                Assert.IsTrue(dataSet.Count > 1);
                var newModifiedDate = new DateTime(2015, 11, 19);
                dataSet._.ModifiedDate[0] = newModifiedDate;

                db.ProductCategory.Update(dataSet, 0).ExecuteAsync().Wait();
                Assert.AreEqual(1, db.ProductCategory.Where(x => x.ModifiedDate == newModifiedDate).CountAsync().Result);
            }
        }

        [TestMethod]
        public async Task DbTable_UpdateAsync_from_Scalar()
        {
            var log = new StringBuilder();
            using (var db = new ProductCategoryMockDb().InitializeAsync(OpenDbAsync(log).Result).Result)
            {
                var dataSet = await db.ProductCategory.ToDataSetAsync();
                Assert.IsTrue(dataSet.Count > 1);
                var newModifiedDate = new DateTime(2015, 11, 19);
                dataSet._.ModifiedDate[0] = newModifiedDate;

                await db.ProductCategory.Update(dataSet, 0).ExecuteAsync();
                Assert.AreEqual(1, await db.ProductCategory.Where(x => x.ModifiedDate == newModifiedDate).CountAsync());
            }
        }

        [TestMethod]
        public void DbTable_Update_from_DataSet()
        {
            var log = new StringBuilder();
            using (var db = new ProductCategoryMockDb().InitializeAsync(OpenDbAsync(log).Result).Result)
            {
                var dataSet = db.ProductCategory.ToDataSetAsync().Result;
                var count = dataSet.Count;
                Assert.IsTrue(count > 1);
                var newModifiedDate = new DateTime(2015, 11, 19);
                for (int i = 0; i < count; i++)
                    dataSet._.ModifiedDate[i] = newModifiedDate;

                db.ProductCategory.Update(dataSet).ExecuteAsync().Wait();
                Assert.AreEqual(count, db.ProductCategory.Where(x => x.ModifiedDate == newModifiedDate).CountAsync().Result);
            }
        }

        [TestMethod]
        public async Task DbTable_UpdateAsync_from_DataSet()
        {
            var log = new StringBuilder();
            using (var db = new ProductCategoryMockDb().InitializeAsync(OpenDbAsync(log).Result).Result)
            {
                var dataSet = await db.ProductCategory.ToDataSetAsync();
                var count = dataSet.Count;
                Assert.IsTrue(count > 1);
                var newModifiedDate = new DateTime(2015, 11, 19);
                for (int i = 0; i < count; i++)
                    dataSet._.ModifiedDate[i] = newModifiedDate;

                await db.ProductCategory.Update(dataSet).ExecuteAsync();
                Assert.AreEqual(count, await db.ProductCategory.Where(x => x.ModifiedDate == newModifiedDate).CountAsync());
            }
        }
    }
}
