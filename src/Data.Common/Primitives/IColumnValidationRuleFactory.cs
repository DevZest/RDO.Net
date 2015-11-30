using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public interface IColumnValidationRuleFactory
    {
        IEnumerable<ValidationRule> GetValidationRules(Column column);
    }
}
