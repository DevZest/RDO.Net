using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region GetDate

        [ExpressionConverterNonGenerics(typeof(GetDateFunction.Converter), Id = "GetDate()")]
        private sealed class GetDateFunction : ScalarFunctionExpression<DateTime?>
        {
            private sealed class Converter : ConverterBase<GetDateFunction>
            {
                protected override GetDateFunction MakeExpression()
                {
                    return new GetDateFunction();
                }
            }

            public override DateTime? this[DataRow dataRow]
            {
                get { return DateTime.Now; }
            }

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.GetDate; }
            }
        }

        public static _DateTime GetDate()
        {
            return new GetDateFunction().MakeColumn<_DateTime>();
        }

        #endregion
    }
}
