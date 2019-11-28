using DevZest.Data.DbInit.TestModels;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DevZest.Data.DbInit
{
    [TestClass]
    public class _BooleanGeneartorTests
    {
        [TestMethod]
        public void _BooleanGenerator_CS()
        {
            var dataSet = DataSet<_BooleanModel>.Create();
            dataSet.AddRows(3);
            var _ = dataSet._;
            _.Column[0] = null;
            _.Column[1] = true;
            _.Column[2] = false;

            using (var g = new DataSetGenerator(dataSet, LanguageNames.CSharp))
            {
                var referencedTypes = g.GetReferencedTypes().ToArray();
                Assert.AreEqual(4, referencedTypes.Length);
                Assert.AreEqual("DevZest.Data.DataSet`1", referencedTypes[0]);
                Assert.AreEqual("DevZest.Data.DbInit.DbInitExtensions", referencedTypes[1]);
                Assert.AreEqual("DevZest.Data.DbInit.TestModels._BooleanModel", referencedTypes[2]);
                Assert.AreEqual("DevZest.Data.Primitives.ModelExtensions", referencedTypes[3]);

                var statements = g.GetStatements().ToArray();
                Assert.AreEqual(10, statements.Length);
                Assert.AreEqual("{", statements[0]);
                Assert.AreEqual("DataSet<_BooleanModel> result = DataSet<_BooleanModel>.Create().AddRows(3);", statements[1]);
                Assert.AreEqual("_BooleanModel _ = result._;", statements[2]);
                Assert.AreEqual("_.SuspendIdentity();", statements[3]);
                Assert.AreEqual(@"_.Column[0] = null;", statements[4]);
                Assert.AreEqual(@"_.Column[1] = true;", statements[5]);
                Assert.AreEqual(@"_.Column[2] = false;", statements[6]);
                Assert.AreEqual("_.ResumeIdentity();", statements[7]);
                Assert.AreEqual("return result;", statements[8]);
                Assert.AreEqual("}", statements[9]);
            }
        }

        [TestMethod]
        public void _BooleanGenerator_VB()
        {
            var dataSet = DataSet<_BooleanModel>.Create();
            dataSet.AddRows(3);
            var _ = dataSet._;
            _.Column[0] = null;
            _.Column[1] = true;
            _.Column[2] = false;

            using (var g = new DataSetGenerator(dataSet, LanguageNames.VisualBasic))
            {
                var referencedTypes = g.GetReferencedTypes().ToArray();
                Assert.AreEqual(4, referencedTypes.Length);
                Assert.AreEqual("DevZest.Data.DataSet`1", referencedTypes[0]);
                Assert.AreEqual("DevZest.Data.DbInit.DbInitExtensions", referencedTypes[1]);
                Assert.AreEqual("DevZest.Data.DbInit.TestModels._BooleanModel", referencedTypes[2]);
                Assert.AreEqual("DevZest.Data.Primitives.ModelExtensions", referencedTypes[3]);

                var statements = g.GetStatements().ToArray();
                Assert.AreEqual(8, statements.Length);
                Assert.AreEqual("Dim result As DataSet(Of _BooleanModel) = DataSet(Of _BooleanModel).Create().AddRows(3)", statements[0]);
                Assert.AreEqual("Dim x As _BooleanModel = result.Entity", statements[1]);
                Assert.AreEqual("x.SuspendIdentity()", statements[2]);
                Assert.AreEqual(@"x.Column(0) = Nothing", statements[3]);
                Assert.AreEqual(@"x.Column(1) = True", statements[4]);
                Assert.AreEqual(@"x.Column(2) = False", statements[5]);
                Assert.AreEqual("x.ResumeIdentity()", statements[6]);
                Assert.AreEqual("Return result", statements[7]);
            }
        }
    }
}
