using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data
{
    public sealed class ModelCollection : ReadOnlyCollection<Model>
    {
        internal ModelCollection(Model parentModel)
            : base(new List<Model>())
        {
            ParentModel = parentModel;
        }

        public Model ParentModel { get; private set; }

        internal void Add(Model model)
        {
            model.Ordinal = Items.Count;
            Items.Add(model);
        }
    }
}
