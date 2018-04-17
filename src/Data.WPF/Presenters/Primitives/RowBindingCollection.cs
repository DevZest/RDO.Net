using System;
using System.Collections.Generic;

namespace DevZest.Data.Presenters.Primitives
{
    internal sealed class RowBindingCollection : BindingCollection<RowBinding>
    {
        private IReadOnlyList<RowBinding> _autoSizeItems;

        internal IReadOnlyList<RowBinding> AutoSizeItems
        {
            get { return _autoSizeItems ?? (_autoSizeItems = FilterAutoSizeBindings(x => x.IsAutoSize).Seal()); }
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
