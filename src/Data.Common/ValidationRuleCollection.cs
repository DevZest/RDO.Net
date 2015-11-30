using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data
{
    public sealed class ValidationRuleCollection : ReadOnlyCollection<ValidationRule>
    {
        internal ValidationRuleCollection()
            : base(new List<ValidationRule>())
        {
        }

        internal void Add(ValidationRule item)
        {
            if (item != null)
                Items.Add(item);
        }
    }
}
