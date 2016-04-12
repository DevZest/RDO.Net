using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class TemplateItemCollection<T> : ReadOnlyCollection<T>
        where T : TemplateItem
    {
        internal TemplateItemCollection()
            : base(new List<T>())
        {
        }

        internal GridRange Range { get; private set; }

        internal void Add(GridRange gridRange, T item)
        {
            Debug.Assert(item != null);
            Items.Add(item);
            Range = Range.Union(gridRange);
        }

        private IReadOnlyList<T> _autoSizeItems;
        internal void InvalidateAutoWidthItems()
        {
            _autoSizeItems = null;
            this.ForEach(x => x.InvalidateAutoWidthGridColumns());
        }

        internal void InvalidateAutoHeightItems()
        {
            _autoSizeItems = null;
            this.ForEach(x => x.InvalidateAutoHeightGridRows());
        }

        internal IReadOnlyList<T> AutoSizeItems
        {
            get {  return _autoSizeItems ?? (_autoSizeItems = this.Where(x => x.IsAutoSize).OrderBy(x => x.AutoSizeMeasureOrder).ToArray()); }
        }
    }
}
