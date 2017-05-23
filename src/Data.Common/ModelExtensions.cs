using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public static class ModelExtensions
    {
        public static DataRowFilter Where<T>(this T _, Func<T, DataRow, bool> condition)
            where T : Model
        {
            Check.NotNull(_, nameof(_));
            Check.NotNull(condition, nameof(condition));
            if (condition.Target != null)
                throw new ArgumentException(Strings.ModelExtensions_ExpressionMustBeStatic, nameof(condition));

            return DataRowFilter.Create(condition);
        }

        public static DataRowSort OrderBy<T>(this T _, Func<T, DataRow, DataRow, int> comparer)
            where T : Model
        {
            Check.NotNull(_, nameof(_));
            Check.NotNull(comparer, nameof(comparer));
            if (comparer.Target != null)
                throw new ArgumentException(Strings.ModelExtensions_ExpressionMustBeStatic, nameof(comparer));

            return DataRowSort.Create(comparer);
        }

        public static DataRowSort OrderBy<T>(this T _, Func<DataRowComparing, T, DataRowCompared> comparer)
            where T : Model
        {
            Check.NotNull(_, nameof(_));
            Check.NotNull(comparer, nameof(comparer));
            if (comparer.Target != null)
                throw new ArgumentException(Strings.ModelExtensions_ExpressionMustBeStatic, nameof(comparer));

            return DataRowSort.Create(comparer);
        }
    }
}
