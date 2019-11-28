using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DevZest.Data.CodeAnalysis
{
    public abstract partial class DbMapper : ClassMapper
    {
        private sealed class Factory : Factory<DbMapper>
        {
            public static readonly Factory Singleton = new Factory();

            private Factory()
            {
            }

            protected override DbMapper GetCSharpMapper(DbMapper mapper, CodeContext context)
            {
                return CSharp.GetMapper(mapper, context);
            }

            protected override DbMapper GetVisualBasicMapper(DbMapper mapper, CodeContext context)
            {
                return VisualBasic.GetMapper(mapper, context);
            }
        }

        public static DbMapper Refresh(DbMapper dbMapper, Document document, TextSpan textSpan)
        {
            return Factory.Singleton.GetMapper(dbMapper, document, textSpan);
        }

        public SyntaxNode DbClass
        {
            get { return GetDbClass(); }
        }

        protected abstract SyntaxNode GetDbClass();

        public INamedTypeSymbol DbType { get; protected set; }

        protected sealed override INamespaceSymbol ContainingNamespace
        {
            get { return DbType.ContainingNamespace; }
        }

        protected override void Update(CodeContext context)
        {
            base.Update(context);
            _treeItems = null;
        }

        private bool IsDbTable(IPropertySymbol propertySymbol)
        {
            return propertySymbol.SetMethod == null && Compilation.GetKnownType(KnownTypes.DbTableOf).Equals(propertySymbol?.Type?.OriginalDefinition);
        }
    }
}
