using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DevZest.Data
{
    [TestClass]
    public class DbSessionTests
    {
        public class SimpleModel : SimpleModelBase
        {
        }

        public abstract class Db : DbSession
        {
            internal DbTable<SimpleModel> _simpleModel;
            public DbTable<SimpleModel> SimpleModel
            {
                get { return GetTable(ref _simpleModel, "SimpleModel"); }
            }
        }

        [TestMethod]
        public void DbSession_get_table()
        {
            var mock = new Mock<Db>();
            mock.CallBase = true;
            using (var db = mock.Object)
            {
                Assert.IsNull(db._simpleModel);
                Assert.AreEqual(DataSourceKind.DbTable, db.SimpleModel._.DataSource.Kind);
                var dbTableExpr = (DbTableClause)db.SimpleModel.FromClause;
                Assert.AreEqual("SimpleModel", dbTableExpr.Name);
                Assert.IsNotNull(db._simpleModel);
            }
        }
    }
}
