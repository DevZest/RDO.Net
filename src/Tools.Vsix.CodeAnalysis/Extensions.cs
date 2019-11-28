using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.CodeAnalysis
{
    internal static partial class Extensions
    {
        public static SyntaxNode GenerateMounterDeclaration(this SyntaxGenerator g, string language, INamedTypeSymbol typeSymbol, IPropertySymbol propertySymbol, string registerPropertyMethodName)
        {
            var mounterName = propertySymbol.GetMounterName();
            var propertyTypeName = propertySymbol.Type.Name;

            return g.FieldDeclaration(mounterName, g.GenericName("Mounter", propertySymbol.Type), Accessibility.Public, DeclarationModifiers.Static | DeclarationModifiers.ReadOnly,
                g.InvocationExpression(language, registerPropertyMethodName, typeSymbol, propertySymbol.Name));
        }

        public static SyntaxNode GeneratePropertyRegistration(this SyntaxGenerator g, string language, INamedTypeSymbol classSymbol, IPropertySymbol propertySymbol,
            string registerPropertyMethodName, bool returnsMounter, bool hasStaticConstructor)
        {
            var propertyName = propertySymbol.Name;
            var mounterName = returnsMounter ? propertySymbol.GetMounterName() : null;

            if (hasStaticConstructor)
                return g.AssignmentOrInvocationStatement(language, classSymbol, propertyName, registerPropertyMethodName, mounterName);
            else
                return g.ConstructorDeclaration(containingTypeName: classSymbol.Name, modifiers: DeclarationModifiers.Static,
                    statements: new SyntaxNode[] { g.AssignmentOrInvocationStatement(language, classSymbol, propertyName, registerPropertyMethodName, mounterName) });
        }

        private static SyntaxNode AssignmentOrInvocationStatement(this SyntaxGenerator g, string language, INamedTypeSymbol typeSymbol, string propertyName, string registerPropertyMethodName, string mounterName)
        {
            if (string.IsNullOrEmpty(mounterName))
                return g.InvocationStatement(language, registerPropertyMethodName, typeSymbol, propertyName);
            else
                return g.AssignmentStatement(language, mounterName, registerPropertyMethodName, typeSymbol, propertyName);
        }

        private static SyntaxNode AssignmentStatement(this SyntaxGenerator g, string language, string mounterName, string methodName, INamedTypeSymbol typeSymbol, string propertyName)
        {
            return g.ExpressionStatement(g.AssignmentStatement(g.IdentifierName(mounterName), g.InvocationExpression(language, methodName, typeSymbol, propertyName)));
        }

        private static SyntaxNode InvocationStatement(this SyntaxGenerator g, string language, string methodName, INamedTypeSymbol typeSymbol, string propertyName)
        {
            return g.ExpressionStatement(g.InvocationExpression(language, methodName, typeSymbol, propertyName));
        }

        private static SyntaxNode InvocationExpression(this SyntaxGenerator g, string language, string methodName, INamedTypeSymbol typeSymbol, string propertyName)
        {
            return g.InvocationExpression(g.IdentifierName(methodName), g.GetterArgument(language, typeSymbol, propertyName));
        }

        private static SyntaxNode GetterArgument(this SyntaxGenerator g, string language, ITypeSymbol typeSymbol, string propertyName)
        {
            return g.Argument(g.Getter(language, typeSymbol, propertyName));
        }

        private static SyntaxNode Getter(this SyntaxGenerator g, string language, ITypeSymbol typeSymbol, string propertyName)
        {
            return g.ValueReturningLambdaExpression(new SyntaxNode[] { g.GetterParameter(language, typeSymbol) }, g.GetterBody(language, propertyName));
        }

        private static SyntaxNode GetterParameter(this SyntaxGenerator g, string language, ITypeSymbol typeSymbol)
        {
            var typeName = typeSymbol.Name;
            var type = typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType
                ? g.GenericName(typeName, namedTypeSymbol.TypeArguments) : g.IdentifierName(typeName);

            return g.ParameterDeclaration(g.GetterLambdaParamName(language), type);
        }

        public static SyntaxNode GetterBody(this SyntaxGenerator g, string language, string propertyName)
        {
            return g.MemberAccessExpression(g.IdentifierName(g.GetterLambdaParamName(language)), propertyName);
        }

        public static string GetterLambdaParamName(this SyntaxGenerator g, string language)
        {
            if (language == LanguageNames.CSharp)
                return "_";
            else if (language == LanguageNames.VisualBasic)
                return "x";
            else
                throw new ArgumentException("Invalid language.", nameof(language));
        }

        public static SyntaxNode GenerateChildModelRegistration(this SyntaxGenerator g, string language, IPropertySymbol childProperty, IPropertySymbol foreignKey)
        {
            var invocationExpression = g.InvocationExpression(g.IdentifierName("RegisterChildModel"),
                g.GetterArgument(language, childProperty.ContainingType, childProperty.Name),
                g.GetterArgument(language, childProperty.Type, foreignKey.Name));
            return g.ExpressionStatement(invocationExpression);
        }

        public static SyntaxNode GenerateChildModelRegistrationStaticConstructor(this SyntaxGenerator g, string language, IPropertySymbol childProperty, IPropertySymbol foreignKey)
        {
            return g.ConstructorDeclaration(containingTypeName: childProperty.ContainingType.Name, modifiers: DeclarationModifiers.Static,
                    statements: new SyntaxNode[] { g.GenerateChildModelRegistration(language, childProperty, foreignKey) });
        }

        private static string GetMounterName(this IPropertySymbol propertySymbol)
        {
            return "_" + propertySymbol.Name;
        }

        public static T FirstAncestor<T>(this SyntaxNode node)
            where T : SyntaxNode
        {
            for (var current = node.Parent; current != null; current = current.Parent)
            {
                if (current is T result)
                    return result;
            }

            return null;
        }

        public static string ToCamelCase(this string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
                return s;

            char[] chars = s.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                    break;

                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    // if the next character is a space, which is not considered uppercase 
                    // (otherwise we wouldn't be here...)
                    // we want to ensure that the following:
                    // 'FOO bar' is rewritten as 'foo bar', and not as 'foO bar'
                    // The code was written in such a way that the first word in uppercase
                    // ends when if finds an uppercase letter followed by a lowercase letter.
                    // now a ' ' (space, (char)32) is considered not upper
                    // but in that case we still want our current character to become lowercase
                    if (char.IsSeparator(chars[i + 1]))
                        chars[i] = ToLower(chars[i]);

                    break;
                }

                chars[i] = ToLower(chars[i]);
            }

            return new string(chars);
        }

        private static char ToLower(char c)
        {
            return char.ToLowerInvariant(c);
        }

        public static bool IsColumn(this IPropertySymbol x, Compilation compilation)
        {
            return x.Type.IsDerivedFrom(KnownTypes.Column, compilation) && !x.IsStatic && !x.IsIndexer && x.GetMethod != null;
        }

        public static async Task<Document> FormatAsync(this DocumentEditor editor, CancellationToken ct = default(CancellationToken))
        {
            return await Formatter.FormatAsync(editor.GetChangedDocument(), Formatter.Annotation, null, ct);
        }

        public static void Add(this NamespaceSet namespaces, ITypeSymbol type)
        {
            var containingNamespace = type.ContainingNamespace;
            if (containingNamespace != null)
                namespaces.Add(containingNamespace);
        }

        public static async Task AddMissingNamespaces(this DocumentEditor editor, NamespaceSet namespaces, IReadOnlyList<ImportedNamespace> importedNamespaces,
            Func<SyntaxNode, IEnumerable<SyntaxNode>, SyntaxNode> addUsings, CancellationToken ct)
        {
            var orderedNamespaces = namespaces.OrderBy((x => x.ToDisplayString())).ToArray();
            if (orderedNamespaces.Length == 0)
                return;

            var g = editor.Generator;
            if (importedNamespaces == null || importedNamespaces.Count == 0)
            {
                var root = await editor.GetChangedDocument().GetSyntaxRootAsync(ct);
                var newRoot = addUsings(root, orderedNamespaces.Select(x => g.GenerateNamespaceImportDeclaration(x)));
                editor.ReplaceNode(root, newRoot);
                return;
            }

            for (int i = 0; i < orderedNamespaces.Length; i++)
            {
                var namespaceSymbol = orderedNamespaces[i];
                var index = namespaceSymbol.GetIndexToInsertBefore(importedNamespaces);
                if (index < 0)
                    continue;

                if (index == importedNamespaces.Count)
                {
                    var lastNode = importedNamespaces.LastOrDefault().SyntaxNode;
                    Append(editor, lastNode, orderedNamespaces, i);
                    return;
                }

                var newNode = g.GenerateNamespaceImportDeclaration(namespaceSymbol);
                editor.InsertBefore(importedNamespaces[index].SyntaxNode, newNode);
            }
        }

        private static void Append(DocumentEditor editor, SyntaxNode lastNode, INamespaceSymbol[] namespaces, int startIndex)
        {
            var g = editor.Generator;
            for (int i = namespaces.Length - 1; i >= startIndex; i--)
            {
                var namespaceSymbol = namespaces[i];
                var newNode = g.GenerateNamespaceImportDeclaration(namespaceSymbol);
                editor.InsertAfter(lastNode, newNode);
            }
        }

        private static SyntaxNode GenerateNamespaceImportDeclaration(this SyntaxGenerator g, INamespaceSymbol namespceSymbol)
        {
            var name = namespceSymbol.ToDisplayString();
            return g.NamespaceImportDeclaration(name).WithAdditionalAnnotations(Formatter.Annotation);
        }

        private static int GetIndexToInsertBefore(this INamespaceSymbol namespaceSymbol, IReadOnlyList<ImportedNamespace> importedNamespaces)
        {
            for (int i = 0; i < importedNamespaces.Count; i++)
            {
                var importedNamespaceSymbol = importedNamespaces[i].NamespaceSymbol;
                var compareResult = string.Compare(namespaceSymbol.ToDisplayString(), importedNamespaceSymbol.ToDisplayString());
                if (compareResult == 0)
                    return -1;

                if (compareResult < 0)
                    return i;
            }

            return importedNamespaces.Count;
        }

        public static string ToAttributeName(this string name)
        {
            return name.EndsWith(nameof(KnownTypes.Attribute)) ? name.Substring(0, name.Length - nameof(KnownTypes.Attribute).Length) : name;
        }

        private static T WithLeadingWhitespaceTrivia<T, TAttributeList>(this T syntaxNode, SyntaxList<TAttributeList> attributeLists, SyntaxTrivia? leadingWhitespaceTrivia)
            where T : SyntaxNode
            where TAttributeList : SyntaxNode
        {
            return attributeLists.Count > 0 ? syntaxNode : syntaxNode.WithLeadingWhitespaceTrivia(leadingWhitespaceTrivia);
        }

        private static T WithLeadingTrivia<T>(this T attributeList, SyntaxList<T> attributeLists, SyntaxTriviaList leadingTrivia, SyntaxTrivia? leadingWhitespaceTrivia)
            where T : SyntaxNode
        {
            return attributeLists.Count == 0 ? attributeList.WithLeadingTrivia(leadingTrivia) : attributeList.WithLeadingWhitespaceTrivia(leadingWhitespaceTrivia);
        }

        private static T WithLeadingWhitespaceTrivia<T>(this T syntaxNode, SyntaxTrivia? leadingWhitespaceTrivia)
            where T : SyntaxNode
        {
            return leadingWhitespaceTrivia.HasValue ? syntaxNode.WithLeadingTrivia(leadingWhitespaceTrivia.Value) : syntaxNode.WithoutLeadingTrivia();
        }

        public static SyntaxNode ArrayOf(this SyntaxGenerator g, INamedTypeSymbol type)
        {
            return g.ArrayTypeExpression(g.IdentifierName(type.Name));
        }

        public static T AddAnnotationIfNotNull<T>(this T syntaxNode, SyntaxAnnotation annotation)
            where T : SyntaxNode
        {
            return annotation == null ? syntaxNode : syntaxNode.WithAdditionalAnnotations(annotation);
        }

        public static ISymbol GetImplementationSymbol(this AttributeData attribute, INamedTypeSymbol type, string name, Compilation compilation)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var crossRefAttributeType = GetCrossReferenceAttributeType(attribute, compilation);
            if (crossRefAttributeType == null)
                return null;

            return type.GetMembers(name).Where(x => HasAttribute(x, crossRefAttributeType)).FirstOrDefault();
        }

        public static IEnumerable<T> Concat<T>(this T node, IEnumerable<T> nodes)
            where T : class
        {
            return Concat(node, null, nodes);
        }

        public static IEnumerable<T> Concat<T>(this T node, T optionalNode, IEnumerable<T> nodes)
            where T : class
        {
            yield return node;
            if (optionalNode != null)
                yield return optionalNode;
            if (nodes != null)
            {
                foreach (var property in nodes)
                    yield return property;
            }
        }

        public static SyntaxNode GenerateNotImplemented(this SyntaxGenerator g, Compilation compilation)
        {
            return g.ThrowStatement(g.ObjectCreationExpression(compilation.GetKnownType(KnownTypes.NotImplementedException)));
        }

        public static bool IsForeignKey(this IPropertySymbol propertySymbol, Compilation compilation)
        {
            return propertySymbol.SetMethod == null && propertySymbol.Type.IsDerivedFrom(KnownTypes.CandidateKey, compilation) && propertySymbol.Name != nameof(Model.PrimaryKey);
        }

        public static bool TryGetDbInitCompilation(this Project project, out Compilation compilation)
        {
            var result = project.TryGetCompilation(out compilation) && compilation.GetKnownType(KnownTypes.DbSessionProviderOf) != null;
            if (!result)
                compilation = null;
            return result;
        }

        public static bool GetIsEmptyDb(this INamedTypeSymbol DbSessionProviderType, Compilation compilation)
        {
            var attribute = compilation.GetKnownType(KnownTypes.EmptyDbAttribute);
            var attributeData = DbSessionProviderType.GetAttributes().FirstOrDefault(x => attribute.Equals(x.AttributeClass));
            return attributeData != null;
        }

        public static INamedTypeSymbol GetPkType(this INamedTypeSymbol type, Compilation compilation)
        {
            return type.GetArgumentType(compilation.GetKnownType(KnownTypes.ModelOf), compilation);
        }

        public static INamedTypeSymbol GetNamedTypeSymbol(this Project project, INamedTypeSymbol namedTypeSymbol)
        {
            return project.TryGetCompilation(out var compilation) ? compilation.GetTypeByMetadataName(namedTypeSymbol.GetFullyQualifiedMetadataName()) : null;
        }

        public static IEnumerable<T> GetTypeMembers<T>(this INamedTypeSymbol type, Func<T, bool> predicate)
        {
            if (type == null)
                yield break;

            foreach (var member in type.GetMembers().OfType<T>())
            {
                if (predicate(member))
                    yield return member;
            }

            foreach (var result in GetTypeMembers(type.BaseType, predicate))
                yield return result;
        }

        public static T GetNamedArgument<T>(this AttributeData attribute, string key, T defaultValue)
        {
            var namedArguments = attribute.NamedArguments;
            for (int i = 0; i < namedArguments.Length; i++)
            {
                var namedArgument = namedArguments[i];
                if (namedArgument.Key == key)
                {
                    var argument = namedArgument.Value;
                    var value = argument.Value;
                    if (argument.Value is T)
                        return (T)value;
                }
            }

            return defaultValue;
        }
    }
}
