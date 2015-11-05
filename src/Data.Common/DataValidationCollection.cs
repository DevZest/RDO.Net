using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data
{
    public sealed class DataValidationCollection : ReadOnlyCollection<DataValidation>
    {
        internal DataValidationCollection(Model model)
            : base(new List<DataValidation>())
        {
            Debug.Assert(model != null);
            Model = model;
        }

        private Model Model { get; set; }

        internal void Add(DataValidation item)
        {
            Debug.Assert(Model.DesignMode);

            if (item != null)
                Items.Add(item);
        }
    }
}
