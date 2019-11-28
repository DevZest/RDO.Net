using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    partial class DbMapper
    {
        private sealed class FolderNode : Node
        {
            public FolderNode(DbMapper mapper, string name)
                : base(mapper)
            {
                _name = name;
            }

            public override NodeKind Kind => NodeKind.Folder;

            public override ISymbol Symbol => null;

            private readonly string _name;
            public override string Name => _name;

            public override bool IsEnabled => true;

            public override bool IsFolder => true;
        }
    }
}
