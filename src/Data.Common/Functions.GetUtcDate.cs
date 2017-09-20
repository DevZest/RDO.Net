using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region GetUtcDate

        [ExpressionConverterNonGenerics(typeof(GetUtcDateFunction.Converter), Id = "GetUtcDate()")]
        private class GetUtcDateFunction : ScalarFunctionExpression<DateTime?>
        {
            private sealed class Converter : ConverterBase<GetUtcDateFunction>
            {
                protected override GetUtcDateFunction MakeExpression()
                {
                    return new GetUtcDateFunction();
                }
            }

            public override DateTime? this[DataRow dataRow]
            {
                get { return DateTime.UtcNow; }
            }

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.GetUtcDate; }
            }
        }

        public static _DateTime GetUtcDate()
        {
            return new GetUtcDateFunction().MakeColumn<_DateTime>();
        }

        #endregion
    }
}
