using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data
{
    internal sealed class ColumnValidationMessageCollection : ReadOnlyCollection<ValidationMessage>
    {
        public static readonly ColumnValidationMessageCollection Empty = new ColumnValidationMessageCollection();

        public ColumnValidationMessageCollection()
            : base(new List<ValidationMessage>())
        {
        }

        public void Add(ValidationMessage item)
        {
            Debug.Assert(this != Empty);
            Items.Add(item);
        }
    }
}
