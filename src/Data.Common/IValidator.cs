
using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IValidator
    {
        IEnumerable<ValidationMessage> Validate(DataRow dataRow);
    }
}
