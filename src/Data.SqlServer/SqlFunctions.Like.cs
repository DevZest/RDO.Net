using DevZest.Data.Primitives;
using System;

namespace DevZest.Data.SqlServer
{
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

        public static _Boolean Like(this _String source, _String pattern)
        {
            source.VerifyNotNull(nameof(source));
            pattern.VerifyNotNull(nameof(pattern));
            return new LikeFunction(source, pattern).MakeColumn<_Boolean>();
        }
    }
}
