using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    public abstract class ColumnConverterTestsBase
    {
        [TestInitialize]
        public void Initialize()
        {
            ColumnConverter.EnsureInitialized(typeof(_Int32));
        }
    }
}
