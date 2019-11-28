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
        public Document AddComputation(string name, string description, ComputationMode? mode)
        {
            return AddComputationAsync(name, description, mode).Result;
        }

        private async Task<Document> AddComputationAsync(string name, string description, ComputationMode? mode, CancellationToken ct = default(CancellationToken))
        {
            var computationAttribute = Compilation.GetKnownType(KnownTypes.ComputationAttribute);
            var _computationAttribute = Compilation.GetKnownType(KnownTypes._ComputationAttribute);
            var computationMode = mode.HasValue ? Compilation.GetKnownType(KnownTypes.ComputationMode) : null;
            var result = await AddComputationMethodAsync(name, computationAttribute, _computationAttribute, ct, computationMode);
            return await GenerateModelAttributeAsync(result.Document, computationAttribute, result.MethodAnnotation, name, GenerateAdditionalArguments, ct);

            IEnumerable<SyntaxNode> GenerateAdditionalArguments(SyntaxGenerator g)
            {
                if (mode.HasValue)
                    yield return g.AttributeArgument(g.DottedName(string.Format("{0}.{1}", computationMode.Name, mode.Value)));

                if (!string.IsNullOrWhiteSpace(description))
                    yield return g.AttributeArgument("Description", g.LiteralExpression(description));
            }
        }

        private async Task<(Document Document, SyntaxAnnotation MethodAnnotation)> AddComputationMethodAsync(string name,
            INamedTypeSymbol modelAttributeType, INamedTypeSymbol modelMemberAttributeType, CancellationToken ct = default(CancellationToken), params INamedTypeSymbol[] typesToImport)
        {
            var editor = await DocumentEditor.CreateAsync(Document, ct);
            var g = editor.Generator;
            var voidType = Compilation.GetSpecialType(SpecialType.System_Void);
            var imports = new NamespaceSet
            {
                modelAttributeType,
                modelMemberAttributeType
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

            var methodBody = g.GenerateNotImplemented(Compilation);
            var method = g.MethodDeclaration(name, null, null, null, Accessibility.Private, default(DeclarationModifiers), statements: new SyntaxNode[] { methodBody });
            var methodAnnotation = new SyntaxAnnotation();
            method = GenerateAttribute(method, modelMemberAttributeType.Name.ToAttributeName()).WithAdditionalAnnotations(Formatter.Annotation, methodAnnotation);

            editor.AddMember(ModelClass, method);

            await AddMissingNamespaces(editor, imports, ct);

            var result = await editor.FormatAsync(ct);
            return (result, methodAnnotation);
        }
    }
}
