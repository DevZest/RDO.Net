using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace DevZest.Data
{
    [TestClass]
    public class DbGeneratorTests
    {
        private static Db CreateDb()
        {
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string attachDbFilename = Path.Combine(outputFolder, "DbGenAdventureWorksLT.mdf");
            File.Copy(Path.Combine(outputFolder, "EmptyDb.mdf"), attachDbFilename, true);
            File.Copy(Path.Combine(outputFolder, "EmptyDb_log.ldf"), Path.Combine(outputFolder, "DbGenAdventureWorksLT_log.ldf"), true);
            var connectionString = string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
            return new Db(connectionString);
        }

        [TestMethod]
        public void DbGenerator_Generate()
        {
            using (var db = CreateDb())
            {
                int count = 0;
                int index = 0;
                new DbGenerator<Db>().GenerateAsync(db, new Progress<MockDbProgress>(x =>
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
