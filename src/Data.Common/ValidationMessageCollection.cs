using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data
{
    internal sealed class ValidationMessageCollection : ReadOnlyCollection<ValidationMessage<IColumnSet>>
    {
        public static readonly ValidationMessageCollection Empty = new ValidationMessageCollection();

        public ValidationMessageCollection()
            : base(new List<ValidationMessage<IColumnSet>>())
        {
        }

        public void Add(ValidationMessage<IColumnSet> item)
        {
            Debug.Assert(this != Empty);
            Items.Add(item);
        }
    }
}
