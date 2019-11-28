using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static SyntaxNode CsGenerateAttribute(this SyntaxNode syntaxNode, string name, SyntaxTrivia? leadingWhitespaceTrivia, SyntaxNode[] arguments, SyntaxAnnotation argumentListAnnotation)
        {
            var leadingTrivia = syntaxNode.GetLeadingTrivia();
            var attribute = CsGetAttributeSyntax(name, arguments, argumentListAnnotation);
            var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new AttributeSyntax[] { attribute }))
                .NormalizeWhitespace()
                .WithTrailingTrivia(SyntaxFactory.EndOfLine(Environment.NewLine));

            if (syntaxNode is PropertyDeclarationSyntax propertySyntax)
                return propertySyntax.WithLeadingWhitespaceTrivia(propertySyntax.AttributeLists, leadingWhitespaceTrivia)
                    .AddAttributeLists(attributeList.WithLeadingTrivia(propertySyntax.AttributeLists, leadingTrivia, leadingWhitespaceTrivia));
            else if (syntaxNode is ClassDeclarationSyntax classSyntax)
                return classSyntax.WithLeadingWhitespaceTrivia(classSyntax.AttributeLists, leadingWhitespaceTrivia)
                    .AddAttributeLists(attributeList.WithLeadingTrivia(classSyntax.AttributeLists, leadingTrivia, leadingWhitespaceTrivia));
            else if (syntaxNode is MethodDeclarationSyntax methodSyntax)
                return methodSyntax.WithLeadingWhitespaceTrivia(methodSyntax.AttributeLists, leadingWhitespaceTrivia)
                    .AddAttributeLists(attributeList.WithLeadingTrivia(methodSyntax.AttributeLists, leadingTrivia, leadingWhitespaceTrivia));
            else
            {
                Debug.Fail("Only property or class syntax node supported.");
                return null;
            }
        }

        private static AttributeSyntax CsGetAttributeSyntax(string name, SyntaxNode[] arguments, SyntaxAnnotation argumentListAnnotation)
        {
            var nameSyntax = SyntaxFactory.IdentifierName(name);

            if (arguments == null)
                return SyntaxFactory.Attribute(nameSyntax);

            if (arguments.Length == 0)
                return SyntaxFactory.Attribute(nameSyntax, SyntaxFactory.AttributeArgumentList().AddAnnotationIfNotNull(argumentListAnnotation));

            var argumentList = new SeparatedSyntaxList<AttributeArgumentSyntax>().AddRange(arguments.Select(x => (AttributeArgumentSyntax)x));
            return SyntaxFactory.Attribute(nameSyntax, SyntaxFactory.AttributeArgumentList(argumentList).AddAnnotationIfNotNull(argumentListAnnotation));
        }
    }
}
