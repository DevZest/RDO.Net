using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DevZest.Data.CodeAnalysis
{
    [TestClass]
    public class ExtensionsTests
    {
        [TestMethod]
        public void AddMissingNamespaces_CS()
        {
            var src =
@"namespace Test
{
}
";

            var document = src.CreateDocument();
            var compilation = document.Project.GetCompilationAsync().Result;
            var namespaces = new NamespaceSet();
            namespaces.Add(compilation.GetKnownType(KnownTypes.Attribute));

            var editor = DocumentEditor.CreateAsync(document).Result;
            editor.AddMissingNamespaces(namespaces, null, ImportedNamespace.AddUsings, CancellationToken.None).Wait();

            var expected =
@"using System;

namespace Test
{
}
";
            Assert.AreEqual(expected, editor.FormatAsync().Result.GetSourceCode());
        }

        [TestMethod]
        public void AddMissingNamespaces_VB()
        {
            var src =
@"Namespace Test
{
}
";

            var document = src.CreateDocument(LanguageNames.VisualBasic);
            var compilation = document.Project.GetCompilationAsync().Result;
            var namespaces = new NamespaceSet();
            namespaces.Add(compilation.GetKnownType(KnownTypes.Attribute));

            var editor = DocumentEditor.CreateAsync(document).Result;
            editor.AddMissingNamespaces(namespaces, null, ImportedNamespace.AddImports, CancellationToken.None).Wait();

            var expected =
@"Imports System

Namespace Test
{
}
";
            Assert.AreEqual(expected, editor.FormatAsync().Result.GetSourceCode());
        }

        [TestMethod]
        public void AddMissingNamespaces_multiple_append()
        {
            var src =
@"using DevZest.Data;

namespace Test
{
}
";

            var document = src.CreateDocument();
            var compilation = document.Project.GetCompilationAsync().Result;
            var namespaces = new NamespaceSet
            {
                compilation.GetKnownType(KnownTypes.Attribute),
                compilation.GetKnownType(KnownTypes.AscAttribute)
            };

            var editor = DocumentEditor.CreateAsync(document).Result;
            editor.AddMissingNamespaces(namespaces, null, ImportedNamespace.AddUsings, CancellationToken.None).Wait();

            var expected =
@"using DevZest.Data;
using DevZest.Data.Annotations;
using System;

namespace Test
{
}
";
            Assert.AreEqual(expected, editor.FormatAsync().Result.GetSourceCode());
        }
    }
}
