using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data
{
    internal sealed class ColumnValidationMessageCollection : ReadOnlyCollection<ColumnValidationMessage>
    {
        public static readonly ColumnValidationMessageCollection Empty = new ColumnValidationMessageCollection();

        public ColumnValidationMessageCollection()
            : base(new List<ColumnValidationMessage>())
        {
        }

        public void Add(ColumnValidationMessage item)
        {
            Debug.Assert(this != Empty);
            Items.Add(item);
        }
    }
}
