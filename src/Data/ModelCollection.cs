using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a collection of <see cref="Model"/> objects.
    /// </summary>
    public sealed class ModelCollection : ReadOnlyCollection<Model>
    {
        internal ModelCollection(Model parentModel)
            : base(new List<Model>())
        {
            ParentModel = parentModel;
        }

        /// <summary>
        /// Gets the parent model which owns this collection.
        /// </summary>
        public Model ParentModel { get; private set; }

        internal void Add(Model model)
        {
            model.Ordinal = Items.Count;
            Items.Add(model);
        }
    }
}
