using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Diagnostics;

namespace DevZest.Data
{
    public abstract class DataRowFilter : DataRowCriteria
    {
        public static DataRowFilter Create<T>(Func<T, DataRow, bool> condition)
            where T : Model
        {
            Check.NotNull(condition, nameof(condition));
            if (condition.Target != null)
                throw new ArgumentException(Strings.DataRowCriteria_ExpressionMustBeStatic, nameof(condition));
            return new FuncFilter<T>(condition);
        }

        private DataRowFilter()
        {
        }

        public abstract bool Evaluate(DataRow dataRow);

        private sealed class FuncFilter<T> : DataRowFilter
            where T : Model
        {
            public FuncFilter(Func<T, DataRow, bool> condition)
            {
                Debug.Assert(condition != null && condition.Target == null);
                _condition = condition;
            }

            private readonly Func<T, DataRow, bool> _condition;

            public override bool Evaluate(DataRow dataRow)
            {
                Check.NotNull(dataRow, nameof(dataRow));
                return _condition((T)dataRow.Model, dataRow);
            }

            public override Type ModelType
            {
                get { return typeof(T); }
            }
        }
    }
}
