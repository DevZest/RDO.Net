using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Wpf
{
    internal class ViewManagerCollection : ReadOnlyCollection<ViewManager>
    {
        internal ViewManagerCollection(DataSetControl owner)
            : base(new List<ViewManager>())
        {
            Debug.Assert(owner != null);
            _owner = owner;
        }

        private DataSetControl _owner;

        internal void Add(ViewManager viewManager, GridRange gridRange)
        {
            Debug.Assert(viewManager != null);
            Debug.Assert(viewManager.Owner == null);
            Items.Add(viewManager.Initialize(_owner, gridRange));
        }

        internal void Clear()
        {
            foreach (var viewManager in this)
                viewManager.Clear();
            Items.Clear();
        }
    }
}
