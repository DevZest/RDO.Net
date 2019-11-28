using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.CodeAnalysis
{
    [TestClass]
    public class ImportedNamespaceTests
    {
        [TestMethod]
        public void GetImportedNamespaces_CS()
        {
            var src =
@"using System;

namespace Test
{
}
";

            var document = src.CreateDocument();
            var syntaxTree = document.GetSyntaxTreeAsync().Result;
            var compilation = document.Project.GetCompilationAsync().Result;
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var result = ImportedNamespace.GetImportedNamespaces(syntaxTree, semanticModel);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetImportedNamespaces_VB()
        {
            var src =
@"Imports System;

Namespace Test
{
}
";

            var document = src.CreateDocument(LanguageNames.VisualBasic);
            var syntaxTree = document.GetSyntaxTreeAsync().Result;
            var compilation = document.Project.GetCompilationAsync().Result;
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var result = ImportedNamespace.GetImportedNamespaces(syntaxTree, semanticModel);
            Assert.AreEqual(1, result.Count);
        }
    }
}
