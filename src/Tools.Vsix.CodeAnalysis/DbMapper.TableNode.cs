using DevZest.Data.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.CodeAnalysis
{
    partial class DbMapper
    {
        private sealed class TableNode : Node
        {
            public TableNode(DbMapper mapper, IPropertySymbol table)
                : base(mapper)
            {
                _table = table;
            }

            public override NodeKind Kind => NodeKind.Table;

            private readonly IPropertySymbol _table;
            public override ISymbol Symbol => _table;

            private Document Document
            {
                get { return Mapper.Document; }
            }

            public override bool CanAddRelationship
            {
                get { return Mapper.GetSyntaxNode(_table) != null; }
            }

            public override Document AddRelationship(string name, IPropertySymbol foreignKey, IPropertySymbol refTable, string description, ForeignKeyRule deleteRule, ForeignKeyRule updateRule)
            {
                Debug.Assert(CanAddRelationship);
                return Mapper.AddRelationship(_table, name, foreignKey, refTable, description, deleteRule, updateRule);
            }
        }

        private sealed class ModelTypeVisitor : SymbolVisitor
        {
            public static List<INamedTypeSymbol> Walk(Compilation compilation, HashSet<INamedTypeSymbol> existingDbTableModels)
            {
                return new ModelTypeVisitor(compilation, existingDbTableModels)._result;
            }

            private ModelTypeVisitor(Compilation compilation, HashSet<INamedTypeSymbol> existingDbTableModels)
            {
                _compilation = compilation;
                _modelType = compilation.GetKnownType(KnownTypes.Model);
                _invisibleToDbDesignerAttribute = compilation.GetKnownType(KnownTypes.InvisibleToDbDesignerAttribute);
                _existingDbTableModels = existingDbTableModels;
                Debug.Assert(_modelType != null);

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
            private readonly INamedTypeSymbol _modelType;
            private readonly INamedTypeSymbol _invisibleToDbDesignerAttribute;
            private readonly HashSet<INamedTypeSymbol> _existingDbTableModels;
            private List<INamedTypeSymbol> _result = new List<INamedTypeSymbol>();

            public override void VisitNamespace(INamespaceSymbol symbol)
            {
                foreach (var member in symbol.GetMembers())
                    member.Accept(this);
            }

            public override void VisitNamedType(INamedTypeSymbol symbol)
            {
                if (!_existingDbTableModels.Contains(symbol)
                    && !symbol.IsAbstract
                    && symbol.DeclaredAccessibility == Accessibility.Public
                    && symbol.IsDerivedFrom(_modelType)
                    && HasPublicParameterlessConstructor(symbol)
                    && !symbol.HasAttribute(_invisibleToDbDesignerAttribute))
                    _result.Add(symbol);
            }

            private static bool HasPublicParameterlessConstructor(INamedTypeSymbol symbol)
            {
                var constructors = symbol.Constructors;
                if (constructors.Length == 0)
                    return false;
                for (int i = 0; i < constructors.Length; i++)
                {
                    var constructor = constructors[i];
                    if (constructor.DeclaredAccessibility == Accessibility.Public && constructor.Parameters.Length == 0)
                        return true;
                }

                return false;
            }
        }

        public IEnumerable GetModelTypeSelection()
        {
            var dbTables = DbType.GetTypeMembers<IPropertySymbol>(IsDbTable);
            var existingDbTableModels = new HashSet<INamedTypeSymbol>();
            foreach (var dbTable in dbTables)
            {
                var modelType = dbTable.GetModelType();
                if (modelType != null)
                    existingDbTableModels.Add(modelType);
            }

            return ModelTypeVisitor.Walk(Compilation, existingDbTableModels).Select(x => new { Value = x, Display = x.Name }).OrderBy(x => x.Display).ToArray();
        }

        #region AddTable

        public Document AddDbTable(INamedTypeSymbol modelType, string name, string dbName, string description)
        {
            return AddDbTableAsync(modelType, name, dbName, description).Result;
        }

        private async Task<Document> AddDbTableAsync(INamedTypeSymbol modelType, string name, string dbName, string description, CancellationToken ct = default(CancellationToken))
        {
            var editor = await DocumentEditor.CreateAsync(Document, ct);
            var g = editor.Generator;
            var imports = new NamespaceSet
            {
                modelType,
                Compilation.GetKnownType(KnownTypes.DbTableOf)
            };
            var needDbTableAttribute = NeedTableAttribute(dbName, description);
            if (needDbTableAttribute)
                imports.Add(Compilation.GetKnownType(KnownTypes.DbTableAttribute));

            var backingFieldName = GetBackingFieldName(name);
            var backingFieldAnnotation = new SyntaxAnnotation();
            var fieldDeclaration = g.FieldDeclaration(backingFieldName, GenerateDbTableType(g, modelType), Accessibility.Private).WithAdditionalAnnotations(Formatter.Annotation, backingFieldAnnotation);

            var getTable = g.InvocationExpression(g.IdentifierName("GetTable"), g.Argument(RefKind.Ref, g.IdentifierName(backingFieldName)));
            var propertyGetter = g.ReturnStatement(getTable);
            var property = g.PropertyDeclaration(name, GenerateDbTableType(g, modelType), Accessibility.Public, DeclarationModifiers.ReadOnly, getAccessorStatements: new SyntaxNode[] { propertyGetter });
            if (needDbTableAttribute)
                property = GenerateDbTableAttribute(g, property, dbName, description);
            var propertyAnnotation = new SyntaxAnnotation();
            property = property.WithAdditionalAnnotations(Formatter.Annotation, propertyAnnotation);

            await AddMissingNamespaces(editor, imports, ct);
            editor.AddMember(DbClass, fieldDeclaration);
            editor.AddMember(DbClass, property);

            var result = await KeepTogetherAsync(await editor.FormatAsync(ct), backingFieldAnnotation, propertyAnnotation, ct);
            return result;
        }

        private static SyntaxNode GenerateDbTableType(SyntaxGenerator g, INamedTypeSymbol modelType)
        {
            return g.GenericName("DbTable", g.IdentifierName(modelType.Name));
        }

        private static bool NeedTableAttribute(string dbName, string description)
        {
            return !string.IsNullOrWhiteSpace(dbName) || !string.IsNullOrWhiteSpace(description);
        }

        private static IEnumerable<SyntaxNode> GenerateDbTableAttributeArguments(SyntaxGenerator g, string dbName, string description)
        {
            if (!string.IsNullOrWhiteSpace(dbName))
                yield return g.AttributeArgument(g.LiteralExpression(dbName));
            if (!string.IsNullOrWhiteSpace(description))
                yield return g.AttributeArgument(nameof(DbTableAttribute.Description), g.LiteralExpression(description));
        }

        private SyntaxNode GenerateDbTableAttribute(SyntaxGenerator g, SyntaxNode dbTable, string dbName, string description)
        {
            Debug.Assert(NeedTableAttribute(dbName, description));
            var dbTableAttribute = Compilation.GetKnownType(KnownTypes.DbTableAttribute);
            var arguments = GenerateDbTableAttributeArguments(g, dbName, description).ToArray();
            return GenerateAttribute(dbTable, dbTableAttribute.Name.ToAttributeName(), null, arguments);
        }
        
        #endregion

        public IEnumerable GetFkSelection(IPropertySymbol dbTable)
        {
            return GetForeignKeys(dbTable).Select(x => new { Value = x, Display = x.Name });
        }

        private IEnumerable<IPropertySymbol> GetForeignKeys(IPropertySymbol dbTable)
        {
            var modelType = dbTable.GetModelType();
            if (modelType == null)
                return ImmutableArray<IPropertySymbol>.Empty;
            else
                return modelType.GetTypeMembers<IPropertySymbol>(x => x.IsForeignKey(Compilation));
        }

        public IEnumerable<IPropertySymbol> GetRefTables(IPropertySymbol dbTable, IPropertySymbol foreignKey)
        {
            var pkType = foreignKey?.Type;
            if (pkType == null)
                return ImmutableArray<IPropertySymbol>.Empty;
            else
                return DbType.GetTypeMembers<IPropertySymbol>(IsDbTable).Where(x => pkType.Equals(x.GetModelType().GetPkType(Compilation)));
        }

        #region AddRelationship
        internal Document AddRelationship(IPropertySymbol dbTable, string name, IPropertySymbol foreignKey, IPropertySymbol refTable,
            string description, ForeignKeyRule deleteRule, ForeignKeyRule updateRule)
        {
            return AddRelationshipAsync(dbTable, name, foreignKey, refTable, description, deleteRule, updateRule).Result;
        }

        private async Task<Document> AddRelationshipAsync(IPropertySymbol dbTable, string name, IPropertySymbol foreignKey, IPropertySymbol refTable,
            string description, ForeignKeyRule deleteRule, ForeignKeyRule updateRule, CancellationToken ct = default(CancellationToken))
        {
            var editor = await DocumentEditor.CreateAsync(Document, ct);
            var g = editor.Generator;
            var declarationAttributeType = Compilation.GetKnownType(KnownTypes.RelationshipAttribute);
            var ruleType = deleteRule != ForeignKeyRule.None || updateRule != ForeignKeyRule.None ? Compilation.GetKnownType(KnownTypes.ForeignKeyRule) : null;
            var implementationAttributeType = Compilation.GetKnownType(KnownTypes._RelationshipAttribute);
            var modelType = dbTable.GetModelType();
            var keyMappingType = Compilation.GetKnownType(KnownTypes.KeyMapping);
            var imports = new NamespaceSet
            {
                declarationAttributeType,
                implementationAttributeType,
                modelType,
                keyMappingType
            };
            if (ruleType != null)
                imports.Add(ruleType);

            var paramName = ModelParamName;
            var methodBody = GenerateImplementationMethodBody();
            var method = g.MethodDeclaration(name, new SyntaxNode[] { g.ParameterDeclaration(paramName, g.IdentifierName(modelType.Name)) }, null,
                g.IdentifierName(keyMappingType.Name), Accessibility.Private, default(DeclarationModifiers), statements: new SyntaxNode[] { methodBody });
            method = GenerateAttribute(method, implementationAttributeType.Name.ToAttributeName()).WithAdditionalAnnotations(Formatter.Annotation);

            editor.AddMember(DbClass, method);

            var argument = g.NameOfExpression(g.IdentifierName(name));
            var arguments = g.AttributeArgument(argument).Concat(GenerateAdditionalArguments()).ToArray();
            var dbTableNode = GetSyntaxNode(dbTable);
            editor.ReplaceNode(dbTableNode, GenerateAttribute(dbTableNode, declarationAttributeType.Name.ToAttributeName(), GetLeadingWhitespaceTrivia(dbTableNode), arguments));

            await AddMissingNamespaces(editor, imports, ct);

            return await editor.FormatAsync(ct);

            IEnumerable<SyntaxNode> GenerateAdditionalArguments()
            {
                if (!string.IsNullOrWhiteSpace(description))
                    yield return g.AttributeArgument("Description", g.LiteralExpression(description));
                if (deleteRule != ForeignKeyRule.None)
                    yield return g.AttributeArgument("DeleteRule", g.DottedName(string.Format("{0}.{1}", ruleType.Name, deleteRule)));
                if (updateRule != ForeignKeyRule.None)
                    yield return g.AttributeArgument("UpdateRule", g.DottedName(string.Format("{0}.{1}", ruleType.Name, updateRule)));
            }

            SyntaxNode GenerateImplementationMethodBody()
            {
                var fkExpr = g.DottedName(string.Format("{0}.{1}", paramName, foreignKey.Name));
                var joinParam = dbTable == refTable ? g.IdentifierName(paramName) : GenerateJoinParam(g, refTable);
                return g.ReturnStatement(g.InvocationExpression(g.MemberAccessExpression(fkExpr, "Join"), joinParam));
            }
        }

        private SyntaxNode GetSyntaxNode(IPropertySymbol dbTable)
        {
            var syntaxRefs = dbTable.DeclaringSyntaxReferences;
            if (syntaxRefs.Length == 0)
                return null;

            for (int i = 0; i < syntaxRefs.Length; i++)
            {
                var syntaxRef = syntaxRefs[i];
                var result = syntaxRef.GetSyntax();
                if (result?.SyntaxTree == SyntaxTree)
                    return result;
            }

            return null;
        }

        #endregion

        protected abstract string ModelParamName { get; }

        private SyntaxNode GenerateJoinParam(SyntaxGenerator g, IPropertySymbol refTable)
        {
            return g.DottedName(string.Format("{0}.{1}", refTable.Name, EntityPropertyName));
        }

        protected abstract string EntityPropertyName { get; }
    }
}
