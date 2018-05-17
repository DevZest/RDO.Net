using System;
using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public abstract class ProjectionComposition : Composition<Projection>
    {
        internal sealed override Type CompositionType
        {
            get { return typeof(Projection); }
        }
    }
}
