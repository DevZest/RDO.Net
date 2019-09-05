using DevZest.Data.Addons;
using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics;

namespace DevZest.Data.SqlServer
{
    [TestClass]
    public class ModelExtensionsTests
    {
        private sealed class TempModel : Model<TempModel.PK>
        {
            public sealed class PK : CandidateKey
            {
                public PK(_Int32 id)
                    : base(id)
                {
                }
            }

            public static readonly Mounter<_Int32> _Id = RegisterColumn((TempModel x) => x.Id);
            public static readonly Mounter<_String> _Name = RegisterColumn((TempModel x) => x.Name);
            public static readonly Mounter<_Int32> _Unique1 = RegisterColumn((TempModel x) => x.Unique1);
            public static readonly Mounter<_Int32> _Unique2 = RegisterColumn((TempModel x) => x.Unique2);

            public TempModel()
            {
                FkRef = new PK(Unique1);
                Name.SetDefaultValue("DEFAULT NAME", null, null);
                AddDbUniqueConstraint("UQ_Temp", null, false, Unique1, Unique2.Desc());
                AddDbCheckConstraint("CK_Temp", null, Name.IsNotNull());
                this.AddDbTableConstraint(CreateForeignKeyConstraint(null, null, FkRef, this, ForeignKeyRule.None, ForeignKeyRule.None), false);
            }

            private static DbForeignKeyConstraint CreateForeignKeyConstraint<TKey>(string name, string description, TKey foreignKey, Model<TKey> refTableModel, ForeignKeyRule deleteRule, ForeignKeyRule updateRule)
                where TKey : CandidateKey
            {
                Debug.Assert(foreignKey != null);
                Debug.Assert(refTableModel != null);

                var model = foreignKey.ParentModel;
                var foreignKeyConstraint = new DbForeignKeyConstraint(name, description, foreignKey, refTableModel.PrimaryKey, deleteRule, updateRule);
                Debug.Assert(!(refTableModel != model && string.IsNullOrEmpty(foreignKeyConstraint.ReferencedTableName)));
                return foreignKeyConstraint;
            }

            protected sealed override PK CreatePrimaryKey()
            {
                return new PK(Id);
            }

            public _Int32 Id { get; private set; }

            public new _String Name { get; private set; }

            public _Int32 Unique1 { get; private set; }

            public _Int32 Unique2 { get; private set; }

            public PK FkRef { get; private set; }

        }

        [TestMethod]
        public void Model_GenerateCreateTableSql_permanent_table()
        {
            var temp = new TempModel();
            DbTable<TempModel>.Create(temp, new Mock<DbSession>().Object, "Test");
            var sqlBuilder = new IndentedStringBuilder();

            temp.GenerateCreateTableSql(sqlBuilder, SqlVersion.Sql13, false);
            var expectedSql =
@"CREATE TABLE [Test] (
    [Id] INT NOT NULL,
    [Name] NVARCHAR(4000) NULL DEFAULT(N'DEFAULT NAME'),
    [Unique1] INT NULL,
    [Unique2] INT NULL

    CONSTRAINT [PK_Test] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_Temp] UNIQUE NONCLUSTERED ([Unique1], [Unique2] DESC),
    CONSTRAINT [CK_Temp] CHECK ([Name] IS NOT NULL),
    FOREIGN KEY ([Unique1])
        REFERENCES [Test] ([Id])
        ON DELETE NO ACTION
        ON UPDATE NO ACTION
);
";
            Assert.AreEqual(expectedSql, sqlBuilder.ToString());
        }

        [TestMethod]
        public void Model_GenerateCreateTableSql_temp_table()
        {
            var temp = new TempModel();
            temp.AddTempTableIdentity();
            var sqlBuilder = new IndentedStringBuilder();

            temp.GenerateCreateTableSql("#Temp", null, sqlBuilder, SqlVersion.Sql13, true);
            var expectedSql =
@"CREATE TABLE [#Temp] (
    [Id] INT NOT NULL,
    [Name] NVARCHAR(4000) NULL DEFAULT(N'DEFAULT NAME'),
    [Unique1] INT NULL,
    [Unique2] INT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    CONSTRAINT [PK_Temp_] PRIMARY KEY NONCLUSTERED ([Id]),
    CONSTRAINT [UQ_Temp_] UNIQUE NONCLUSTERED ([Unique1], [Unique2] DESC),
    CONSTRAINT [CK_Temp_] CHECK ([Name] IS NOT NULL),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);
";
            Assert.AreEqual(expectedSql, sqlBuilder.ToString().RemoveGuids());
        }
    }
}
