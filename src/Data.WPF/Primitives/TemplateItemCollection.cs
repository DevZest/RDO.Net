using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class TemplateItemCollection<T> : TemplateItemCollectionBase<T>
        where T : TemplateItem
    {
        private IReadOnlyList<T> _autoSizeItems;

        internal IReadOnlyList<T> AutoSizeItems
        {
            get { return _autoSizeItems ?? (_autoSizeItems = CalcAutoSizeItems()); }
        }

        internal override void InvalidateAutoHeightItems()
        {
            _autoSizeItems = null;
            base.InvalidateAutoHeightItems();
        }

        internal override void InvalidateAutoWidthItems()
        {
            _autoSizeItems = null;
            base.InvalidateAutoWidthItems();
        }

        private IReadOnlyList<T> CalcAutoSizeItems()
        {
            ReadOnlyCollection<T> collection = this;
            return collection.Where(x => x.IsAutoSize).OrderBy(x => x.AutoSizeMeasureIndex).ToArray();
        }
    }
}
