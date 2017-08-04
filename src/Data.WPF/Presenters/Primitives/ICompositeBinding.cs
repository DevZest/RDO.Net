using System.Collections.Generic;

namespace DevZest.Data.Presenters.Primitives
{
    internal interface ICompositeBinding
    {
        IReadOnlyList<Binding> Bindings { get; }
        IReadOnlyList<string> Names { get; }
    }
}
