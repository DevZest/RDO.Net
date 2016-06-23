using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data
{
    internal sealed class ValidationMessageCollection : ReadOnlyCollection<ValidationMessage>
    {
        public static readonly ValidationMessageCollection Empty = new ValidationMessageCollection();

        public ValidationMessageCollection()
            : base(new List<ValidationMessage>())
        {
        }

        public void Add(ValidationMessage item)
        {
            Items.Add(item);
        }
    }
}
