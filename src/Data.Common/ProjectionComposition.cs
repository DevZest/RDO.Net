using System;
using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public abstract class ProjectionComposition : ProjectionContainer<Projection>
    {
        internal sealed override Type ChildType
        {
            get { return typeof(Projection); }
        }
    }
}
