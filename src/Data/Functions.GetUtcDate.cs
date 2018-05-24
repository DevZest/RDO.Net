using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region GetUtcDate

        private class GetUtcDateFunction : ScalarFunctionExpression<DateTime?>
        {
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
