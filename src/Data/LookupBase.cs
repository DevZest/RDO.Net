using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public abstract class LookupBase : Projection
    {
        internal sealed override void PreventExternalAssemblyInheritance()
        {
        }
    }
}
