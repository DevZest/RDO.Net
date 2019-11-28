using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
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

        private void InitTreeItems()
        {
            _treeItems = DataSet<TreeItem>.Create();

            var root = AddRow(null, new ModelNode(this));

            InitPrimaryKeyItem(root);
            InitFieldItems(root);
            InitComputationItems(root);
            InitConstraintItems(root);
            InitCustomValidatorItems(root);
            InitIndexItems(root);
            InitRelationshipItems(root);
            InitProjectionItems(root);
        }

        private void InitPrimaryKeyItem(DataRow root)
        {
            var pkNode = GetPrimaryKeyNode();
            if (pkNode != null)
            {
                var pkRow = AddRow(root, pkNode);
                foreach (var node in _modelType.GetTypeMembers<INamedTypeSymbol>(IsKeyOrRef).Select(CreateNode))
                    AddRow(pkRow, node);
            }

            PrimaryKeyNode GetPrimaryKeyNode()
            {
                return PkType != null ? new PrimaryKeyNode(this) : null;
            }

            Node CreateNode(INamedTypeSymbol type)
            {
                if (IsKey(type))
                    return new KeyNode(this, type);
                else
                {
                    Debug.Assert(IsRef(type));
                    return new RefNode(this, type);
                }
            }
        }

        private bool IsKeyOrRef(INamedTypeSymbol type)
        {
            return IsKey(type) || IsRef(type);
        }

        private bool IsKey(INamedTypeSymbol type)
        {
            return type.IsDerivedFrom(KnownTypes.KeyOf, Compilation);
        }

        private bool IsRef(ITypeSymbol type)
        {
            return type.IsDerivedFrom(KnownTypes.RefOf, Compilation);
        }

        private void InitFieldItems(DataRow root)
        {
            InitFieldNodes(root, _modelType);
        }

        private void InitFieldNodes(DataRow parent, INamedTypeSymbol typeSymbol)
        {
            foreach (var node in typeSymbol.GetTypeMembers<IPropertySymbol>(IsField).Select(CreateNode))
                AddRow(parent, node);

            bool IsField(IPropertySymbol propertySymbol)
            {
                return FieldNode.IsField(propertySymbol, Compilation);
            }

            Node CreateNode(IPropertySymbol propertySymbol)
            {
                return new FieldNode(this, propertySymbol);
            }
        }

        private void InitComputationItems(DataRow root)
        {
            DataRow folderRow = null;

            foreach (var node in GetTypeAttributes(_modelType, IsComputation).Select(CreateNode).Where(x => x != null))
            {
                if (folderRow == null)
                    folderRow = AddRow(root, new FolderNode(UserMessages.FolderName_Computation));
                AddRow(folderRow, node);
            }

            bool IsComputation(AttributeData attribute)
            {
                return Compilation.GetTypeByMetadataName("DevZest.Data.Annotations.ComputationAttribute").Equals(attribute.AttributeClass);
            }

            Node CreateNode((INamedTypeSymbol, AttributeData) info)
            {
                return AttributeNode.CreateComputation(this, info);
            }
        }

        private void InitConstraintItems(DataRow root)
        {
            DataRow folderRow = null;

            foreach (var node in GetTypeAttributes(_modelType, IsConstraint).Select(CreateNode).Where(x => x != null))
            {
                if (folderRow == null)
                    folderRow = AddRow(root, new FolderNode(UserMessages.FolderName_Constraint));
                AddRow(folderRow, node);
            }

            bool IsConstraint(AttributeData attribute)
            {
                return IsCheckConstraint(attribute) || IsUniqueConstraint(attribute);
            }

            bool IsCheckConstraint(AttributeData attribute)
            {
                return Compilation.GetKnownType(KnownTypes.CheckConstraintAttribute).Equals(attribute.AttributeClass);
            }

            Node CreateNode((INamedTypeSymbol Type, AttributeData Attribute) info)
            {
                if (IsCheckConstraint(info.Attribute))
                    return AttributeNode.CreateCheckConstraint(this, info);
                else
                {
                    Debug.Assert(IsUniqueConstraint(info.Attribute));
                    return AttributeNode.CreateUniqueConstraint(this, info);
                }
            }
        }

        private bool IsUniqueConstraint(AttributeData attribute)
        {
            return Compilation.GetKnownType(KnownTypes.UniqueConstraintAttribute).Equals(attribute.AttributeClass);
        }

        private void InitCustomValidatorItems(DataRow root)
        {
            DataRow folderRow = null;

            foreach (var node in GetTypeAttributes(_modelType, IsCustomValidator).Select(CreateNode).Where(x => x != null))
            {
                if (folderRow == null)
                    folderRow = AddRow(root, new FolderNode(UserMessages.FolderName_Validator));
                AddRow(folderRow, node);
            }

            bool IsCustomValidator(AttributeData attribute)
            {
                return Compilation.GetKnownType(KnownTypes.CustomValidatorAttribute).Equals(attribute.AttributeClass);
            }

            Node CreateNode((INamedTypeSymbol Type, AttributeData Attribute) info)
            {
                return AttributeNode.CreateCustomValidator(this, info);
            }
        }

        private void InitIndexItems(DataRow root)
        {
            DataRow folderRow = null;

            foreach (var node in GetTypeAttributes(_modelType, IsIndex).Select(CreateNode).Where(x => x != null))
            {
                if (folderRow == null)
                    folderRow = AddRow(root, new FolderNode(UserMessages.FolderName_Index));
                AddRow(folderRow, node);
            }

            bool IsIndex(AttributeData attribute)
            {
                return IsDbIndex(attribute) || IsUniqueConstraint(attribute);
            }

            bool IsDbIndex(AttributeData attribute)
            {
                return Compilation.GetKnownType(KnownTypes.DbIndexAttribute).Equals(attribute.AttributeClass);
            }

            Node CreateNode((INamedTypeSymbol Type, AttributeData Attribute) info)
            {
                if (IsDbIndex(info.Attribute))
                    return AttributeNode.CreateIndex(this, info);
                else
                {
                    Debug.Assert(IsUniqueConstraint(info.Attribute));
                    return AttributeNode.CreateUniqueConstraint(this, info);
                }
            }
        }

        private void InitRelationshipItems(DataRow root)
        {
            DataRow folderRow = null;

            foreach (var node in _modelType.GetTypeMembers<IPropertySymbol>(IsRelationship).Select(CreateNode))
            {
                if (folderRow == null)
                    folderRow = AddRow(root, new FolderNode(UserMessages.FolderName_Relationship));
                AddRow(folderRow, node);
            }

            bool IsRelationship(IPropertySymbol propertySymbol)
            {
                return IsForeignKey(propertySymbol) || IsChildModel(propertySymbol);
            }

            bool IsForeignKey(IPropertySymbol propertySymbol)
            {
                return propertySymbol.IsForeignKey(Compilation);
            }

            bool IsChildModel(IPropertySymbol propertySymbol)
            {
                return propertySymbol.SetMethod != null && propertySymbol.GetPropertyRegistrationType(Compilation) == PropertyRegistrationType.ChildModel;
            }

            Node CreateNode(IPropertySymbol propertySymbol)
            {
                if (IsForeignKey(propertySymbol))
                    return new ForeignKeyNode(this, propertySymbol);
                else
                {
                    Debug.Assert(IsChildModel(propertySymbol));
                    return new ChildModelNode(this, propertySymbol);
                }
            }
        }

        private void InitProjectionItems(DataRow root)
        {
            DataRow folderRow = null;

            foreach (var node in _modelType.GetTypeMembers<INamedTypeSymbol>(IsProjection).Select(CreateNode))
            {
                if (folderRow == null)
                    folderRow = AddRow(root, new FolderNode(UserMessages.FolderName_Projection));
                var row = AddRow(folderRow, node);
                InitFieldNodes(row, node.TypeSymbol);
            }

            bool IsProjection(INamedTypeSymbol type)
            {
                return type.IsDerivedFrom(KnownTypes.Projection, Compilation) && !IsKeyOrRef(type);
            }

            ProjectionNode CreateNode(INamedTypeSymbol typeSymbol)
            {
                return new ProjectionNode(this, typeSymbol);
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
    }
}
