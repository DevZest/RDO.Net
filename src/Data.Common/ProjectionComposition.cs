using System;
using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public abstract class ProjectionComposition : ProjectionContainer<Projection>
    {
        internal sealed override Type CompositionType
        {
            get { return typeof(Projection); }
        }
    }
}
