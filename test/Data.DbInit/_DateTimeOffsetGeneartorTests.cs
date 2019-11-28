using DevZest.Data.DbInit.TestModels;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace DevZest.Data.DbInit
{
    [TestClass]
    public class _DateTimeOffsetGeneartorTests
    {
        [TestMethod]
        public void _DateTimeOffsetGenerator_CS()
        {
            var dataSet = DataSet<_DateTimeOffsetModel>.Create();
            dataSet.AddRows(2);
            var _ = dataSet._;
            _.Column[0] = null;
            _.Column[1] = new DateTimeOffset(new DateTime(2018, 12, 13), new TimeSpan(5, 0, 0));

            using (var g = new DataSetGenerator(dataSet, LanguageNames.CSharp))
            {
                var referencedTypes = g.GetReferencedTypes().ToArray();
                Assert.AreEqual(5, referencedTypes.Length);
                Assert.AreEqual("DevZest.Data.DataSet`1", referencedTypes[0]);
                Assert.AreEqual("DevZest.Data.DbInit.DbInitExtensions", referencedTypes[1]);
                Assert.AreEqual("DevZest.Data.DbInit.TestModels._DateTimeOffsetModel", referencedTypes[2]);
                Assert.AreEqual("DevZest.Data.Primitives.ModelExtensions", referencedTypes[3]);
                Assert.AreEqual("System.DateTimeOffset", referencedTypes[4]);

                var statements = g.GetStatements().ToArray();
                Assert.AreEqual(9, statements.Length);
                Assert.AreEqual("{", statements[0]);
                Assert.AreEqual("DataSet<_DateTimeOffsetModel> result = DataSet<_DateTimeOffsetModel>.Create().AddRows(2);", statements[1]);
                Assert.AreEqual("_DateTimeOffsetModel _ = result._;", statements[2]);
                Assert.AreEqual("_.SuspendIdentity();", statements[3]);
                Assert.AreEqual(@"_.Column[0] = null;", statements[4]);
                Assert.AreEqual(@"_.Column[1] = DateTimeOffset.Parse(""2018-12-13T00:00:00.0000000+05:00"");", statements[5]);
                Assert.AreEqual("_.ResumeIdentity();", statements[6]);
                Assert.AreEqual("return result;", statements[7]);
                Assert.AreEqual("}", statements[8]);
            }
        }

        [TestMethod]
        public void _DateTimeOffsetGenerator_VB()
        {
            var dataSet = DataSet<_DateTimeOffsetModel>.Create();
            dataSet.AddRows(2);
            var _ = dataSet._;
            _.Column[0] = null;
            _.Column[1] = new DateTimeOffset(new DateTime(2018, 12, 13), new TimeSpan(5, 0, 0));

            using (var g = new DataSetGenerator(dataSet, LanguageNames.VisualBasic))
            {
                var referencedTypes = g.GetReferencedTypes().ToArray();
                Assert.AreEqual(5, referencedTypes.Length);
                Assert.AreEqual("DevZest.Data.DataSet`1", referencedTypes[0]);
                Assert.AreEqual("DevZest.Data.DbInit.DbInitExtensions", referencedTypes[1]);
                Assert.AreEqual("DevZest.Data.DbInit.TestModels._DateTimeOffsetModel", referencedTypes[2]);
                Assert.AreEqual("DevZest.Data.Primitives.ModelExtensions", referencedTypes[3]);
                Assert.AreEqual("System.DateTimeOffset", referencedTypes[4]);

                var statements = g.GetStatements().ToArray();
                Assert.AreEqual(7, statements.Length);
                Assert.AreEqual("Dim result As DataSet(Of _DateTimeOffsetModel) = DataSet(Of _DateTimeOffsetModel).Create().AddRows(2)", statements[0]);
                Assert.AreEqual("Dim x As _DateTimeOffsetModel = result.Entity", statements[1]);
                Assert.AreEqual("x.SuspendIdentity()", statements[2]);
                Assert.AreEqual(@"x.Column(0) = Nothing", statements[3]);
                Assert.AreEqual(@"x.Column(1) = DateTimeOffset.Parse(""2018-12-13T00:00:00.0000000+05:00"")", statements[4]);
                Assert.AreEqual("x.ResumeIdentity()", statements[5]);
                Assert.AreEqual("Return result", statements[6]);
            }
        }
    }
}
