using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Wpf
{
    internal class ViewGeneratorCollection : ReadOnlyCollection<ViewGenerator>
    {
        internal ViewGeneratorCollection(DataSetControl dataSetControl)
            : base(new List<ViewGenerator>())
        {
            Debug.Assert(dataSetControl != null);
            _dataSetControl = dataSetControl;
        }

        private DataSetControl _dataSetControl;

        internal void Add(ViewGenerator viewGenerator)
        {
            Debug.Assert(viewGenerator != null);
            Debug.Assert(viewGenerator.DataSetControl == null);
            Items.Add(viewGenerator);
            viewGenerator.DataSetControl = _dataSetControl;
        }

        internal void Clear()
        {
            foreach (var viewGenerator in this)
                viewGenerator.DataSetControl = null;
            Items.Clear();
        }
    }
}
