using System;
using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public abstract class RefContainer : ProjectionContainer<Ref>
    {
        internal sealed override Type ChildType
        {
            get { return typeof(Ref); }
        }
    }
}
