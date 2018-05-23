// Code from: https://github.com/Wintellect/Wintellect.Analyzers/blob/master/Source/Wintellect.Analyzers/Wintellect.Analyzers/Helpers/SyntaxFactoryHelper.cs
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

/// <summary>
/// A SyntaxFactory helper class that combines numerous tedious operations.
/// </summary>
static class SyntaxFactoryHelper
{
    /// <summary>
    /// Creates an AttributeSyntax from the attribute name string and parameter string.
    /// </summary>
    /// <param name="attributeName">
    /// The string name of the attribute to use.
    /// </param>
    /// <param name="attributeParameters">
    /// The string comprising the parameters for the attribute as though it were typed in the 
    /// language of choice. If the open and close parameters are not specified this method will
    /// add them.
    /// </param>
    /// <returns>
    /// The AttributeSyntax from the specified parameters.
    /// </returns>
    public static AttributeSyntax Attribute(string attributeName, string attributeParameters)
    {
        var attrName = SyntaxFactory.ParseName(attributeName);

        // Put on the parenthesis if necessary.
        if (attributeParameters[0] != '(')
            attributeParameters = "(" + attributeParameters;

        if (attributeParameters[attributeParameters.Length - 1] != ')')
            attributeParameters += ")";

        AttributeArgumentListSyntax args = SyntaxFactory.ParseAttributeArgumentList(attributeParameters);
        return SyntaxFactory.Attribute(attrName, args);
    }

    /// <summary>
    /// A simple wrapper to create a QuailifiedNameSyntax from two strings.
    /// </summary>
    /// <param name="left">
    /// The left part of the name.
    /// </param>
    /// <param name="right">
    /// The right part of the name.
    /// </param>
    /// <returns>
    /// The built up name as a QualifiedNameSyntax
    /// </returns>
    public static QualifiedNameSyntax QualifiedName(string left, string right)
    {
        return SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName(left), SyntaxFactory.IdentifierName(right));
    }

    /// <summary>
    /// Takes a normal using statement string and builds up a UsingDirectiveSyntax. 
    /// </summary>
    /// <param name="statement">
    /// The assembly name reference in text form, such as "System.Diagnostics" or "Microsoft.CodeAnalysis.CSharp.Syntax". 
    /// </param>
    /// <returns>
    /// The built up UsingDirectiveSyntax.
    /// </returns>
    public static UsingDirectiveSyntax QualifiedUsing(string statement)
    {
        var parts = statement.Split('.');
        var count = parts.Count();

        // A single name is easy.
        if (count == 1)
            return SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(parts[0]));

        // Two part names are easy.
        if (count == 2)
            return SyntaxFactory.UsingDirective(QualifiedName(parts[0], parts[1]));

        // Seed the left side.
        QualifiedNameSyntax left = QualifiedName(parts[0], parts[1]);
        for (var i = 2; i < count - 1; i++)
        {
            IdentifierNameSyntax right = SyntaxFactory.IdentifierName(parts[i]);
            left = SyntaxFactory.QualifiedName(left, right);
        }

        QualifiedNameSyntax final = SyntaxFactory.QualifiedName(left, SyntaxFactory.IdentifierName(parts[count - 1]));
        return SyntaxFactory.UsingDirective(final);
    }
}
