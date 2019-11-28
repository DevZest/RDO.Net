using Microsoft.CodeAnalysis;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    partial class DbMapper
    {
        public sealed class TreeItem : TreeItem<Node>
        {
            static TreeItem()
            {
                RegisterChildModel((TreeItem _) => _.Child);
            }

            public TreeItem Child { get; private set; }

            public override Model GetChild()
            {
                return Child;
            }
        }

        private DataSet<TreeItem> _treeItems;
        public DataSet<TreeItem> TreeItems
        {
            get
            {
                if (_treeItems == null)
                    InitTreeItems();
                return _treeItems;
            }
        }

        private DataRow AddRow(DataRow parentDataRow, Node node)
        {
            var dataSet = GetDataSet();
            var result = dataSet.AddRow();
            var _ = dataSet._;
            _.Node[result] = node;
            return result;

            DataSet<TreeItem> GetDataSet()
            {
                return parentDataRow == null ? _treeItems : ((TreeItem)parentDataRow.Model).Child.GetChildDataSet(parentDataRow);
            }
        }

        private void InitTreeItems()
        {
            _treeItems = DataSet<TreeItem>.Create();

            var root = AddRow(null, new DbNode(this));

            InitTableItems(root);
            InitRelationshipItems(root);
        }

        private void InitTableItems(DataRow root)
        {
            InitTableNodes(root, DbType);
        }

        private void InitTableNodes(DataRow parent, INamedTypeSymbol typeSymbol)
        {
            foreach (var table in typeSymbol.GetTypeMembers<IPropertySymbol>(IsDbTable))
            {
                var dataRow = AddRow(parent, CreateNode(table));
                foreach (var attribute in table.GetAttributes().Where(x => Compilation.GetKnownType(KnownTypes.RelationshipAttribute).Equals(x.AttributeClass)))
                {
                    var name = attribute.GetStringArgument();
                    if (!string.IsNullOrWhiteSpace(name))
                        AddRow(dataRow, new RelationshipDeclarationNode(this, table, attribute, attribute.GetStringArgument()));
                }
            }

            Node CreateNode(IPropertySymbol propertySymbol)
            {
                return new TableNode(this, propertySymbol);
            }
        }

        private void InitRelationshipItems(DataRow root)
        {
            DataRow folderRow = null;

            foreach (var node in DbType.GetTypeMembers<IMethodSymbol>(IsForeignKeyImplementation).Select(CreateNode))
            {
                if (folderRow == null)
                    folderRow = AddRow(root, new FolderNode(this, UserMessages.FolderName_Relationship));
                AddRow(folderRow, node);
            }

            bool IsForeignKeyImplementation(IMethodSymbol methodSymbol)
            {
                return methodSymbol.HasAttribute(Compilation.GetKnownType(KnownTypes._RelationshipAttribute));
            }

            Node CreateNode(IMethodSymbol methodSymbol)
            {
                return new RelationshipImplementationNode(this, methodSymbol);
            }
        }
    }
}
