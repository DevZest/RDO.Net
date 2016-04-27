using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class TemplateItemCollectionBase<T> : ReadOnlyCollection<T>
        where T : TemplateItem
    {
        internal TemplateItemCollectionBase()
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

        internal virtual void InvalidateAutoWidthItems()
        {
            ReadOnlyCollection<T> collection = this;
            collection.ForEach(x => x.InvalidateAutoWidthGridColumns());
        }

        internal virtual void InvalidateAutoHeightItems()
        {
            ReadOnlyCollection<T> collection = this;
            collection.ForEach(x => x.InvalidateAutoHeightGridRows());
        }
    }
}
