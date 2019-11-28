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
        public class ProjectionEntry : ColumnEntry
        {   
            static ProjectionEntry()
            {
                RegisterLocalColumn((ProjectionEntry _) => _.Mounter);
            }

            public ProjectionEntry()
            {
                Mounter.ComputedAs(Column, GetMounter, true);
            }

            private static IFieldSymbol GetMounter(DataRow dataRow, Column<IPropertySymbol> column)
            {
                var modelMapper = ((ProjectionEntry)dataRow.Model)?.ModelMapper;
                return modelMapper?.GetMounter(column[dataRow]);
            }

            [Required]
            [Display(Name = nameof(UserMessages.Display_ProjectionEntry_Mounter), ResourceType = typeof(UserMessages))]
            public LocalColumn<IFieldSymbol> Mounter { get; private set; }
        }

        public DataSet<ProjectionEntry> CreateProjectionEntries(bool fillColumns = true)
        {
            var result = CreateEntries<ProjectionEntry>();
            if (!fillColumns)
                return result;

            var _ = result._;
            foreach (var column in GetColumns())
            {
                var dataRow = result.AddRow();
                _.Column[dataRow] = column;

            }
            return result;
        }

        public Document AddProjection(string typeName, DataSet<ProjectionEntry> entries)
        {
            return AddProjectionAsync(typeName, entries, CancellationToken.None).Result;
        }

        public async Task<Document> AddProjectionAsync(string typeName, DataSet<ProjectionEntry> entries, CancellationToken ct)
        {
            var editor = await DocumentEditor.CreateAsync(Document, ct);
            var g = editor.Generator;
            var imports = new NamespaceSet();
            var projectionClass = GenerateProjectionClass(g, imports, typeName, entries).WithAdditionalAnnotations(Formatter.Annotation);
            var lastNodeToAddProjection = GetLastNodeToAddProjection();
            if (lastNodeToAddProjection != null)
                editor.InsertAfter(lastNodeToAddProjection, projectionClass);
            else
                editor.InsertMembers(ModelClass, 0, new SyntaxNode[] { projectionClass });
            await AddMissingNamespaces(editor, imports, ct);
            return await editor.FormatAsync(ct);
        }

        private SyntaxNode GenerateProjectionClass(SyntaxGenerator g, NamespaceSet imports, string className, DataSet<ProjectionEntry> entries)
        {
            imports.Add(Compilation.GetKnownType(KnownTypes.Projection));
            var baseType = g.IdentifierName("Projection");
            var constructor = GenerateStaticConstructorForColumnRegistration(g, imports, Language, ModelType, className, entries);
            var properties = GenerateColumnProperties(g, imports, entries);

            return g.ClassDeclaration(className, accessibility: Accessibility.Public, baseType: baseType, members: constructor.Concat(null, properties));
        }
    }
}
