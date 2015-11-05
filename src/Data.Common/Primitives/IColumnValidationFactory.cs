using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public interface IColumnValidationFactory
    {
        IEnumerable<DataValidation> GetValidations(Column column);
    }
}
