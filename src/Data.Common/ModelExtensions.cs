using System;

namespace DevZest.Data
{
    public static class ModelExtensions
    {
        public static Filter Where<T>(this T _, Func<T, DataRow, bool> condition)
            where T : Model
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            if (condition.Target != null)
                throw new ArgumentException(Strings.ModelExtensions_ExpressionMustBeStatic, nameof(condition));

            return Filter.Create(condition);
        }
    }
}
