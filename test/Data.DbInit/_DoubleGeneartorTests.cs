using DevZest.Data.DbInit.TestModels;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DevZest.Data.DbInit
{
    [TestClass]
    public class _DoubleGeneartorTests
    {
        [TestMethod]
        public void _DoubleGenerator_CS()
        {
            var dataSet = DataSet<_DoubleModel>.Create();
            dataSet.AddRows(2);
            var _ = dataSet._;
            _.Column[0] = null;
            _.Column[1] = 100;

            using (var g = new DataSetGenerator(dataSet, LanguageNames.CSharp))
            {
                var referencedTypes = g.GetReferencedTypes().ToArray();
                Assert.AreEqual(4, referencedTypes.Length);
                Assert.AreEqual("DevZest.Data.DataSet`1", referencedTypes[0]);
                Assert.AreEqual("DevZest.Data.DbInit.DbInitExtensions", referencedTypes[1]);
                Assert.AreEqual("DevZest.Data.DbInit.TestModels._DoubleModel", referencedTypes[2]);
                Assert.AreEqual("DevZest.Data.Primitives.ModelExtensions", referencedTypes[3]);

                var statements = g.GetStatements().ToArray();
                Assert.AreEqual(9, statements.Length);
                Assert.AreEqual("{", statements[0]);
                Assert.AreEqual("DataSet<_DoubleModel> result = DataSet<_DoubleModel>.Create().AddRows(2);", statements[1]);
                Assert.AreEqual("_DoubleModel _ = result._;", statements[2]);
                Assert.AreEqual("_.SuspendIdentity();", statements[3]);
                Assert.AreEqual(@"_.Column[0] = null;", statements[4]);
                Assert.AreEqual(@"_.Column[1] = 100D;", statements[5]);
                Assert.AreEqual("_.ResumeIdentity();", statements[6]);
                Assert.AreEqual("return result;", statements[7]);
                Assert.AreEqual("}", statements[8]);
            }
        }

        [TestMethod]
        public void _DoubleGenerator_VB()
        {
            var dataSet = DataSet<_DoubleModel>.Create();
            dataSet.AddRows(2);
            var _ = dataSet._;
            _.Column[0] = null;
            _.Column[1] = 100;

            using (var g = new DataSetGenerator(dataSet, LanguageNames.VisualBasic))
            {
                var referencedTypes = g.GetReferencedTypes().ToArray();
                Assert.AreEqual(4, referencedTypes.Length);
                Assert.AreEqual("DevZest.Data.DataSet`1", referencedTypes[0]);
                Assert.AreEqual("DevZest.Data.DbInit.DbInitExtensions", referencedTypes[1]);
                Assert.AreEqual("DevZest.Data.DbInit.TestModels._DoubleModel", referencedTypes[2]);
                Assert.AreEqual("DevZest.Data.Primitives.ModelExtensions", referencedTypes[3]);

                var statements = g.GetStatements().ToArray();
                Assert.AreEqual(7, statements.Length);
                Assert.AreEqual("Dim result As DataSet(Of _DoubleModel) = DataSet(Of _DoubleModel).Create().AddRows(2)", statements[0]);
                Assert.AreEqual("Dim x As _DoubleModel = result.Entity", statements[1]);
                Assert.AreEqual("x.SuspendIdentity()", statements[2]);
                Assert.AreEqual(@"x.Column(0) = Nothing", statements[3]);
                Assert.AreEqual(@"x.Column(1) = 100R", statements[4]);
                Assert.AreEqual("x.ResumeIdentity()", statements[5]);
                Assert.AreEqual("Return result", statements[6]);
            }
        }
    }
}
