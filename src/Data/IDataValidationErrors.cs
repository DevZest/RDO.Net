using DevZest.Data.Primitives;
using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IDataValidationErrors : IReadOnlyList<DataValidationError>
    {
        IDataValidationErrors Seal();
        IDataValidationErrors Add(DataValidationError value);
    }
}
