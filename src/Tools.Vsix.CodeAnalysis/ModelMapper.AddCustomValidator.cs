using DevZest.Data.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        public Document AddCustomValidator(string name, string description)
        {
            return AddCustomValidatorAsync(name, description).Result;
        }

        private async Task<Document> AddCustomValidatorAsync(string name, string description, CancellationToken ct = default(CancellationToken))
        {
            var customValidatorAttribute = Compilation.GetKnownType(KnownTypes.CustomValidatorAttribute);
            var customValidatorImplAttribute = Compilation.GetKnownType(KnownTypes._CustomValidatorAttribute);
            var result = await AddCustomValidatorPropertyAsync(name, customValidatorAttribute, customValidatorImplAttribute, ct);
            return await GenerateModelAttributeAsync(result.Document, customValidatorAttribute, result.PropertyAnnotation, name, GenerateAdditionalArguments, ct);

            IEnumerable<SyntaxNode> GenerateAdditionalArguments(SyntaxGenerator g)
            {
                if (!string.IsNullOrWhiteSpace(description))
                    yield return g.AttributeArgument("Description", g.LiteralExpression(description));
            }
        }

        private async Task<(Document Document, SyntaxAnnotation PropertyAnnotation)> AddCustomValidatorPropertyAsync(string name,
            INamedTypeSymbol modelAttributeType, INamedTypeSymbol modelMemberAttributeType, CancellationToken ct)
        {
            var editor = await DocumentEditor.CreateAsync(Document, ct);
            var g = editor.Generator;
            var entryType = Compilation.GetKnownType(KnownTypes.CustomValidatorEntry);
            var imports = new NamespaceSet
            {
                modelAttributeType,
                modelMemberAttributeType,
                entryType
            };

            var property = g.PropertyDeclaration(name, g.IdentifierName(entryType.Name), Accessibility.Private, DeclarationModifiers.ReadOnly, 
                getAccessorStatements: GenerateCustomValidatorGetter(g));
            var propertyAnnotation = new SyntaxAnnotation();
            property = GenerateAttribute(property, modelMemberAttributeType.Name.ToAttributeName()).WithAdditionalAnnotations(Formatter.Annotation, propertyAnnotation);

            editor.AddMember(ModelClass, property);

            await AddMissingNamespaces(editor, imports, ct);

            var result = await editor.FormatAsync(ct);
            return (result, propertyAnnotation);
        }

        protected abstract IEnumerable<SyntaxNode> GenerateCustomValidatorGetter(SyntaxGenerator g);
    }
}
