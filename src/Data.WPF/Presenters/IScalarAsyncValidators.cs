using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data.Presenters
{
    public interface IScalarAsyncValidators : IReadOnlyList<ScalarAsyncValidator>
    {
        bool IsSealed { get; }
        IScalarAsyncValidators Seal();
        IScalarAsyncValidators Add(ScalarAsyncValidator value);
    }
}
