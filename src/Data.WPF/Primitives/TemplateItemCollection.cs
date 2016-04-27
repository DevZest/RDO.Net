using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class TemplateItemCollection<T> : TemplateItemCollectionBase<T>
        where T : TemplateItem
    {
        private IReadOnlyList<T> _autoSizeItemsBefore;

        private IReadOnlyList<T> _autoSizeItemsAfter;

        internal IReadOnlyList<T> AutoSizeItemsBefore
        {
            get { return _autoSizeItemsBefore ?? (_autoSizeItemsBefore = CalcAutoSizeItems(AutoSizeMeasureOrder.Before)); }
        }

        internal IReadOnlyList<T> AutoSizeItemsAfter
        {
            get { return _autoSizeItemsAfter ?? (_autoSizeItemsAfter = CalcAutoSizeItems(AutoSizeMeasureOrder.After)); }
        }

        internal override void InvalidateAutoHeightItems()
        {
            _autoSizeItemsBefore = _autoSizeItemsAfter = null;
            base.InvalidateAutoHeightItems();
        }

        internal override void InvalidateAutoWidthItems()
        {
            _autoSizeItemsBefore = _autoSizeItemsAfter = null;
            base.InvalidateAutoWidthItems();
        }

        private IReadOnlyList<T> CalcAutoSizeItems(AutoSizeMeasureOrder order)
        {
            ReadOnlyCollection<T> collection = this;
            return collection.Where(x => x.AutoSizeMeasureOrder == order && x.IsAutoSize).OrderBy(x => x.AutoSizeMeasureIndex).ToArray();
        }
    }
}
