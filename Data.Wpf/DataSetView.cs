using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Wpf
{
    public sealed class DataSetView
    {
        internal DataSetView(DataRowView owner, GridTemplate template)
        {
            Debug.Assert(template != null);
            Debug.Assert(owner == null || template.Model.GetParentModel() == owner.Model);
            Template = template;
        }

        public GridTemplate Template { get; private set; }

        public Model Model
        {
            get { return Template.Model; }
        }
    }
}
