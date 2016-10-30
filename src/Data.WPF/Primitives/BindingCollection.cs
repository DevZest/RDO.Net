using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
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
            return x.AutoSizeOrder.CompareTo(y.AutoSizeOrder);
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

        internal void BeginSetup()
        {
            foreach (var binding in this)
                binding.BeginSetup();
        }

        internal void EndSetup()
        {
            foreach (var binding in this)
                binding.EndSetup();
        }
    }
}
