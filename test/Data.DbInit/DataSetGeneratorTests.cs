using DevZest.Data.DbInit.TestModels;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DevZest.Data.DbInit
{
    [TestClass]
    public class DataSetGeneratorTests
    {
        [TestMethod]
        public void DataSetGenerator_CS()
        {
            var dataSet = DataSet<SimpleModel>.Create();
            dataSet.AddRows(2);
            var _ = dataSet._;
            _.Id[0] = 0;
            _.Id[1] = 1;

            using (var g = new DataSetGenerator(dataSet, LanguageNames.CSharp))
            {
                var referencedTypes = g.GetReferencedTypes().ToArray();
                Assert.AreEqual(4, referencedTypes.Length);
                Assert.AreEqual("DevZest.Data.DataSet`1", referencedTypes[0]);
                Assert.AreEqual("DevZest.Data.DbInit.DbInitExtensions", referencedTypes[1]);
                Assert.AreEqual("DevZest.Data.DbInit.TestModels.SimpleModel", referencedTypes[2]);
                Assert.AreEqual("DevZest.Data.Primitives.ModelExtensions", referencedTypes[3]);

                var statements = g.GetStatements().ToArray();
                Assert.AreEqual(9, statements.Length);
                Assert.AreEqual("{", statements[0]);
                Assert.AreEqual("DataSet<SimpleModel> result = DataSet<SimpleModel>.Create().AddRows(2);", statements[1]);
                Assert.AreEqual("SimpleModel _ = result._;", statements[2]);
                Assert.AreEqual("_.SuspendIdentity();", statements[3]);
                Assert.AreEqual(@"_.Id[0] = 0;", statements[4]);
                Assert.AreEqual(@"_.Id[1] = 1;", statements[5]);
                Assert.AreEqual("_.ResumeIdentity();", statements[6]);
                Assert.AreEqual("return result;", statements[7]);
                Assert.AreEqual("}", statements[8]);
            }
        }

        [TestMethod]
        public void DataSetGenerator_VB()
        {
            var dataSet = DataSet<SimpleModel>.Create();
            dataSet.AddRows(2);
            var _ = dataSet._;
            _.Id[0] = 0;
            _.Id[1] = 1;

            using (var g = new DataSetGenerator(dataSet, LanguageNames.VisualBasic))
            {
                var referencedTypes = g.GetReferencedTypes().ToArray();
                Assert.AreEqual(4, referencedTypes.Length);
                Assert.AreEqual("DevZest.Data.DataSet`1", referencedTypes[0]);
                Assert.AreEqual("DevZest.Data.DbInit.DbInitExtensions", referencedTypes[1]);
                Assert.AreEqual("DevZest.Data.DbInit.TestModels.SimpleModel", referencedTypes[2]);
                Assert.AreEqual("DevZest.Data.Primitives.ModelExtensions", referencedTypes[3]);

                var statements = g.GetStatements().ToArray();
                Assert.AreEqual(7, statements.Length);
                Assert.AreEqual("Dim result As DataSet(Of SimpleModel) = DataSet(Of SimpleModel).Create().AddRows(2)", statements[0]);
                Assert.AreEqual("Dim x As SimpleModel = result.Entity", statements[1]);
                Assert.AreEqual("x.SuspendIdentity()", statements[2]);
                Assert.AreEqual(@"x.Id(0) = 0", statements[3]);
                Assert.AreEqual(@"x.Id(1) = 1", statements[4]);
                Assert.AreEqual("x.ResumeIdentity()", statements[5]);
                Assert.AreEqual("Return result", statements[6]);
            }
        }
    }
}
