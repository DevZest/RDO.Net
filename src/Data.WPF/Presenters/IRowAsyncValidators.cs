using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    public interface IRowAsyncValidators : IReadOnlyList<RowAsyncValidator>
    {
        bool IsSealed { get; }
        IRowAsyncValidators Seal();
        IRowAsyncValidators Add(RowAsyncValidator value);
        RowAsyncValidator this[IColumns sourceColumns] { get; }
    }
}
