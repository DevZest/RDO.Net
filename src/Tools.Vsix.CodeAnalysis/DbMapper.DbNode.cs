using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    partial class DbMapper
    {
        private sealed class DbNode : Node
        {
            public DbNode(DbMapper mapper)
                : base(mapper)
            {
            }

            public override NodeKind Kind
            {
                get { return NodeKind.Db; }
            }

            public override ISymbol Symbol
            {
                get { return Mapper.DbType; }
            }
        }
    }
}
