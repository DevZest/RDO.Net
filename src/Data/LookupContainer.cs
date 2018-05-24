using System;
using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public abstract class LookupContainer : ProjectionContainer<LookupBase>
    {
        internal sealed override void PreventExternalAssemblyInheritance()
        {
        }
    }
}
