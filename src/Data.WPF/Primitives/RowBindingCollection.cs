using System;
using System.Collections.Generic;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class RowBindingCollection : BindingCollection<RowBinding>, IConcatList<RowBindingCollection>
    {
        #region IConcatList<RowBindingCollection>

        IEnumerator<RowBindingCollection> IEnumerable<RowBindingCollection>.GetEnumerator()
        {
            yield return this;
        }

        void IConcatList<RowBindingCollection>.Sort(Comparison<RowBindingCollection> comparision)
        {
        }

        int IReadOnlyCollection<RowBindingCollection>.Count
        {
            get { return 1; }
        }

        RowBindingCollection IReadOnlyList<RowBindingCollection>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }
        #endregion

        private IReadOnlyList<RowBinding> _autoSizeItems;

        internal IReadOnlyList<RowBinding> AutoSizeItems
        {
            get { return _autoSizeItems ?? (_autoSizeItems = FilterAutoSizeBindings(x => x.IsAutoSize)); }
        }

        internal override void InvalidateAutoHeightBindings()
        {
            _autoSizeItems = null;
            base.InvalidateAutoHeightBindings();
        }

        internal override void InvalidateAutoWidthBindings()
        {
            _autoSizeItems = null;
            base.InvalidateAutoWidthBindings();
        }
    }
}
