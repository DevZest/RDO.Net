using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class TemplateItemCollection<T> : ReadOnlyCollection<T>, IConcatList<TemplateItemCollection<T>>
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
            ReadOnlyCollection<T> collection = this;
            collection.ForEach(x => x.InvalidateAutoWidthGridColumns());
        }

        internal void InvalidateAutoHeightItems()
        {
            _autoSizeItems = null;
            ReadOnlyCollection<T> collection = this;
            collection.ForEach(x => x.InvalidateAutoHeightGridRows());
        }

        internal IReadOnlyList<T> AutoSizeItems
        {
            get
            {
                ReadOnlyCollection<T> collection = this;
                return _autoSizeItems ?? (_autoSizeItems = collection.Where(x => x.IsAutoSize).OrderBy(x => x.AutoSizeMeasureOrder).ToArray());
            }
        }

        #region IConcatList<TemplateCollection<T>>

        IEnumerator<TemplateItemCollection<T>> IEnumerable<TemplateItemCollection<T>>.GetEnumerator()
        {
            yield return this;
        }

        bool IConcatList<TemplateItemCollection<T>>.IsReadOnly
        {
            get { return true; }
        }

        int IReadOnlyCollection<TemplateItemCollection<T>>.Count
        {
            get { return 1; }
        }

        TemplateItemCollection<T> IReadOnlyList<TemplateItemCollection<T>>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }
        #endregion
    }
}
