using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region NewGuid

        private class NewGuidFunction : ScalarFunctionExpression<Guid?>
        {
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
