using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data
{
    internal sealed class ModelValidationMessageCollection : ReadOnlyCollection<ModelValidationMessage>
    {
        public static readonly ModelValidationMessageCollection Empty = new ModelValidationMessageCollection();

        public ModelValidationMessageCollection()
            : base(new List<ModelValidationMessage>())
        {
        }

        public void Add(ModelValidationMessage item)
        {
            Debug.Assert(this != Empty);
            Items.Add(item);
        }
    }
}
