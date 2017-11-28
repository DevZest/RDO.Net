using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.SqlServer;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class ColumnAttributeTests
    {
        private sealed class MyTable : Model<MyTable.Key>
        {
            public sealed class Key : KeyBase
            {
                public Key(_Int32 id)
                {
                    Id = id;
                }

                public _Int32 Id { get; private set; }
            }

            public static Mounter<_Int32> _Id = RegisterColumn((MyTable x) => x.Id);
            public static Mounter<_Boolean> _IsActive = RegisterColumn((MyTable x) => x.IsActive);

            public MyTable()
            {
                _primaryKey = new Key(Id);
            }

            private Key _primaryKey;
            public sealed override Key PrimaryKey
            {
                get { return _primaryKey; }
            }

            [Identity(1, 1)]
            public _Int32 Id { get; private set; }

            [Required]
            public _Boolean IsActive { get; private set; }

            [ColumnInitializer(nameof(IsActive))]
            private static void InitializeIsActive(_Boolean isActive)
            {
                isActive.SetDefault(true);
            }
        }

        private sealed class MyDb : Db
        {
            public MyDb(SqlVersion sqlVersion)
                : base(sqlVersion)
            {
            }

            private DbTable<MyTable> _myTable;
            public DbTable<MyTable> MyTable
            {
                get { return GetTable(ref _myTable, "MyTable"); }
            }
        }

        [TestMethod]
        public void ColumnAttribute_generate_create_table_sql()
        {
            using (var myDb = new MyDb(SqlVersion.Sql11))
            {
                var command = myDb.GetCreateTableCommand(new MyTable(), "MyTable", false);
                var expectedSql =
@"CREATE TABLE [MyTable] (
    [Id] INT NOT NULL IDENTITY(1, 1),
    [IsActive] BIT NOT NULL DEFAULT(1)

    PRIMARY KEY CLUSTERED ([Id])
);
";
                Assert.AreEqual(expectedSql, command.CommandText);
            }
        }
    }
}
