using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.CodeAnalysis
{
    public abstract partial class ClassMapper : CodeMapper
    {
        protected abstract class Factory<T>
            where T : ClassMapper
        {
            public T GetMapper(T mapper, Document document, TextSpan textSpan)
            {
                var context = CodeContext.Create(document, textSpan);
                if (context.IsEmpty)
                    return null;

                if (context.Language == LanguageNames.CSharp)
                    return GetCSharpMapper(mapper, context);
                else if (context.Language == LanguageNames.VisualBasic)
                    return GetVisualBasicMapper(mapper, context);
                else
                    return null;
            }

            protected abstract T GetCSharpMapper(T mapper, CodeContext context);

            protected abstract T GetVisualBasicMapper(T mapper, CodeContext context);
        }

        protected abstract INamespaceSymbol ContainingNamespace { get; }

        protected static IEnumerable<(INamedTypeSymbol Type, AttributeData Attribute)> GetTypeAttributes(INamedTypeSymbol type, Func<AttributeData, bool> predicate)
        {
            if (type == null)
                yield break;

            foreach (var attribute in type.GetAttributes())
            {
                if (predicate(attribute))
                    yield return (type, attribute);
            }

            foreach (var result in GetTypeAttributes(type.BaseType, predicate))
                yield return result;
        }

        protected Task AddMissingNamespaces(DocumentEditor editor, NamespaceSet namespaces, CancellationToken ct)
        {
            return editor.AddMissingNamespacesAsync(namespaces, Compilation, ContainingNamespace, SyntaxTree, SemanticModel, ct);
        }

        internal async Task<Document> RemoveMemberAttributeAsync(AttributeData attribute, CancellationToken ct = default(CancellationToken))
        {
            var node = attribute.ApplicationSyntaxReference.GetSyntax(ct);
            node = GetAttributeNodeToRemove(node);
            var annotation = new SyntaxAnnotation();
            var nodeWithoutWhiteSpaceLeadingTriva = GetNodeWithoutWhitespaceLeadingTriva(node).WithAdditionalAnnotations(annotation);

            var syntaxRoot = await Document.GetSyntaxRootAsync(ct);
            if (node != nodeWithoutWhiteSpaceLeadingTriva)
            {
                syntaxRoot = syntaxRoot.ReplaceNode(node, nodeWithoutWhiteSpaceLeadingTriva);
                nodeWithoutWhiteSpaceLeadingTriva = syntaxRoot.GetAnnotatedNodes(annotation).Single();
            }
            syntaxRoot = syntaxRoot.RemoveNode(nodeWithoutWhiteSpaceLeadingTriva, SyntaxRemoveOptions.KeepLeadingTrivia);
            return Document.WithSyntaxRoot(syntaxRoot);
        }

        private SyntaxNode GetNodeWithoutWhitespaceLeadingTriva(SyntaxNode node)
        {
            var leadingTrivia = node.GetLeadingTrivia();
            if (leadingTrivia.Count == 0 || leadingTrivia.Last().RawKind != WhitespaceTrivaKind)
                return node;

            var trivas = new SyntaxTrivia[leadingTrivia.Count - 1];
            for (int i = 0; i < trivas.Length; i++)
                trivas[i] = leadingTrivia[i];
            return node.WithLeadingTrivia(new SyntaxTriviaList().AddRange(trivas));
        }

        private SyntaxNode GetAttributeNodeToRemove(SyntaxNode node)
        {
            return node.Ancestors().Where(x => x.RawKind == AttributeListSyntaxKind).FirstOrDefault();
        }

        internal async Task<(Document Document, TextSpan? TextSpan)> AddMemberAttribute(ISymbol symbol, INamedTypeSymbol attributeClass,
            string specAttributeKnownType, string namedRequiresArgument = "RequiresArgument", CancellationToken ct = default(CancellationToken))
        {
            var editor = await DocumentEditor.CreateAsync(Document, ct);
            var g = editor.Generator;
            var imports = new NamespaceSet { attributeClass };

            var syntaxNode = GetSyntaxNode(symbol);
            var leadingWhitespaceTrivia = GetLeadingWhitespaceTrivia(syntaxNode);
            var hasArgumentList = HasArgumentList(attributeClass, specAttributeKnownType, namedRequiresArgument);
            var argumentListAnnotation = hasArgumentList ? new SyntaxAnnotation() : null;
            var newSyntaxNode = GenerateAttribute(syntaxNode, attributeClass.Name.ToAttributeName(), leadingWhitespaceTrivia, argumentListAnnotation);
            editor.ReplaceNode(syntaxNode, newSyntaxNode);

            await AddMissingNamespaces(editor, imports, ct);

            var resultDocument = await editor.FormatAsync(ct);

            TextSpan? textSpan;
            if (argumentListAnnotation != null)
            {
                var resultRoot = await resultDocument.GetSyntaxRootAsync(ct);
                var argumentListSyntax = resultRoot.GetAnnotatedNodes(argumentListAnnotation).Single();
                textSpan = new TextSpan(argumentListSyntax.FullSpan.Start + 1, 0);
            }
            else
                textSpan = null;

            return (resultDocument, textSpan);
        }

        protected SyntaxTrivia? GetLeadingWhitespaceTrivia(SyntaxNode syntaxNode)
        {
            var leadingTrivia = syntaxNode.GetLeadingTrivia();
            if (leadingTrivia.Count == 0)
                return null;

            var last = leadingTrivia.Last();
            if (last.RawKind == WhitespaceTrivaKind)
                return last;
            else
                return null;
        }

        protected SyntaxNode GenerateAttribute(SyntaxNode syntaxNode, string name)
        {
            return GenerateAttribute(syntaxNode, name, null, null, null);
        }

        protected SyntaxNode GenerateAttribute(SyntaxNode syntaxNode, string name, SyntaxTrivia? leadingWhitespaceTrivia, SyntaxNode[] arguments)
        {
            return GenerateAttribute(syntaxNode, name, leadingWhitespaceTrivia, arguments, null);
        }

        protected SyntaxNode GenerateAttribute(SyntaxNode syntaxNode, string name, SyntaxTrivia? leadingWhitespaceTrivia, SyntaxAnnotation argumentListAnnotation)
        {
            SyntaxNode[] arguments = argumentListAnnotation == null ? null : Array.Empty<SyntaxNode>();
            return GenerateAttribute(syntaxNode, name, leadingWhitespaceTrivia, arguments, argumentListAnnotation);
        }

        protected abstract SyntaxNode GenerateAttribute(SyntaxNode syntaxNode, string name, SyntaxTrivia? leadingWhitespaceTrivia, SyntaxNode[] arguments, SyntaxAnnotation argumentListAnnotation);

        private bool HasArgumentList(INamedTypeSymbol attributeClass, string specAttributeKnownType, string namedRequiresArgument)
        {
            return !HasParameterlessConstructor(attributeClass) || RequiresArgument(attributeClass, specAttributeKnownType, namedRequiresArgument);
        }

        private static bool HasParameterlessConstructor(INamedTypeSymbol attributeClass)
        {
            return attributeClass.Constructors.Any(x => x.Parameters.Length == 0);
        }

        private bool RequiresArgument(INamedTypeSymbol attributeClass, string specAttributeKnownType, string namedRequiresArgument)
        {
            var spec = attributeClass.GetAttribute(Compilation.GetKnownType(specAttributeKnownType));
            Debug.Assert(spec != null);
            var namedArguments = spec.NamedArguments;
            for (int i = 0; i < namedArguments.Length; i++)
            {
                var namedArgument = namedArguments[i];
                if (namedArgument.Key == namedRequiresArgument)
                {
                    var argument = namedArgument.Value;
                    return argument.Kind == TypedConstantKind.Primitive && true.Equals(argument.Value);
                }
            }
            return false;
        }

        protected SyntaxNode GetLastNode<T>(INamedTypeSymbol type, Func<T, bool> predicate)
            where T : ISymbol
        {
            foreach (var member in type.GetMembers().OfType<T>().Reverse())
            {
                if (!predicate(member))
                    continue;

                var result = GetSyntaxNode(member);
                if (result != null)
                    return result;
            }

            return null;
        }

        private SyntaxNode GetSyntaxNode(ISymbol symbol)
        {
            var syntaxReferences = symbol.DeclaringSyntaxReferences;
            for (int i = 0; i < syntaxReferences.Length; i++)
            {
                var syntaxReference = syntaxReferences[i];
                if (syntaxReference.SyntaxTree == SyntaxTree)
                    return syntaxReference.GetSyntax();
            }
            return null;
        }

        protected abstract int AttributeListSyntaxKind { get; }

        protected abstract int WhitespaceTrivaKind { get; }

        protected async Task<Document> KeepTogetherAsync(Document document, SyntaxAnnotation firstAnnotation, SyntaxAnnotation secondAnnotation, CancellationToken ct)
        {
            var root = await document.GetSyntaxRootAsync(ct);
            var first = root.GetAnnotatedNodes(firstAnnotation).Single();
            var second = root.GetAnnotatedNodes(secondAnnotation).Single();

            var editor = await DocumentEditor.CreateAsync(document, ct);
            editor.ReplaceNode(first, first.WithTrailingTrivia(GetTrailingTriviaToKeep(first)));
            editor.ReplaceNode(second, second.WithLeadingTrivia(GetLeadingTriviaToKeep(second)));

            return editor.GetChangedDocument();
        }

        private IEnumerable<SyntaxTrivia> GetTrailingTriviaToKeep(SyntaxNode node)
        {
            return GetTriviaToKeep(node.GetTrailingTrivia(), true);
        }

        private IEnumerable<SyntaxTrivia> GetLeadingTriviaToKeep(SyntaxNode node)
        {
            return GetTriviaToKeep(node.GetLeadingTrivia().Reverse(), false).Reverse();
        }

        private IEnumerable<SyntaxTrivia> GetTriviaToKeep(IEnumerable<SyntaxTrivia> triviaList, bool returnsNonWhitespace)
        {
            foreach (var trivia in triviaList)
            {
                if (trivia.RawKind != WhitespaceTrivaKind)
                {
                    if (returnsNonWhitespace)
                    {
                        yield return trivia;
                        returnsNonWhitespace = false;   // Ensure only one non-whitespace trivia returned.
                    }
                }
                else
                    yield return trivia;
            }
        }

        public INamedTypeSymbol GetMessageResourceType()
        {
            var messageResourceAttribute = Compilation.GetKnownType(KnownTypes.MessageResourceAttribute);
            var attribute = Compilation.Assembly.GetAttributes().Where(x => messageResourceAttribute.Equals(x.AttributeClass)).FirstOrDefault();
            if (attribute == null)
                return null;
            var arguments = attribute.ConstructorArguments;
            return arguments.Length == 0 ? null : arguments[0].Value as INamedTypeSymbol;
        }

        public abstract bool IsKeyword(string name);

        public abstract bool IsValidIdentifier(string name);

        public string ValidateIdentifier(string s)
        {
            s = s?.Trim();
            return string.IsNullOrEmpty(s) || IsValidIdentifier(s) ? null : UserMessages.CodeMapper_InvalidIdentifier;
        }

        public abstract TextSpan? CodeSnippetInsertionSpan { get; }

        protected abstract string GetBackingFieldName(string propertyName);
    }
}
