using System;
using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public abstract class RefComposition : Composition<Ref>
    {
        internal sealed override Type CompositionType
        {
            get { return typeof(Ref); }
        }
    }
}
