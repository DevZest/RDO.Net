using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        public Document AddCheckConstraint(string name, string description, INamedTypeSymbol resourceType, IPropertySymbol resourceProperty, string message)
        {
            return AddCheckConstraintAsync(name, description, resourceType, resourceProperty, message).Result;
        }

        private async Task<Document> AddCheckConstraintAsync(string name, string description, INamedTypeSymbol resourceType, IPropertySymbol resourceProperty,
            string message, CancellationToken ct = default(CancellationToken))
        {
            var checkConstraintAttribute = Compilation.GetKnownType(KnownTypes.CheckConstraintAttribute);
            var _checkConstraintAttribute = Compilation.GetKnownType(KnownTypes._CheckConstraintAttribute);
            var result = await AddCheckPropertyAsync(name, checkConstraintAttribute, _checkConstraintAttribute, ct, resourceType);
            return await GenerateModelAttributeAsync(result.Document, checkConstraintAttribute, result.PropertyAnnotation, name, GenerateAdditionalArguments, ct);

            IEnumerable<SyntaxNode> GenerateAdditionalArguments(SyntaxGenerator g)
            {
                if (resourceType != null)
                {
                    Debug.Assert(resourceProperty != null);
                    yield return g.AttributeArgument(g.TypeOfExpression(g.IdentifierName(resourceType.Name)));
                    yield return g.AttributeArgument(g.NameOfExpression(g.DottedName(string.Format("{0}.{1}", resourceType.Name, resourceProperty.Name))));
                }
                else if (!string.IsNullOrWhiteSpace(message))
                    yield return g.AttributeArgument(g.LiteralExpression(message));

                if (!string.IsNullOrWhiteSpace(description))
                    yield return g.AttributeArgument("Description", g.LiteralExpression(description));
            }
        }

        private async Task<(Document Document, SyntaxAnnotation PropertyAnnotation)> AddCheckPropertyAsync(string name,
            INamedTypeSymbol modelAttributeType, INamedTypeSymbol modelMemberAttributeType, CancellationToken ct = default(CancellationToken), params INamedTypeSymbol[] typesToImport)
        {
            var editor = await DocumentEditor.CreateAsync(Document, ct);
            var g = editor.Generator;
            var booleanType = Compilation.GetKnownType(KnownTypes._Boolean);
            var imports = new NamespaceSet
            {
                modelAttributeType,
                modelMemberAttributeType,
                booleanType
            };
            if (typesToImport != null && typesToImport.Length > 0)
            {
                for (int i = 0; i < typesToImport.Length; i++)
                {
                    var typeToImport = typesToImport[i];
                    if (typeToImport != null)
                        imports.Add(typeToImport);
                }
            }

            var propertyGetter = g.GenerateNotImplemented(Compilation);
            var property = g.PropertyDeclaration(name, g.IdentifierName(booleanType.Name), Accessibility.Private, DeclarationModifiers.ReadOnly, getAccessorStatements: new SyntaxNode[] { propertyGetter });
            var propertyAnnotation = new SyntaxAnnotation();
            property = GenerateAttribute(property, modelMemberAttributeType.Name.ToAttributeName()).WithAdditionalAnnotations(Formatter.Annotation, propertyAnnotation);

            editor.AddMember(ModelClass, property);

            await AddMissingNamespaces(editor, imports, ct);

            var result = await editor.FormatAsync(ct);
            return (result, propertyAnnotation);
        }
    }
}
