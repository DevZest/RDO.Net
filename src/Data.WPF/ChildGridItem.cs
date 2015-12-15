using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class ChildGridItem : GridItem
    {
        internal ChildGridItem(GridTemplate template)
                : base(template.Model.GetParentModel())
        {
            Template = template;
        }


        public GridTemplate Template { get; private set; }
    }
}
