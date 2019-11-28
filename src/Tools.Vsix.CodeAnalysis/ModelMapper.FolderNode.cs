using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        private sealed class FolderNode : Node
        {
            public FolderNode(string name)
                : base(null)
            {
                _name = name;
            }

            public override NodeKind Kind
            {
                get { return NodeKind.Folder; }
            }

            private readonly string _name;
            public override string Name
            {
                get { return _name; }
            }

            public override bool IsEnabled
            {
                get { return true; }
            }

            public override Location Location
            {
                get { return null; }
            }

            public override Location LocalLocation
            {
                get { return null; }
            }

            public override bool IsFolder
            {
                get { return true; }
            }

            public override INavigatableMarker CreateMarker(bool navigationSuggested)
            {
                return null;
            }
        }
    }
}
