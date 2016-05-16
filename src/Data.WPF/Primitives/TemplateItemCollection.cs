using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class TemplateItemCollection<T> : ReadOnlyCollection<T>
        where T : TemplateItem, IConcatList<T>
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

        private static int CompareByAutoSizeOrder(T x, T y)
        {
            Debug.Assert(x != null && y != null);
            return x.AutoSizeOrder.CompareTo(y.AutoSizeOrder);
        }

        protected IConcatList<T> FilterAutoSizeItems(Func<T, bool> predict)
        {
            var result = ConcatList<T>.Empty;
            foreach (var templateItem in this)
            {
                if (predict(templateItem))
                    result = result.Concat(templateItem);
            }

            result.Sort(CompareByAutoSizeOrder);
            return result;
        }
    }
}
