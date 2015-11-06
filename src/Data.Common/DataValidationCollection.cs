using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data
{
    public sealed class DataValidationCollection : ReadOnlyCollection<DataValidation>
    {
        internal DataValidationCollection()
            : base(new List<DataValidation>())
        {
        }

        internal void Add(DataValidation item)
        {
            if (item != null)
                Items.Add(item);
        }

        internal void Clear()
        {
            Items.Clear();
        }
    }
}
