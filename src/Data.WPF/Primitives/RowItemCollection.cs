using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class RowItemCollection : TemplateItemCollectionBase<RowItem>, IConcatList<RowItemCollection>
    {
        #region IConcatList<RowItemCollection>

        IEnumerator<RowItemCollection> IEnumerable<RowItemCollection>.GetEnumerator()
        {
            yield return this;
        }

        bool IConcatList<RowItemCollection>.IsReadOnly
        {
            get { return true; }
        }

        int IReadOnlyCollection<RowItemCollection>.Count
        {
            get { return 1; }
        }

        RowItemCollection IReadOnlyList<RowItemCollection>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }
        #endregion

        private IReadOnlyList<RowItem> _autoSizeItems;

        internal IReadOnlyList<RowItem> AutoSizeItems
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

        private IReadOnlyList<RowItem> CalcAutoSizeItems()
        {
            ReadOnlyCollection<RowItem> collection = this;
            return collection.Where(x => x.IsAutoSize).OrderBy(x => x.AutoSizeMeasureIndex).ToArray();
        }
    }
}
