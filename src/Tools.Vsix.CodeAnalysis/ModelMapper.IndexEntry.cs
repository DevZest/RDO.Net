using DevZest.Data.Annotations;
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
        public class IndexEntry : ColumnEntry
        {   
            static IndexEntry()
            {
                RegisterLocalColumn((IndexEntry _) => _.SortDirection);
            }

            [Required]
            [Display(Name = nameof(UserMessages.Display_IndexEntry_SortDirection), ResourceType = typeof(UserMessages))]
            public LocalColumn<SortDirection> SortDirection { get; private set; }
        }

        public DataSet<IndexEntry> CreateIndexEntries()
        {
            return CreateEntries<IndexEntry>();
        }

        public Document AddIndex(string name, string description, string dbName, bool isUnique, bool isValidOnTable, bool isValidOnTempTable, DataSet<IndexEntry> entries)
        {
            return AddIndexAsync(name, description, dbName, isUnique, isValidOnTable, isValidOnTempTable, entries).Result;
        }

        private async Task<Document> AddIndexAsync(string name, string description, string dbName, bool isUnique, bool isValidOnTable, bool isValidOnTempTable,
            DataSet<IndexEntry> entries, CancellationToken ct = default(CancellationToken))
        {
            var dbIndexAttribute = Compilation.GetKnownType(KnownTypes.DbIndexAttribute);
            var _dbIndexAttribute = Compilation.GetKnownType(KnownTypes._DbIndexAttribute);
            var result = await AddIndexPropertyAsync(name, entries, dbIndexAttribute, _dbIndexAttribute, ct);
            return await GenerateModelAttributeAsync(result.Document, dbIndexAttribute, result.PropertyAnnotation, name, GenerateAdditionalArguments, ct);

            IEnumerable<SyntaxNode> GenerateAdditionalArguments(SyntaxGenerator g)
            {
                if (!string.IsNullOrWhiteSpace(description))
                    yield return g.AttributeArgument("Description", g.LiteralExpression(description));
                if (!string.IsNullOrWhiteSpace(dbName))
                    yield return g.AttributeArgument("DbName", g.LiteralExpression(dbName));
                if (isUnique)
                    yield return g.AttributeArgument("IsUnique", g.LiteralExpression(true));
                if (!isValidOnTable)
                    yield return g.AttributeArgument("IsValidOnTable", g.LiteralExpression(false));
                if (isValidOnTempTable)
                    yield return g.AttributeArgument("IsValidOnTempTable", g.LiteralExpression(true));
            }
        }

        public Document AddUniqueConstraint(string name, string description, string dbName, INamedTypeSymbol resourceType, IPropertySymbol resourceProperty, string message, DataSet<IndexEntry> entries)
        {
            return AddUniqueConstraintAsync(name, description, dbName, resourceType, resourceProperty, message, entries).Result;
        }

        private async Task<Document> AddUniqueConstraintAsync(string name, string description, string dbName, INamedTypeSymbol resourceType, IPropertySymbol resourceProperty,
            string message, DataSet<IndexEntry> entries, CancellationToken ct = default(CancellationToken))
        {
            var uniqueConstraintAttribute = Compilation.GetKnownType(KnownTypes.UniqueConstraintAttribute);
            var _uniqueConstraintAttribute = Compilation.GetKnownType(KnownTypes._UniqueConstraintAttribute);
            var result = await AddIndexPropertyAsync(name, entries, uniqueConstraintAttribute, _uniqueConstraintAttribute, ct, resourceType);
            return await GenerateModelAttributeAsync(result.Document, uniqueConstraintAttribute, result.PropertyAnnotation, name, GenerateAdditionalArguments, ct);

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

                if (!string.IsNullOrWhiteSpace(dbName))
                    yield return g.AttributeArgument("DbName", g.LiteralExpression(dbName));
            }
        }

        private async Task<(Document Document, SyntaxAnnotation PropertyAnnotation)> AddIndexPropertyAsync(string name, DataSet<IndexEntry> entries,
            INamedTypeSymbol modelAttributeType, INamedTypeSymbol modelMemberAttributeType, CancellationToken ct = default(CancellationToken), params INamedTypeSymbol[] typesToImport)
        {
            var editor = await DocumentEditor.CreateAsync(Document, ct);
            var g = editor.Generator;
            var columnSortType = Compilation.GetKnownType(KnownTypes.ColumnSort);
            var imports = new NamespaceSet
            {
                modelAttributeType,
                modelMemberAttributeType,
                columnSortType
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

            var propertyGetter = g.ReturnStatement(g.ArrayCreationExpression(g.IdentifierName(columnSortType.Name), GenerateColumnSortElements(g, entries)));
            var property = g.PropertyDeclaration(name, g.ArrayOf(columnSortType), Accessibility.Private, DeclarationModifiers.ReadOnly, getAccessorStatements: new SyntaxNode[] { propertyGetter });
            var propertyAnnotation = new SyntaxAnnotation();
            property = GenerateAttribute(property, modelMemberAttributeType.Name.ToAttributeName()).WithAdditionalAnnotations(Formatter.Annotation, propertyAnnotation);

            editor.AddMember(ModelClass, property);

            await AddMissingNamespaces(editor, imports, ct);

            var result = await editor.FormatAsync(ct);
            return (result, propertyAnnotation);
        }

        private static SyntaxNode[] GenerateColumnSortElements(SyntaxGenerator g, DataSet<IndexEntry> entries)
        {
            var _ = entries._;
            var result = new SyntaxNode[entries.Count];
            for (int i = 0; i < result.Length; i++)
            {
                SyntaxNode expression = g.IdentifierName(_.Column[i].Name);
                var sortDirection = _.SortDirection[i];
                if (sortDirection == SortDirection.Ascending)
                    expression = g.InvocationExpression(g.MemberAccessExpression(expression, "Asc"));
                else if (sortDirection == SortDirection.Descending)
                    expression = g.InvocationExpression(g.MemberAccessExpression(expression, "Desc"));
                result[i] = expression;
            }

            return result;
        }
    }
}
