using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region CountRows

        private sealed class CountRowsFunction : AggregateFunctionExpression<Int32?>
        {
            public CountRowsFunction(Column x)
                : base(x)
            {
            }

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.CountRows; }
            }

            int count;
            protected override void EvalInit()
            {
                count = 0;
            }

            protected override void EvalAccumulate(DataRow dataRow)
            {
                checked
                {
                    count++;
                }
            }

            protected override int? EvalReturn()
            {
                return count;
            }
        }

        public static _Int32 CountRows(this Column x)
        {
            return new CountRowsFunction(x).MakeColumn<_Int32>();
        }

        #endregion
    }
}
