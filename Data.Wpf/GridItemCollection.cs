using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Wpf
{
    public sealed class GridItemCollection : ReadOnlyCollection<GridItem>
    {
        internal GridItemCollection(GridView owner)
            : base(new List<GridItem>())
        {
            Debug.Assert(owner != null);
            _owner = owner;
        }

        private GridView _owner;

        internal GridRange CalculatedDataRowRange { get; private set; }

        internal void Add(GridItem viewItem, GridRange gridRange)
        {
            Debug.Assert(viewItem != null);
            Debug.Assert(viewItem.Owner == null);
            Items.Add(viewItem.Initialize(_owner, gridRange));
            if (!(viewItem is IScalarViewItem))
                CalculatedDataRowRange = CalculatedDataRowRange.Union(gridRange);
        }

        internal void Clear()
        {
            foreach (var viewItem in this)
                viewItem.Clear();
            Items.Clear();
            CalculatedDataRowRange = new GridRange();
        }
    }
}
