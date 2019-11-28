using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.CodeAnalysis
{
    public abstract partial class ModelMapper : ClassMapper
    {
        private sealed class Factory : Factory<ModelMapper>
        {
            public static readonly Factory Singleton = new Factory();

            private Factory()
            {
            }

            protected override ModelMapper GetCSharpMapper(ModelMapper mapper, CodeContext context)
            {
                return CSharp.GetMapper(mapper, context);
            }

            protected override ModelMapper GetVisualBasicMapper(ModelMapper mapper, CodeContext context)
            {
                return VisualBasic.GetMapper(mapper, context);
            }
        }

        public static ModelMapper Refresh(ModelMapper modelMapper, Document document, TextSpan textSpan)
        {
            return Factory.Singleton.GetMapper(modelMapper, document, textSpan);
        }

        public SyntaxNode ModelClass
        {
            get { return GetModelClass(); }
        }

        protected abstract SyntaxNode GetModelClass();

        public SyntaxNode BaseModel
        {
            get { return GetBaseModel(); }
        }

        protected abstract SyntaxNode GetBaseModel();

        private INamedTypeSymbol _modelType;
        public INamedTypeSymbol ModelType
        {
            get { return _modelType; }
            protected set
            {
                if (_modelType == value)
                    return;

                _modelType = value;
                PkTypeSymbol = value?.GetPkType(Compilation);
            }
        }

        protected sealed override INamespaceSymbol ContainingNamespace
        {
            get { return ModelType.ContainingNamespace; }
        }

        public IEnumerable<IPropertySymbol> GetColumns(bool currentDocumentOnly = false)
        {
            return currentDocumentOnly ? GetCurrentDocumentColumns() : ModelType.GetTypeMembers<IPropertySymbol>(IsColumn);
        }

        private IEnumerable<IPropertySymbol> GetCurrentDocumentColumns()
        {
            return ModelType.GetMembers().OfType<IPropertySymbol>().Where(IsColumn);
        }

        private bool IsColumn(IPropertySymbol propertySymbol)
        {
            return propertySymbol.Type.IsDerivedFrom(KnownTypes.Column, Compilation)
                && propertySymbol.SetMethod != null
                && propertySymbol.GetPropertyRegistrationType(Compilation).HasValue;
        }

        public IFieldSymbol GetMounter(IPropertySymbol column)
        {
            if (column == null)
                return null;

            var containingType = column.ContainingType;
            foreach (var field in containingType.GetMembers("_" + column.Name).OfType<IFieldSymbol>())
            {
                if (!(field.Type is INamedTypeSymbol namedType))
                    continue;
                if (namedType.EqualsTo(KnownTypes.MounterOf, Compilation) && field.IsStatic && field.IsReadOnly && namedType.TypeArguments[0].Equals(column.Type))
                    return field;
            }

            return null;
        }

        protected override void Update(CodeContext context)
        {
            base.Update(context);
            _treeItems = null;
        }

        public ITypeSymbol PkTypeSymbol { get; private set; }

        public INamedTypeSymbol PkType
        {
            get { return PkTypeSymbol as INamedTypeSymbol; }
        }

        private SyntaxNode GetLastNodeToAddProjection()
        {
            return GetLastProjectionNode() ?? GetLastNodeToAddKeyOrRef();
        }

        protected virtual SyntaxNode GetLastProjectionNode()
        {
            return GetLastNode<INamedTypeSymbol>(ModelType, IsProjection);
        }

        private bool IsProjection(INamedTypeSymbol type)
        {
            return type.IsDerivedFrom(KnownTypes.Projection, Compilation) && !IsRef(type);
        }

        private static SyntaxNode GenerateStaticConstructorForColumnRegistration<T>(SyntaxGenerator g, NamespaceSet imports, string language, INamedTypeSymbol modelType, string className, DataSet<T> entries)
            where T : ProjectionEntry, new()
        {
            return g.ConstructorDeclaration(className, modifiers: DeclarationModifiers.Static,
                statements: GenerateColumnRegistration(g, imports, language, modelType, className, entries));
        }

        private static SyntaxNode[] GenerateColumnRegistration<T>(SyntaxGenerator g, NamespaceSet imports, string language, INamedTypeSymbol modelType, string className, DataSet<T> entries)
            where T : ProjectionEntry, new()
        {
            var _ = entries._;
            var result = new SyntaxNode[entries.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = GenerateColumnRegistration(g, imports, language, modelType, className, _.Column[i], _.Mounter[i]);

            return result;
        }

        private static SyntaxNode GenerateColumnRegistration(SyntaxGenerator g, NamespaceSet imports, string language, INamedTypeSymbol modelType, string className, IPropertySymbol column, IFieldSymbol mounter)
        {
            var lambdaExpressionParam = g.ParameterDeclaration(g.GetterLambdaParamName(language), g.IdentifierName(className));
            var lambdaExpression = g.ValueReturningLambdaExpression(new SyntaxNode[] { lambdaExpressionParam }, g.GetterBody(language, column.Name));
            var mounterArgument = g.Argument(GenerateMounterIdentifier(g, imports, modelType, mounter));
            var invocation = g.InvocationExpression(g.IdentifierName("Register"), g.Argument(lambdaExpression), mounterArgument);
            return g.ExpressionStatement(invocation);
        }

        private static SyntaxNode GenerateMounterIdentifier(SyntaxGenerator g, NamespaceSet imports, INamedTypeSymbol modelType, IFieldSymbol mounter)
        {
            if (mounter.ContainingType == modelType)
                return g.IdentifierName(mounter.Name);

            imports.Add(mounter.ContainingType);
            return g.QualifiedName(g.IdentifierName(mounter.ContainingType.Name), g.IdentifierName(mounter.Name));
        }

        private IEnumerable<SyntaxNode> GenerateColumnProperties<T>(SyntaxGenerator g, NamespaceSet imports, DataSet<T> entries)
            where T : ProjectionEntry, new()
        {
            var _ = entries._;
            for (int i = 0; i < entries.Count; i++)
            {
                var column = _.Column[i];
                var type = column.Type;
                imports.Add(type);
                foreach (var result in GenerateColumnProperty(g, type, column.Name))
                    yield return result;
            }
        }

        protected abstract IEnumerable<SyntaxNode> GenerateColumnProperty(SyntaxGenerator g, ITypeSymbol type, string name);

        private DataSet<T> CreateEntries<T>()
            where T : Model, IInitializable<ModelMapper>, new()
        {
            return DataSet<T>.Create(_ => _.Initialize(this));
        }

        public static INavigatableMarker CreateNavigatableMarker(NodeKind nodeKind, ISymbol containingType, string symbolName, bool navigationSuggested = true)
        {
            return SymbolNodeBase.CreateMarker(nodeKind, containingType, symbolName, navigationSuggested);
        }

        protected virtual bool SupportsClassLevelNameOf
        {
            get { return true; }
        }

        protected virtual SyntaxNode GenerateModelAttributeName(SyntaxGenerator g, string name)
        {
            return g.NameOfExpression(g.IdentifierName(name));
        }

        private async Task<Document> GenerateModelAttributeAsync(Document document, INamedTypeSymbol modelAttributeType, SyntaxAnnotation propertyAnnotation, string name,
            Func<SyntaxGenerator, IEnumerable<SyntaxNode>> generateAdditionalArguments, CancellationToken ct)
        {
            var editor = await DocumentEditor.CreateAsync(document, ct);
            var g = editor.Generator;
            var root = await document.GetSyntaxRootAsync(ct);
            var property = root.GetAnnotatedNodes(propertyAnnotation).Single();
            var model = property.Parent;
            var argument = GenerateModelAttributeName(g, name);
            var arguments = g.AttributeArgument(argument).Concat(generateAdditionalArguments?.Invoke(g)).ToArray();
            editor.ReplaceNode(model, GenerateAttribute(model, modelAttributeType.Name.ToAttributeName(), GetLeadingWhitespaceTrivia(model), arguments));

            return editor.GetChangedDocument();
        }
    }
}
