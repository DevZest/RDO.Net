using System.Collections.Generic;

namespace DevZest.Data.Windows.Primitives
{
    /// <summary>Collection container for <see cref="ScalarItem"/> and <see cref="BlockItem"/>.</summary>
    /// <typeparam name="T"><see cref="ScalarItem"/> or <see cref="BlockItem"/>.</typeparam>
    internal sealed class RecapItemCollection<T> : TemplateItemCollection<T>
        where T : TemplateItem, IConcatList<T>
    {
        private IReadOnlyList<T> _preAutoSizeItems;
        internal IReadOnlyList<T> PreAutoSizeItems
        {
            get { return _preAutoSizeItems ?? (_preAutoSizeItems = FilterAutoSizeItems(x => x.IsAutoSize && x.AutoSizeOrder <= 0)); }
        }

        private IReadOnlyList<T> _postAutoSizeItems;
        internal IReadOnlyList<T> PostAutoSizeItems
        {
            get { return _postAutoSizeItems ?? (_postAutoSizeItems = FilterAutoSizeItems(x => x.IsAutoSize && x.AutoSizeOrder > 0)); }
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
