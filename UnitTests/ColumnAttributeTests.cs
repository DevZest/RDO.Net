
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdventureWorksLT;
using DevZest.Data.SqlServer;

namespace DevZest.Data
{
    [TestClass]
    public class ColumnAttributeTests
    {
        private sealed class MyTable : Model<MyTable.Key>
        {
            public sealed class Key : ModelKey
            {
                public Key(_Int32 id)
                {
                    Id = id;
                }

                public _Int32 Id { get; private set; }
            }

            public static Accessor<MyTable, _Int32> IdAccessor = RegisterColumn((MyTable x) => x.Id);
            public static Accessor<MyTable, _Boolean> IsActiveAccessor = RegisterColumn((MyTable x) => x.IsActive, c => c.DefaultValue(true));

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

            [Nullable(false)]
            public _Boolean IsActive { get; private set; }
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
