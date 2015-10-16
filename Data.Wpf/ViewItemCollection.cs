using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Wpf
{
    public sealed class ViewItemCollection : ReadOnlyCollection<ViewItem>
    {
        internal ViewItemCollection(DataSetView owner)
            : base(new List<ViewItem>())
        {
            Debug.Assert(owner != null);
            _owner = owner;
        }

        private DataSetView _owner;

        internal GridRange CalculatedDataRowRange { get; private set; }

        internal void Add(ViewItem viewItem, GridRange gridRange)
        {
            Debug.Assert(viewItem != null);
            Debug.Assert(viewItem.Owner == null);
            Items.Add(viewItem.Initialize(_owner, gridRange));
            var kind = viewItem.Kind;
            if (kind == ViewItemKind.RowSelector || kind == ViewItemKind.ColumnValue || kind == ViewItemKind.ChildSet)
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
