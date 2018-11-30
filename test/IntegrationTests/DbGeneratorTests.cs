using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace DevZest.Data.Primitives
{
    [TestClass]
    public class DbGeneratorTests
    {
        private static string GetEmptyDbConnectionString()
        {
            string mdfFilename = "EmptyDb.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string attachDbFilename = Path.Combine(outputFolder, mdfFilename);
            return string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
        }

        private sealed class DbGenerator : DbGenerator<Db>
        {
        }

        [TestMethod]
        public void DbSession_CreateTables()
        {
            using (var db = new Db(GetEmptyDbConnectionString()))
            {
                int count = 0;
                int index = 0;
                new DbGenerator().GenerateAsync(db, new Progress<MockDbProgress>(x =>
                {
                    Assert.AreEqual(index, x.Index);
                    if (x.Index == 0)
                    {
                        Assert.AreEqual(0, count);
                        count = x.Count;
                    }
                    else
                        Assert.AreEqual(count, x.Count);
                    index++;
                })).Wait();
                Assert.IsTrue(count > 0);
                Assert.AreEqual(index, count);
            }
        }
    }
}
