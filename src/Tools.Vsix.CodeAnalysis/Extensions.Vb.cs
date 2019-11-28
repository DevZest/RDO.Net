using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static INamespaceSymbol GetNamespaceSymbol(this ImportsClauseSyntax importsClause, SemanticModel semanticModel)
        {
            if (importsClause is SimpleImportsClauseSyntax simpleImport)
            {
                var nameSymbol = simpleImport.Name;
                if (nameSymbol == null)
                    return null;

                var symbolInfo = semanticModel.GetSymbolInfo(nameSymbol);
                return symbolInfo.Symbol as INamespaceSymbol;
            }
            return null;
        }

        public static SyntaxNode VbGenerateAttribute(this SyntaxNode syntaxNode, string name, SyntaxTrivia? leadingWhitespaceTrivia, SyntaxNode[] arguments, SyntaxAnnotation argumentListAnnotation)
        {
            var leadingTrivia = syntaxNode.GetLeadingTrivia();
            var attribute = VbGetAttributeSyntax(name, arguments, argumentListAnnotation);
            var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new AttributeSyntax[] { attribute }))
                .NormalizeWhitespace()
                .WithTrailingTrivia(SyntaxFactory.EndOfLine(Environment.NewLine));

            if (syntaxNode is ClassBlockSyntax classBlock)
                return classBlock.WithClassStatement((ClassStatementSyntax)VbGenerateAttribute(classBlock.ClassStatement, name, leadingWhitespaceTrivia, arguments, argumentListAnnotation));

            if (syntaxNode is PropertyBlockSyntax propertyBlock)
                return propertyBlock.WithPropertyStatement((PropertyStatementSyntax)VbGenerateAttribute(propertyBlock.PropertyStatement, name, leadingWhitespaceTrivia, arguments, argumentListAnnotation));

            if (syntaxNode is MethodBlockSyntax methodBlock)
                return methodBlock.WithSubOrFunctionStatement((MethodStatementSyntax)VbGenerateAttribute(methodBlock.SubOrFunctionStatement, name, leadingWhitespaceTrivia, arguments, argumentListAnnotation));

            if (syntaxNode is PropertyStatementSyntax propertySyntax)
                return propertySyntax.WithLeadingWhitespaceTrivia(propertySyntax.AttributeLists, leadingWhitespaceTrivia)
                    .AddAttributeLists(attributeList.WithLeadingTrivia(propertySyntax.AttributeLists, leadingTrivia, leadingWhitespaceTrivia));
            else if (syntaxNode is ClassStatementSyntax classSyntax)
                return classSyntax.WithLeadingWhitespaceTrivia(classSyntax.AttributeLists, leadingWhitespaceTrivia)
                    .AddAttributeLists(attributeList.WithLeadingTrivia(classSyntax.AttributeLists, leadingTrivia, leadingWhitespaceTrivia));
            else if (syntaxNode is MethodStatementSyntax methodSyntax)
                return methodSyntax.WithLeadingWhitespaceTrivia(methodSyntax.AttributeLists, leadingWhitespaceTrivia)
                    .AddAttributeLists(attributeList.WithLeadingTrivia(methodSyntax.AttributeLists, leadingTrivia, leadingWhitespaceTrivia));
            else
            {
                Debug.Fail("Only property or class syntax node supported.");
                return null;
            }
        }

        private static AttributeSyntax VbGetAttributeSyntax(string name, SyntaxNode[] arguments, SyntaxAnnotation argumentListAnnotation)
        {
            var nameSyntax = SyntaxFactory.IdentifierName(name);

            if (arguments == null)
                return SyntaxFactory.Attribute(nameSyntax);

            if (arguments.Length == 0)
                return SyntaxFactory.Attribute(null, nameSyntax, SyntaxFactory.ArgumentList().AddAnnotationIfNotNull(argumentListAnnotation));

            var argumentList = new SeparatedSyntaxList<ArgumentSyntax>().AddRange(arguments.Select(x => (ArgumentSyntax)x));
            return SyntaxFactory.Attribute(null, nameSyntax, SyntaxFactory.ArgumentList(argumentList).AddAnnotationIfNotNull(argumentListAnnotation));
        }

        public static SyntaxNode GenerateUnderscoreNameConst(this SyntaxGenerator g, string name)
        {
            var type = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
            return g.FieldDeclaration("_" + name, type, Accessibility.Friend, DeclarationModifiers.Const, g.NameOfExpression(g.IdentifierName(name)));
        }

        public static async Task AddMissingNamespacesAsync(this DocumentEditor editor, NamespaceSet namespaces,
            Compilation compilation, INamespaceSymbol containingNamespace, SyntaxTree syntaxTree, SemanticModel semanticModel, CancellationToken ct)
        {
            namespaces = new NamespaceSet(namespaces); // Make a copy because namespaces is a input parameter

            namespaces.ExceptWith(containingNamespace.GetImplicitlyImportedNamespaces());
            if (namespaces.Count == 0)
                return;

            var globalImports = compilation.GetGlobalImports().ToArray();
            if (globalImports.Length > 0)
                namespaces.RemoveWhere(x => globalImports.Contains(x.ToDisplayString()));

            var importedNamespaces = ImportedNamespace.GetImportedNamespaces(syntaxTree, semanticModel);
            await editor.AddMissingNamespaces(namespaces, importedNamespaces, ImportedNamespace.GetAddFunc(semanticModel.Language), ct);
        }

        private static IEnumerable<INamespaceSymbol> GetImplicitlyImportedNamespaces(this INamespaceSymbol containingNamespace)
        {
            foreach (var result in containingNamespace.ConstituentNamespaces)
                yield return result;
        }

        private static IEnumerable<string> GetGlobalImports(this Compilation compilation)
        {
            if (compilation is VisualBasicCompilation visualBasicCompilation)
            {
                var options = visualBasicCompilation.Options;
                var globalImports = options.GlobalImports;
                foreach (var globalImport in globalImports)
                {
                    var importsClause = globalImport.Clause;
                    if (importsClause is SimpleImportsClauseSyntax simpleImport)
                    {
                        var name = simpleImport.Name;
                        if (name != null)
                            yield return name.ToString();
                    }
                }
            }
        }
    }
}
