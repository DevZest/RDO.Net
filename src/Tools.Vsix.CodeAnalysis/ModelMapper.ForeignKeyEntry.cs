using DevZest.Data.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        public sealed class ForeignKeyEntry : ColumnEntry
        {
            static ForeignKeyEntry()
            {
                RegisterLocalColumn((ForeignKeyEntry _) => _.Parameter);
                RegisterLocalColumn((ForeignKeyEntry _) => _.ColumnSelection);
            }

            [Required]
            [Display(Name = nameof(UserMessages.Display_ForeignKeyEntry_Parameter), ResourceType = typeof(UserMessages))]
            public LocalColumn<IParameterSymbol> Parameter { get; private set; }

            public LocalColumn<IEnumerable> ColumnSelection { get; private set; }
        }

        public DataSet<ForeignKeyEntry> CreateForeignKeyEntries()
        {
            return CreateEntries<ForeignKeyEntry>();
        }

        private sealed class KeyTypeVisitor : SymbolVisitor
        {
            public static List<INamedTypeSymbol> Walk(Compilation compilation)
            {
                return new KeyTypeVisitor(compilation)._result;
            }

            private KeyTypeVisitor(Compilation compilation)
            {
                _compilation = compilation;
                _keyType = compilation.GetKnownType(KnownTypes.CandidateKey);
                Debug.Assert(_keyType != null);

                Visit(compilation.Assembly.GlobalNamespace);
                foreach (var reference in compilation.ExternalReferences)
                {
                    var symbol = compilation.GetAssemblyOrModuleSymbol(reference);
                    if (symbol is IAssemblySymbol assembly)
                    {
                        if (!assembly.Name.StartsWith("System.") && assembly.Name != "DevZest.Data")
                            Visit(assembly.GlobalNamespace);
                    }
                }
            }

            private readonly Compilation _compilation;
            private readonly INamedTypeSymbol _keyType;
            private List<INamedTypeSymbol> _result = new List<INamedTypeSymbol>();

            public override void VisitNamespace(INamespaceSymbol symbol)
            {
                foreach (var member in symbol.GetMembers())
                    member.Accept(this);
            }

            public override void VisitNamedType(INamedTypeSymbol symbol)
            {
                if (symbol.IsDerivedFrom(_keyType))
                    _result.Add(symbol);

                foreach (var member in symbol.GetTypeMembers())
                    member.Accept(this);
            }
        }

        public IEnumerable GetKeyTypeSelection()
        {
            return KeyTypeVisitor.Walk(Compilation).Where(x => !x.Equals(PkType))
                .Select(x => new { Value = x, Display = GetDisplayName(x) }).OrderBy(x => x.Display).ToArray();
        }

        private static string GetDisplayName(INamedTypeSymbol type)
        {
            var containingType = type.ContainingType;
            return containingType != null ? GetDisplayName(containingType) + "." + type.Name : type.Name;
        }

        public void InitForeignKeyEntries(DataSet<ForeignKeyEntry> entries, INamedTypeSymbol pkType)
        {
            entries.Clear();

            var constructorParams = GetConstructorParams(pkType);
            if (constructorParams.IsDefaultOrEmpty)
                return;

            var _ = entries._;
            var columns = GetColumns().ToArray();
            for (int i = 0; i < constructorParams.Length; i++)
            {
                var dataRow = entries.AddRow();
                var constructorParam = constructorParams[i];
                _.Parameter[dataRow] = constructorParam;
                var constructorParamType = constructorParam.Type as INamedTypeSymbol;
                var columnSelection = columns.Where(x => x.Type.Equals(constructorParamType) || x.Type.IsDerivedFrom(constructorParamType));
                _.Column[dataRow] = columnSelection.Where(x => x.Name.ToLower() == constructorParam.Name.ToLower()).FirstOrDefault();
                _.ColumnSelection[dataRow] = columnSelection.Select(x => new { Value = x, Display = x.Name }).ToArray();
            }
        }

        private static ImmutableArray<IParameterSymbol> GetConstructorParams(INamedTypeSymbol pkType)
        {
            var constructors = pkType.Constructors;
            if (constructors.IsDefaultOrEmpty || constructors.Length != 1)
                return default(ImmutableArray<IParameterSymbol>);

            var constructor = constructors[0];
            return constructor == null ? default(ImmutableArray<IParameterSymbol>) : constructor.Parameters;
        }

        public Document AddForeignKey(INamedTypeSymbol fkType, string fkName, DataSet<ForeignKeyEntry> entries)
        {
            return AddForeignKeyAsync(fkType, fkName, entries).Result;
        }

        private async Task<Document> AddForeignKeyAsync(INamedTypeSymbol fkType, string fkName, DataSet<ForeignKeyEntry> entries, CancellationToken ct = default(CancellationToken))
        {
            var editor = await DocumentEditor.CreateAsync(Document, ct);
            var g = editor.Generator;
            var imports = new NamespaceSet
            {
                fkType
            };

            var backingFieldName = GetBackingFieldName(fkName);
            var backingFieldAnnotation = new SyntaxAnnotation();
            var fieldDeclaration = g.FieldDeclaration(backingFieldName, GenerateFkType(g, fkType), Accessibility.Private).WithAdditionalAnnotations(Formatter.Annotation, backingFieldAnnotation);

            var assignment = g.AssignmentStatement(g.IdentifierName(backingFieldName), g.ObjectCreationExpression(GenerateFkType(g, fkType), GenerateArguments(g, entries)));
            var propertyGetter = GenerateFkPropertyGetter(g, backingFieldName, assignment);
            var propertyAnnotation = new SyntaxAnnotation();
            var property = g.PropertyDeclaration(fkName, GenerateFkType(g, fkType), Accessibility.Public, DeclarationModifiers.ReadOnly, getAccessorStatements: propertyGetter)
                .WithAdditionalAnnotations(Formatter.Annotation, propertyAnnotation);

            await AddMissingNamespaces(editor, imports, ct);
            editor.AddMember(ModelClass, fieldDeclaration);
            editor.AddMember(ModelClass, property);

            return await KeepTogetherAsync(await editor.FormatAsync(ct), backingFieldAnnotation, propertyAnnotation, ct);
        }

        private static SyntaxNode GenerateFkType(SyntaxGenerator g, INamedTypeSymbol fkType)
        {
            var containgingType = fkType.ContainingType;
            return containgingType == null ? g.IdentifierName(fkType.Name) : g.DottedName(string.Format("{0}.{1}", containgingType.Name, fkType.Name));
        }

        protected abstract IEnumerable<SyntaxNode> GenerateFkPropertyGetter(SyntaxGenerator g, string backingFieldName, SyntaxNode assignment);
    }
}
