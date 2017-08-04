using System.Collections.Generic;

namespace DevZest.Data.Presenters.Primitives
{
    public interface ICompositeBinding
    {
        IReadOnlyList<Binding> Bindings { get; }
        IReadOnlyList<string> Names { get; }
    }
}
