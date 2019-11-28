using DevZest.Data.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        [CustomValidator(nameof(VAL_ConstructorParam))]
        public sealed class PrimaryKeyEntry : ProjectionEntry
        {   
            static PrimaryKeyEntry()
            {
                RegisterLocalColumn((PrimaryKeyEntry _) => _.SortDirection);
                RegisterLocalColumn((PrimaryKeyEntry _) => _.ConstructorParamName);
            }

            [Display(Name = nameof(UserMessages.Display_PrimaryKeyEntry_Sort), ResourceType = typeof(UserMessages))]
            public LocalColumn<SortDirection> SortDirection { get; private set; }

            [Required]
            [Display(Name = nameof(UserMessages.Display_PrimaryKeyEntry_ConstructorParamName), ResourceType = typeof(UserMessages))]
            public LocalColumn<string> ConstructorParamName { get; private set; }

            protected override void OnValueChanged(ValueChangedEventArgs e)
            {
                if (e.Columns.Contains(Column))
                    RefreshConstructorParamName(e.DataRow);
                base.OnValueChanged(e);
            }

            private void RefreshConstructorParamName(DataRow dataRow)
            {
                ConstructorParamName[dataRow] = GetDefaultConstructorParamName(dataRow);
            }

            private string GetDefaultConstructorParamName(DataRow dataRow)
            {
                var column = Column[dataRow];
                return column == null ? null : column.Name.ToCamelCase();
            }

            private bool IsValidParamName(string name)
            {
                return ModelMapper == null ? true : ModelMapper.IsValidIdentifier(name) || ModelMapper.IsKeyword(name);
            }

            [_CustomValidator]
            private CustomValidatorEntry VAL_ConstructorParam
            {
                get
                {
                    string Validate(DataRow dataRow)
                    {
                        var constructorParamName = ConstructorParamName[dataRow];
                        if (!IsValidParamName(constructorParamName))
                            return UserMessages.PrimaryKeyEntry_InvalidParamName;
                        else
                        {
                            var column = Column[dataRow];
                            if (column == null)
                                return null;
                            if (column.Name.ToLower() != constructorParamName.ToLower())
                                return UserMessages.PrimaryKeyEntry_MismatchParamName;
                        }
                        return null;
                    }

                    IColumns GetSourceColumns()
                    {
                        return ConstructorParamName;
                    }

                    return new CustomValidatorEntry(Validate, GetSourceColumns);
                }
            }
        }

        public DataSet<PrimaryKeyEntry> CreatePrimaryKeyEntries()
        {
            var result = CreateEntries<PrimaryKeyEntry>();

            foreach (var column in GetColumns())
            {
                var columnName = column.Name.ToLower();
                if (columnName == "id" || columnName == (column.ContainingType.Name + "id").ToLower())
                {
                    var dataRow = result.AddRow();
                    result._.Column[dataRow] = column;
                    break;
                }
            }

            return result;
        }

        public DataSet<PrimaryKeyEntry> GetPrimaryKeyEntries()
        {
            var result = CreateEntries<PrimaryKeyEntry>();
            var _ = result._;
            var constructorParams = GetKeyConstructorParameters();
            var constructorArguments = GetKeyConstructorArguments(constructorParams);
            var count = Math.Max(constructorParams.Length, constructorArguments.Length);
            for (int i = 0; i < count; i++)
            {
                var dataRow = result.AddRow();
                _.Column[dataRow] = i < constructorArguments.Length ? constructorArguments[i] : null;
                _.ConstructorParamName[dataRow] = i < constructorParams.Length ? constructorParams[i].Name : null;
            }
            return result;
        }

        private ImmutableArray<IParameterSymbol> GetKeyConstructorParameters()
        {
            foreach (var member in ModelType.GetMembers().OfType<IMethodSymbol>())
            {
                if (member is IMethodSymbol methodSymbol)
                {
                    var result = methodSymbol.GetKeyConstructorParams(Compilation, out var isKeyCreation);
                    if (isKeyCreation)
                        return result;
                }
            }
            return ImmutableArray<IParameterSymbol>.Empty;
        }

        protected abstract ImmutableArray<IPropertySymbol> GetKeyConstructorArguments(ImmutableArray<IParameterSymbol> constructorParams);

        public bool CanAddPrimaryKey
        {
            get { return BaseModel != null && ModelType.BaseTypeEqualsTo(KnownTypes.Model, Compilation); }
        }

        public Document AddPrimaryKey(string pkTypeName, DataSet<PrimaryKeyEntry> entries, string keyTypeName, string refTypeName)
        {
            return AddPrimaryKeyAsync(pkTypeName, entries, keyTypeName, refTypeName, CancellationToken.None).Result;
        }

        public async Task<Document> AddPrimaryKeyAsync(string pkTypeName, DataSet<PrimaryKeyEntry> entries, string keyTypeName, string refTypeName, CancellationToken ct)
        {
            if (!CanAddPrimaryKey || entries == null || entries.Count == 0)
                return null;

            var editor = await DocumentEditor.CreateAsync(Document, ct);
            var g = editor.Generator;
            var imports = new NamespaceSet();

            var pkClass = GeneratePkClass(g, imports, pkTypeName, entries).WithAdditionalAnnotations(Formatter.Annotation);
            var pkCreate = GeneratePkCreate(g, "CreatePrimaryKey", pkTypeName, entries).WithAdditionalAnnotations(Formatter.Annotation);
            var keyClass = string.IsNullOrEmpty(keyTypeName) ? null : GenerateKeyOrRefClass(g, imports, keyTypeName, "Key", pkTypeName, "CreatePrimaryKey", entries).WithAdditionalAnnotations(Formatter.Annotation);
            var refClass = string.IsNullOrEmpty(refTypeName) ? null : GenerateKeyOrRefClass(g, imports, refTypeName, "Ref", pkTypeName, "CreateForeignKey", entries).WithAdditionalAnnotations(Formatter.Annotation);

            editor.InsertMembers(ModelClass, 0, (new SyntaxNode[] { pkClass, pkCreate, keyClass, refClass }).Where(x => x != null));

            var genericName = g.GenericName("Model", g.QualifiedName(g.IdentifierName(ModelType.Name), g.IdentifierName(pkTypeName))).WithAdditionalAnnotations(Formatter.Annotation);
            editor.ReplaceNode(BaseModel, genericName);
            await AddMissingNamespaces(editor, imports, ct);
            return await editor.FormatAsync(ct);
        }

        public Document AddKey(string typeName, DataSet<PrimaryKeyEntry> entries)
        {
            return AddKeyAsync(typeName, entries, CancellationToken.None).Result;
        }

        public Task<Document> AddKeyAsync(string typeName, DataSet<PrimaryKeyEntry> entries, CancellationToken ct)
        {
            return AddKeyOrRefAsync(typeName, entries, "Key", "CreatePrimaryKey", ct);
        }

        public Document AddRef(string typeName, DataSet<PrimaryKeyEntry> entries)
        {
            return AddRefAsync(typeName, entries, CancellationToken.None).Result;
        }

        public Task<Document> AddRefAsync(string typeName, DataSet<PrimaryKeyEntry> entries, CancellationToken ct)
        {
            return AddKeyOrRefAsync(typeName, entries, "Ref", "CreateForeignKey", ct);
        }

        private async Task<Document> AddKeyOrRefAsync(string keyOrRefTypeName, DataSet<PrimaryKeyEntry> entries, string baseTypeName, string pkCreateMethodName, CancellationToken ct)
        {
            var editor = await DocumentEditor.CreateAsync(Document, ct);
            var g = editor.Generator;
            var imports = new NamespaceSet();
            var keyOrRefClass = GenerateKeyOrRefClass(g, imports, keyOrRefTypeName, baseTypeName, PkType.Name, pkCreateMethodName, entries).WithAdditionalAnnotations(Formatter.Annotation);
            var lastNodeToAddKeyOrRef = GetLastNodeToAddKeyOrRef();
            editor.InsertAfter(lastNodeToAddKeyOrRef, keyOrRefClass);
            await AddMissingNamespaces(editor, imports, ct);
            return await editor.FormatAsync(ct);
        }

        protected virtual SyntaxNode GetLastNodeToAddKeyOrRef()
        {
            return GetLastNode<INamedTypeSymbol>(ModelType, IsKeyOrRef) ?? GetLastNode<IMethodSymbol>(ModelType, x => x.IsKeyCreation(Compilation));
        }

        private SyntaxNode GeneratePkClass(SyntaxGenerator g, NamespaceSet imports, string pkTypeName, DataSet<PrimaryKeyEntry> entries)
        {
            imports.Add(Compilation.GetKnownType(KnownTypes.CandidateKey));
            var baseType = g.IdentifierName(nameof(KnownTypes.CandidateKey));
            var parameters = GeneratePkParams(g, imports, entries);
            var arguments = GeneratePkArguments(g, entries);
            var constructor = g.ConstructorDeclaration(pkTypeName, accessibility: Accessibility.Public, parameters: parameters, baseConstructorArguments: arguments);

            return g.ClassDeclaration(pkTypeName, accessibility: Accessibility.Public, modifiers: DeclarationModifiers.Sealed, baseType: baseType, members: new SyntaxNode[] { constructor });
        }

        private SyntaxNode[] GeneratePkParams(SyntaxGenerator g, NamespaceSet imports, DataSet<PrimaryKeyEntry> entries)
        {
            var _ = entries._;
            var result = new SyntaxNode[entries.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var type = _.Column[i].Type;
                imports.Add(type);
                var parameter = g.ParameterDeclaration(_.ConstructorParamName[i], g.IdentifierName(type.Name));
                var sortDirection = _.SortDirection[i];
                SyntaxNode sortAttribute = null;
                if (sortDirection == SortDirection.Ascending)
                    sortAttribute = g.Attribute(g.IdentifierName("Asc"));
                else if (sortDirection == SortDirection.Descending)
                    sortAttribute = g.Attribute(g.IdentifierName("Desc"));
                if (sortAttribute != null)
                {
                    imports.Add(Compilation.GetKnownType(KnownTypes.AscAttribute));
                    parameter = g.AddAttributes(parameter, sortAttribute);
                }
                result[i] = parameter;
            }

            return result;
        }

        private static SyntaxNode[] GeneratePkArguments(SyntaxGenerator g, DataSet<PrimaryKeyEntry> entries)
        {
            var _ = entries._;
            var result = new SyntaxNode[entries.Count];
            for (int i = 0; i < result.Length; i++)
            {
                SyntaxNode expression = g.IdentifierName(_.ConstructorParamName[i]);
                var sortDirection = _.SortDirection[i];
                if (sortDirection == SortDirection.Ascending)
                    expression = g.InvocationExpression(g.MemberAccessExpression(expression, "Asc"));
                else if (sortDirection == SortDirection.Descending)
                    expression = g.InvocationExpression(g.MemberAccessExpression(expression, "Desc"));
                result[i] = g.Argument(expression);
            }

            return result;
        }

        private static SyntaxNode GeneratePkCreate(SyntaxGenerator g, string methodName, string pkTypeName, DataSet<PrimaryKeyEntry> entries)
        {
            var returnType = g.IdentifierName(pkTypeName);
            var arguments = GenerateArguments(g, entries);
            var returnStatement = g.ReturnStatement(g.ObjectCreationExpression(g.IdentifierName(pkTypeName), arguments));
            return g.MethodDeclaration(methodName, returnType: returnType, accessibility: Accessibility.Protected,
                modifiers: DeclarationModifiers.Sealed | DeclarationModifiers.Override,
                statements: new SyntaxNode[] { returnStatement });
        }

        private SyntaxNode GenerateKeyOrRefClass(SyntaxGenerator g, NamespaceSet imports, string className, string baseTypeName, string pkTypeName, string pkCreateMethodName, DataSet<PrimaryKeyEntry> entries)
        {
            imports.Add(Compilation.GetKnownType(KnownTypes.KeyOf));
            var baseType = g.GenericName(baseTypeName, g.IdentifierName(pkTypeName));
            var constructor = GenerateStaticConstructorForColumnRegistration(g, imports, Language, ModelType, className, entries);
            var pkCreate = GeneratePkCreate(g, pkCreateMethodName, pkTypeName, entries);
            var properties = GenerateColumnProperties(g, imports, entries);

            return g.ClassDeclaration(className, accessibility: Accessibility.Public, baseType: baseType, members: constructor.Concat(pkCreate, properties));
        }
    }
}
