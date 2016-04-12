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

        internal IEnumerable<T> AutoSizeItems
        {
            get { return this.Where(x => x.IsAutoSize).OrderBy(x => x.AutoSizeMeasureOrder); }
        }
    }
}
