using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region Count

        private sealed class CountFunction : AggregateFunctionExpression<Int32?>
        {
            public CountFunction(Column x)
                : base(x)
            {
            }

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.Count; }
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
                    if (!Param.IsNull(dataRow))
                        count++;
                }
            }

            protected override int? EvalReturn()
            {
                return count;
            }
        }

        /// <summary>
        /// Counts number of non null values in specified column.
        /// </summary>
        /// <param name="x">The specified column.</param>
        /// <returns>Number of non null values in specified column.</returns>
        public static _Int32 Count(this Column x)
        {
            return new CountFunction(x).MakeColumn<_Int32>();
        }

        #endregion
    }
}
