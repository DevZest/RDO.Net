using DevZest.Data.DbInit.TestModels;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DevZest.Data.DbInit
{
    [TestClass]
    public class _BinaryGeneartorTests
    {
        [TestMethod]
        public void _BinaryGenerator_CS()
        {
            var dataSet = DataSet<_BinaryModel>.Create();
            dataSet.AddRows(2);
            var _ = dataSet._;
            _.Column[0] = null;
            _.Column[1] = new Binary(new byte[] { 1, 2, 3 });

            using (var g = new DataSetGenerator(dataSet, LanguageNames.CSharp))
            {
                var referencedTypes = g.GetReferencedTypes().ToArray();
                Assert.AreEqual(6, referencedTypes.Length);
                Assert.AreEqual("DevZest.Data.Binary", referencedTypes[0]);
                Assert.AreEqual("DevZest.Data.DataSet`1", referencedTypes[1]);
                Assert.AreEqual("DevZest.Data.DbInit.DbInitExtensions", referencedTypes[2]);
                Assert.AreEqual("DevZest.Data.DbInit.TestModels._BinaryModel", referencedTypes[3]);
                Assert.AreEqual("DevZest.Data.Primitives.ModelExtensions", referencedTypes[4]);
                Assert.AreEqual("System.Convert", referencedTypes[5]);

                var statements = g.GetStatements().ToArray();
                Assert.AreEqual(9, statements.Length);
                Assert.AreEqual("{", statements[0]);
                Assert.AreEqual("DataSet<_BinaryModel> result = DataSet<_BinaryModel>.Create().AddRows(2);", statements[1]);
                Assert.AreEqual("_BinaryModel _ = result._;", statements[2]);
                Assert.AreEqual("_.SuspendIdentity();", statements[3]);
                Assert.AreEqual(@"_.Column[0] = null;", statements[4]);
                Assert.AreEqual(@"_.Column[1] = new Binary(Convert.FromBase64String(""AQID""));", statements[5]);
                Assert.AreEqual("_.ResumeIdentity();", statements[6]);
                Assert.AreEqual("return result;", statements[7]);
                Assert.AreEqual("}", statements[8]);
            }
        }

        [TestMethod]
        public void _BinaryGenerator_VB()
        {
            var dataSet = DataSet<_BinaryModel>.Create();
            dataSet.AddRows(2);
            var _ = dataSet._;
            _.Column[0] = null;
            _.Column[1] = new Binary(new byte[] { 1, 2, 3 });

            using (var g = new DataSetGenerator(dataSet, LanguageNames.VisualBasic))
            {
                var referencedTypes = g.GetReferencedTypes().ToArray();
                Assert.AreEqual(6, referencedTypes.Length);
                Assert.AreEqual("DevZest.Data.Binary", referencedTypes[0]);
                Assert.AreEqual("DevZest.Data.DataSet`1", referencedTypes[1]);
                Assert.AreEqual("DevZest.Data.DbInit.DbInitExtensions", referencedTypes[2]);
                Assert.AreEqual("DevZest.Data.DbInit.TestModels._BinaryModel", referencedTypes[3]);
                Assert.AreEqual("DevZest.Data.Primitives.ModelExtensions", referencedTypes[4]);
                Assert.AreEqual("System.Convert", referencedTypes[5]);

                var statements = g.GetStatements().ToArray();
                Assert.AreEqual(7, statements.Length);
                Assert.AreEqual("Dim result As DataSet(Of _BinaryModel) = DataSet(Of _BinaryModel).Create().AddRows(2)", statements[0]);
                Assert.AreEqual("Dim x As _BinaryModel = result.Entity", statements[1]);
                Assert.AreEqual("x.SuspendIdentity()", statements[2]);
                Assert.AreEqual(@"x.Column(0) = Nothing", statements[3]);
                Assert.AreEqual(@"x.Column(1) = New Binary(Convert.FromBase64String(""AQID""))", statements[4]);
                Assert.AreEqual("x.ResumeIdentity()", statements[5]);
                Assert.AreEqual("Return result", statements[6]);
            }
        }
    }
}
