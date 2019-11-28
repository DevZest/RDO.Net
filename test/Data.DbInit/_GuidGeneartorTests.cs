using DevZest.Data.DbInit.TestModels;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace DevZest.Data.DbInit
{
    [TestClass]
    public class _GuidGeneartorTests
    {
        [TestMethod]
        public void _GuidGenerator_CS()
        {
            var dataSet = DataSet<_GuidModel>.Create();
            dataSet.AddRows(2);
            var _ = dataSet._;
            _.Column[0] = null;
            _.Column[1] = new Guid("ADD56204-38F3-4E24-B281-AC40CDDE7C67");

            using (var g = new DataSetGenerator(dataSet, LanguageNames.CSharp))
            {
                var referencedTypes = g.GetReferencedTypes().ToArray();
                Assert.AreEqual(5, referencedTypes.Length);
                Assert.AreEqual("DevZest.Data.DataSet`1", referencedTypes[0]);
                Assert.AreEqual("DevZest.Data.DbInit.DbInitExtensions", referencedTypes[1]);
                Assert.AreEqual("DevZest.Data.DbInit.TestModels._GuidModel", referencedTypes[2]);
                Assert.AreEqual("DevZest.Data.Primitives.ModelExtensions", referencedTypes[3]);
                Assert.AreEqual("System.Guid", referencedTypes[4]);

                var statements = g.GetStatements().ToArray();
                Assert.AreEqual(9, statements.Length);
                Assert.AreEqual("{", statements[0]);
                Assert.AreEqual("DataSet<_GuidModel> result = DataSet<_GuidModel>.Create().AddRows(2);", statements[1]);
                Assert.AreEqual("_GuidModel _ = result._;", statements[2]);
                Assert.AreEqual("_.SuspendIdentity();", statements[3]);
                Assert.AreEqual(@"_.Column[0] = null;", statements[4]);
                Assert.AreEqual(@"_.Column[1] = new Guid(""add56204-38f3-4e24-b281-ac40cdde7c67"");", statements[5]);
                Assert.AreEqual("_.ResumeIdentity();", statements[6]);
                Assert.AreEqual("return result;", statements[7]);
                Assert.AreEqual("}", statements[8]);
            }
        }

        [TestMethod]
        public void _GuidGenerator_VB()
        {
            var dataSet = DataSet<_GuidModel>.Create();
            dataSet.AddRows(2);
            var _ = dataSet._;
            _.Column[0] = null;
            _.Column[1] = new Guid("ADD56204-38F3-4E24-B281-AC40CDDE7C67");

            using (var g = new DataSetGenerator(dataSet, LanguageNames.VisualBasic))
            {
                var referencedTypes = g.GetReferencedTypes().ToArray();
                Assert.AreEqual(5, referencedTypes.Length);
                Assert.AreEqual("DevZest.Data.DataSet`1", referencedTypes[0]);
                Assert.AreEqual("DevZest.Data.DbInit.DbInitExtensions", referencedTypes[1]);
                Assert.AreEqual("DevZest.Data.DbInit.TestModels._GuidModel", referencedTypes[2]);
                Assert.AreEqual("DevZest.Data.Primitives.ModelExtensions", referencedTypes[3]);
                Assert.AreEqual("System.Guid", referencedTypes[4]);

                var statements = g.GetStatements().ToArray();
                Assert.AreEqual(7, statements.Length);
                Assert.AreEqual("Dim result As DataSet(Of _GuidModel) = DataSet(Of _GuidModel).Create().AddRows(2)", statements[0]);
                Assert.AreEqual("Dim x As _GuidModel = result.Entity", statements[1]);
                Assert.AreEqual("x.SuspendIdentity()", statements[2]);
                Assert.AreEqual(@"x.Column(0) = Nothing", statements[3]);
                Assert.AreEqual(@"x.Column(1) = New Guid(""add56204-38f3-4e24-b281-ac40cdde7c67"")", statements[4]);
                Assert.AreEqual("x.ResumeIdentity()", statements[5]);
                Assert.AreEqual("Return result", statements[6]);
            }
        }
    }
}
