using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region NewGuid

        [ExpressionConverterNonGenerics(typeof(NewGuidFunction.Converter), Id = "NewGuid()")]
        private class NewGuidFunction : ScalarFunctionExpression<Guid?>
        {
            private sealed class Converter : ConverterBase<NewGuidFunction>
            {
                protected override NewGuidFunction MakeExpression()
                {
                    return new NewGuidFunction();
                }
            }

            public override Guid? this[DataRow dataRow]
            {
                get { return Guid.NewGuid(); }
            }

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.NewGuid; }
            }
        }

        public static _Guid NewGuid()
        {
            return new NewGuidFunction().MakeColumn<_Guid>();
        }

        #endregion
    }
}
