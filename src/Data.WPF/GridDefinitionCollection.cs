using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data.Windows
{
    public class GridDefinitionCollection<T> : ReadOnlyCollection<T>
        where T : GridDefinition
    {
        internal GridDefinitionCollection()
            : base(new List<T>())
        {
        }

        internal void Add(T item)
        {
            Items.Add(item);
        }
    }
}
