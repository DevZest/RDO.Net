using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace DevZest.Data.Primitives
{
    [TestClass]
    public class DbSessionCreateTablesTests
    {
        private static string GetEmptyDbConnectionString()
        {
            string mdfFilename = "EmptyDb.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string attachDbFilename = Path.Combine(outputFolder, mdfFilename);
            return string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
        }

        [TestMethod]
        public void DbSession_CreateTables()
        {
            using (var db = new Db(GetEmptyDbConnectionString()))
            {
                int count = 0;
                db.CreateTables(new Progress<string>(x =>
                {
                    count++;
                }));
                Assert.IsTrue(count > 0);
                Assert.AreEqual(db.GetTotalNumberOfTables(), count);
            }
        }
    }
}
