using DevZest.Data.Primitives;
using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    public abstract class ColumnConverterTestsBase
    {
        [TestInitialize]
        public void Initialize()
        {
            ColumnConverter.EnsureInitialized(typeof(_Int32));
            ColumnConverter.EnsureInitialized(typeof(_DateTimeOffset));
        }
    }
}
