using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace DevZest.Data.CodeAnalysis
{
    internal abstract partial class ImportedNamespace
    {
        public static IReadOnlyList<ImportedNamespace> GetImportedNamespaces(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            var root = syntaxTree.GetRoot();
            if (semanticModel.Language == LanguageNames.CSharp)
                return GetUsings(root, semanticModel);
            else if (semanticModel.Language == LanguageNames.VisualBasic)
                return GetImports(root, semanticModel);
            else
                return null;
        }

        public static Func<SyntaxNode, IEnumerable<SyntaxNode>, SyntaxNode> GetAddFunc(string languageName)
        {
            if (languageName == LanguageNames.CSharp)
                return AddUsings;
            else if (languageName == LanguageNames.VisualBasic)
                return AddImports;
            else
                return null;
        }

        protected ImportedNamespace(INamespaceSymbol namespaceSymbol)
        {
            NamespaceSymbol = namespaceSymbol;
        }

        public INamespaceSymbol NamespaceSymbol { get; }

        public abstract SyntaxNode SyntaxNode { get; }
    }
}
