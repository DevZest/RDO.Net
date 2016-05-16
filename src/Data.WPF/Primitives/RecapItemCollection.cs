using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DevZest.Data.Windows.Primitives
{
    /// <summary>Collection container for <see cref="ScalarItem"/> and <see cref="BlockItem"/>.</summary>
    /// <typeparam name="T"><see cref="ScalarItem"/> or <see cref="BlockItem"/>.</typeparam>
    internal sealed class RecapItemCollection<T> : TemplateItemCollectionBase<T>
        where T : TemplateItem
    {
        private IReadOnlyList<T> _preAutoSizeItems;
        internal IReadOnlyList<T> PreAutoSizeItems
        {
            get { return _preAutoSizeItems ?? (_preAutoSizeItems = CalcPreAutoSizeItems()); }
        }

        private IReadOnlyList<T> CalcPreAutoSizeItems()
        {
            ReadOnlyCollection<T> collection = this;
            return collection.Where(x => x.IsAutoSize && x.AutoSizeOrder <= 0).OrderBy(x => x.AutoSizeOrder).ToArray();
        }

        private IReadOnlyList<T> _postAutoSizeItems;
        internal IReadOnlyList<T> PostAutoSizeItems
        {
            get { return _postAutoSizeItems ?? (_postAutoSizeItems = CalcPostAutoSizeItems()); }
        }

        private IReadOnlyList<T> CalcPostAutoSizeItems()
        {
            ReadOnlyCollection<T> collection = this;
            return collection.Where(x => x.IsAutoSize && x.AutoSizeOrder > 0).OrderBy(x => x.AutoSizeOrder).ToArray();
        }

        internal override void InvalidateAutoHeightItems()
        {
            _preAutoSizeItems = _postAutoSizeItems = null;
            base.InvalidateAutoHeightItems();
        }

        internal override void InvalidateAutoWidthItems()
        {
            _preAutoSizeItems = _postAutoSizeItems = null;
            base.InvalidateAutoWidthItems();
        }
    }
}
