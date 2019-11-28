using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    public partial struct CodeContext
    {
        private abstract class LanguageDispatcher
        {
            public abstract INamedTypeSymbol GetCurrentType(CodeContext codeContext, string knownType);
            public abstract IMethodSymbol GetCurrentMethod(CodeContext codeContext);
        }

        private static LanguageDispatcher GetLanguageDispatcher(Document document)
        {
            if (document == null)
                return null;
            var project = document.Project;
            if (project == null)
                return null;
            var language = project.Language;
            if (language == LanguageNames.CSharp)
                return CSharp.GetDispatcher();
            else if (language == LanguageNames.VisualBasic)
                return VisualBasic.GetDispatcher();
            else
                return null;
        }

        public static bool IsSupported(Document document)
        {
            return GetLanguageDispatcher(document) != null;
        }

        public INamedTypeSymbol GetCurrentType(string knownType)
        {
            return GetLanguageDispatcher(Document)?.GetCurrentType(this, knownType);
        }

        public IMethodSymbol GetCurrentMethod()
        {
            return GetLanguageDispatcher(Document)?.GetCurrentMethod(this);
        }

        public static CodeContext Create(Document document, TextSpan textSpan)
        {
            if (!IsSupported(document))
                return default(CodeContext);

            var rootSyntaxNode = document.GetSyntaxRootAsync().Result;
            var currentSyntaxNode = rootSyntaxNode.FindNode(textSpan);
            var compilation = document.Project.GetCompilationAsync().Result;
            var semanticModel = compilation.GetSemanticModel(rootSyntaxNode.SyntaxTree);
            return new CodeContext(document, rootSyntaxNode, currentSyntaxNode, compilation, semanticModel);
        }

        private CodeContext(Document document, SyntaxNode rootSyntaxNode, SyntaxNode currentSyntaxNode, Compilation compilation, SemanticModel semanticModel)
        {
            Document = document;
            RootSyntaxNode = rootSyntaxNode;
            CurrentSyntaxNode = currentSyntaxNode;
            Compilation = compilation;
            SemanticModel = semanticModel;
        }

        public readonly Document Document;

        public Project Project
        {
            get { return Document?.Project; }
        }

        public string Language
        {
            get { return Project?.Language; }
        }

        public readonly SyntaxNode RootSyntaxNode;

        public SyntaxTree SyntaxTree
        {
            get { return RootSyntaxNode.SyntaxTree; }
        }

        public readonly SyntaxNode CurrentSyntaxNode;

        public readonly Compilation Compilation;

        public readonly SemanticModel SemanticModel;

        public bool IsEmpty
        {
            get { return Document == null; }
        }

        public TNode FindClassDeclaration<TNode>(string knownType, out INamedTypeSymbol typeSymbol)
            where TNode : SyntaxNode
        {
            var currentNode = CurrentSyntaxNode;
            if (currentNode is TNode result)
            {
                typeSymbol = GetTypeSymbol(result, knownType);
                if (typeSymbol != null)
                    return result;
            }

            while (currentNode != null)
            {
                result = FindAncestorClassDeclaration<TNode>(ref currentNode, knownType, out typeSymbol);
                if (result != null)
                    return result;
            }

            return FindSingleClassDeclaration<TNode>(knownType, out typeSymbol);
        }

        private TNode FindAncestorClassDeclaration<TNode>(ref SyntaxNode currentNode, string knownType, out INamedTypeSymbol typeSymbol)
            where TNode : SyntaxNode
        {
            TNode result;
            currentNode = result = currentNode.FirstAncestor<TNode>();
            typeSymbol = GetTypeSymbol(result, knownType);
            return typeSymbol == null ? null : result;
        }

        private TNode FindSingleClassDeclaration<TNode>(string knownType, out INamedTypeSymbol typeSymbol)
            where TNode : SyntaxNode
        {
            var results = RootSyntaxNode.DescendantNodesAndSelf(x => !(x is TNode)).OfType<TNode>().ToArray();

            typeSymbol = null;
            TNode result = null;
            if (results != null && results.Length == 1)
            {
                result = results[0];
                typeSymbol = GetTypeSymbol(result, knownType);
            }
            return typeSymbol == null ? null : result;
        }

        private INamedTypeSymbol GetTypeSymbol(SyntaxNode result, string knownType)
        {
            if (result != null && SemanticModel.GetDeclaredSymbol(result) is INamedTypeSymbol typeSymbol && typeSymbol.IsDerivedFrom(knownType, Compilation))
                return typeSymbol;
            return null;
        }
    }
}
