namespace DevZest.Data.CodeAnalysis
{
    public interface ITreeItem
    {
        LocalColumn<TreeNode> TreeNode { get; }
        Model GetChild();
    }

    public abstract class TreeItem<T> : Model, ITreeItem
        where T : TreeNode
    {
        static TreeItem()
        {
            RegisterLocalColumn((TreeItem<T> _) => _.Node);
            RegisterLocalColumn((TreeItem<T> _) => _.TreeNode);
        }

        public TreeItem()
        {
            TreeNode.ComputedAs(Node, GetTreeNode, false);
        }

        public LocalColumn<T> Node { get; private set; }

        public LocalColumn<TreeNode> TreeNode { get; private set; }

        private static TreeNode GetTreeNode(DataRow dataRow, LocalColumn<T> node)
        {
            return node[dataRow];
        }

        public abstract Model GetChild();
    }
}
