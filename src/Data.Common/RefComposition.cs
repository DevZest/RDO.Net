using System;
using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public abstract class RefComposition : ProjectionContainer<Ref>
    {
        internal sealed override Type CompositionType
        {
            get { return typeof(Ref); }
        }
    }
}
