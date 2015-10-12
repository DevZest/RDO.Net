using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Wpf
{
    internal class ViewGeneratorCollection : ReadOnlyCollection<ViewGenerator>
    {
        internal ViewGeneratorCollection(DataSetControl owner)
            : base(new List<ViewGenerator>())
        {
            Debug.Assert(owner != null);
            _owner = owner;
        }

        private DataSetControl _owner;

        internal void Add(ViewGenerator viewGenerator, GridRange gridRange)
        {
            Debug.Assert(viewGenerator != null);
            Debug.Assert(viewGenerator.Owner == null);
            Items.Add(viewGenerator.Initialize(_owner, gridRange));
        }

        internal void Clear()
        {
            foreach (var viewGenerator in this)
                viewGenerator.Clear();
            Items.Clear();
        }
    }
}
