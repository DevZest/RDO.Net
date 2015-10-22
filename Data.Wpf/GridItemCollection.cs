using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Wpf
{
    public sealed class GridItemCollection<T> : ReadOnlyCollection<T>
        where T : GridItem
    {
        internal GridItemCollection(GridTemplate owner)
            : base(new List<T>())
        {
            Debug.Assert(owner != null);
            _owner = owner;
        }

        private GridTemplate _owner;

        internal GridRange Range { get; private set; }

        internal void Add(T gridItem, GridRange gridRange)
        {
            Debug.Assert(gridItem != null);
            Debug.Assert(gridItem.Owner == null);
            gridItem.Initialize(_owner, gridRange);
            Items.Add(gridItem);
            Range = Range.Union(gridRange);
        }

        internal void Clear()
        {
            foreach (var viewItem in this)
                viewItem.Clear();
            Items.Clear();
            Range = new GridRange();
        }
    }
}
