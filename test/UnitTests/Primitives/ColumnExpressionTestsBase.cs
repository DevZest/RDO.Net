using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Primitives
{
    public abstract class ColumnExpressionTestsBase
    {
        [TestInitialize]
        public void Initialize()
        {
            ColumnConverter.EnsureInitialized(typeof(_Int32));
        }
    }
}
