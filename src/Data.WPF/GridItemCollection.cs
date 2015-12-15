using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Windows
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
            Debug.Assert(gridItem != null && gridItem.Owner == null);
            gridItem.Seal(_owner, gridRange);
            Items.Add(gridItem);
            Range = Range.Union(gridRange);
        }
    }
}
