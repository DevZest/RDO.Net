
using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IValidator
    {
        ValidationMessage Validate(DataRow dataRow);
    }
}
