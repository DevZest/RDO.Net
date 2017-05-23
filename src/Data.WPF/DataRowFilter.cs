using DevZest.Data;
using System;
using System.Diagnostics;

namespace DevZest.Windows
{
    public abstract class DataRowFilter
    {
        internal static DataRowFilter Create<T>(Func<T, DataRow, bool> condition)
            where T : Model
        {
            Debug.Assert(condition != null && condition.Target == null);
            return new FuncFilter<T>(condition);
        }

        private DataRowFilter()
        {
        }

        internal abstract bool Evaluate(DataRow dataRow);

        public abstract Type ModelType { get; }

        private sealed class FuncFilter<T> : DataRowFilter
            where T : Model
        {
            public FuncFilter(Func<T, DataRow, bool> condition)
            {
                Debug.Assert(condition != null && condition.Target == null);
                _condition = condition;
            }

            private readonly Func<T, DataRow, bool> _condition;

            internal override bool Evaluate(DataRow dataRow)
            {
                return _condition((T)dataRow.Model, dataRow);
            }

            public override Type ModelType
            {
                get { return typeof(T); }
            }
        }
    }
}
