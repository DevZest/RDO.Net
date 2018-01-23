using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Presenters.Primitives
{
    internal abstract class BindingCollection<T> : ReadOnlyCollection<T>
        where T : Binding, IConcatList<T>
    {
        internal BindingCollection()
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

        internal virtual void InvalidateAutoWidthBindings()
        {
            ReadOnlyCollection<T> collection = this;
            collection.ForEach(x => x.InvalidateAutoWidthGridColumns());
        }

        internal virtual void InvalidateAutoHeightBindings()
        {
            ReadOnlyCollection<T> collection = this;
            collection.ForEach(x => x.InvalidateAutoHeightGridRows());
        }

        private static int CompareByAutoSizeOrder(T x, T y)
        {
            Debug.Assert(x != null && y != null);
            var result = x.AutoSizeOrder.CompareTo(y.AutoSizeOrder);
            if (result == 0)
                result = x.Ordinal.CompareTo(y.Ordinal);
            return result;
        }

        protected IConcatList<T> FilterAutoSizeBindings(Func<T, bool> predict)
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

        internal void EndSetup()
        {
            foreach (var binding in this)
                binding.EndSetup();
        }

        internal void Clear()
        {
            base.Items.Clear();
        }
    }
}
