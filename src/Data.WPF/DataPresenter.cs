using System.Collections.Generic;
using DevZest.Data.Windows.Primitives;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class DataPresenter
    {
        private readonly RowPresenter _parent;
        public RowPresenter Parent
        {
            get { return _parent; }
        }

        public DataSet DataSet
        {
            get { return GetDataSet(); }
        }

        internal abstract DataSet GetDataSet();

        private readonly Template _template = new Template();
        public Template Template
        {
            get { return _template; }
        }

        public bool IsRecursive
        {
            get { return Template.IsRecursive; }
        }

        private LayoutManager _layoutManager;
        internal LayoutManager LayoutManager
        {
            get { return _layoutManager ?? (_layoutManager = LayoutManager.Create(this)); }
        }

        public IReadOnlyList<RowPresenter> Rows
        {
            get { return LayoutManager.Rows; }
        }

        public RowPresenter CurrentRow
        {
            get { return LayoutManager.CurrentRow; }
        }

        public RowPresenter EditingRow
        {
            get { return LayoutManager.EditingRow; }
        }

        public IReadOnlyCollection<RowPresenter> SelectedRows
        {
            get { return LayoutManager.SelectedRows; }
        }

        public IReadOnlyList<IBlockPresenter> Blocks
        {
            get { return LayoutManager.Blocks; }
        }

        public IReadOnlyList<UIElement> Elements
        {
            get { return LayoutManager.Elements; }
        }
    }
}
