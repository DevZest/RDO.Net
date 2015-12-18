using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data.Windows
{
    public class GridSpecCollection<T> : ReadOnlyCollection<T>
        where T : GridSpec
    {
        internal GridSpecCollection()
            : base(new List<T>())
        {
        }

        internal void Add(T item)
        {
            Items.Add(item);
        }
    }
}
