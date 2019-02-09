using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Reflection;

namespace DevZest.Data.MySql
{
    [TestClass]
    public class DbGeneratorTests
    {
        private static Db CreateDb()
        {
            CreateEmptyDb();
            return new Db("Server=127.0.0.1;Port=3306;Database=EmptyDb;Uid=root;Allow User Variables=True");
        }

        private static void CreateEmptyDb()
        {
            using (var connection = new MySqlConnection("Server=127.0.0.1;Port=3306;Uid=root"))
            {
                connection.Open();
                var sqlText =
@"set @@sql_notes = 0;
drop database if exists EmptyDb;
set @@sql_notes = 1;

create database EmptyDb;
";
                var command = new MySqlCommand(sqlText, connection);
                command.ExecuteNonQuery();
            }
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
