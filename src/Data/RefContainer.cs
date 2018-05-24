using System;
using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public abstract class RefContainer : ProjectionContainer<RefBase>
    {
        internal sealed override void PreventExternalAssemblyInheritance()
        {
        }
    }
}
