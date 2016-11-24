using DevZest.Data.Windows.Primitives;
using System.Collections.Generic;

namespace DevZest.Data.Windows
{
    public interface IScalarSet : IReadOnlyList<Scalar>
    {
        bool Contains(Scalar scalar);
    }
}
