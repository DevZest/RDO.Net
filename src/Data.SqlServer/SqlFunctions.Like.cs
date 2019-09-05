using DevZest.Data.Primitives;
using System;

namespace DevZest.Data.SqlServer
{
    /// <summary>
    /// Provides SQL Server specific functions.
    /// </summary>
    public static partial class SqlFunctions
    {
        private sealed class LikeFunction : ScalarFunctionExpression<bool?>
        {
            public LikeFunction(_String source, _String pattern)
                : base(source, pattern)
            {
            }

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.Like; }
            }

            public override bool? this[DataRow dataRow]
            {
                get { throw new NotSupportedException("Like function is not supported by DataSet."); }
            }
        }

        /// <summary>
        /// Determines whether a specific character string matches a specified pattern.
        /// </summary>
        /// <param name="source">The source string column.</param>
        /// <param name="pattern">The pattern.</param>
        /// <returns><see cref="_Boolean"/> column that contains the result.</returns>
        public static _Boolean Like(this _String source, _String pattern)
        {
            source.VerifyNotNull(nameof(source));
            pattern.VerifyNotNull(nameof(pattern));
            return new LikeFunction(source, pattern).MakeColumn<_Boolean>();
        }
    }
}
