using System;
using System.Collections.Generic;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class RowItemCollection : TemplateItemCollection<RowItem>, IConcatList<RowItemCollection>
    {
        #region IConcatList<RowItemCollection>

        IEnumerator<RowItemCollection> IEnumerable<RowItemCollection>.GetEnumerator()
        {
            yield return this;
        }

        void IConcatList<RowItemCollection>.Sort(Comparison<RowItemCollection> comparision)
        {
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
            get { return _autoSizeItems ?? (_autoSizeItems = FilterAutoSizeItems(x => x.IsAutoSize)); }
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
    }
}
