using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevZest.Data.Primitives;
using Moq;
using DevZest.Data.Helpers;
using DevZest.Data.SqlServer;

namespace DevZest.Data
{
    [TestClass]
    public class ModelTests
    {
        private class SimpleModel : Model
        {
            public static readonly Accessor<SimpleModel, _Int32> Column1Accessor = RegisterColumn((SimpleModel x) => x.Column1, x => x.DbColumnName = "DbColumnName");

            public static readonly Accessor<SimpleModel, _Int32> Column2Accessor = RegisterColumn((SimpleModel x) => x.Column2, x => x.DbColumnName = "DbColumnName");

            [Required]
            public _Int32 Column1 { get; private set; }

            [Required]
            public _Int32 Column2 { get; private set; }
        }

        private class RefSimpleModel : Model
        {
            public static readonly Accessor<RefSimpleModel, _Int32> Column1Accessor = RegisterColumn(
                (RefSimpleModel x) => x.Col1, SimpleModel.Column1Accessor);

            public static readonly Accessor<RefSimpleModel, _Int32> Column2Accessor = RegisterColumn(
                (RefSimpleModel x) => x.Col2, SimpleModel.Column2Accessor);

            public _Int32 Col1 { get; private set; }

            public _Int32 Col2 { get; private set; }
        }

        private class RefSimpleModel2 : Model
        {
            public static readonly Accessor<RefSimpleModel2, _Int32> Column1Accessor = RegisterColumn(
                (RefSimpleModel2 x) => x.Col1, RefSimpleModel.Column1Accessor);

            public static readonly Accessor<RefSimpleModel2, _Int32> Column2Accessor = RegisterColumn(
                (RefSimpleModel2 x) => x.Col2, RefSimpleModel.Column2Accessor);

            public _Int32 Col1 { get; private set; }

            public _Int32 Col2 { get; private set; }
        }

        private class ColumnListModel : Model
        {
            public static readonly Accessor<ColumnListModel, ColumnList<_Int32>> ColumnsAccessor = RegisterColumnList((ColumnListModel x) => x.Cols);

            public ColumnListModel()
            {
                Cols.Add<_Int32>();
                Cols.Add(SimpleModel.Column1Accessor, true);
            }

            public ColumnList<_Int32> Cols { get; private set; }
        }

        [TestMethod]
        public void Model_RegisterColumn()
        {
            var model = new SimpleModel();
            model.Column1.Verify(model, typeof(SimpleModel), "Column1");
            model.Column2.Verify(model, typeof(SimpleModel), "Column2");
        }

        [TestMethod]
        public void Model_RegisterColumn_reference_to_existing_accessor()
        {
            var model = new RefSimpleModel();
            model.Col1.Verify(model, typeof(RefSimpleModel), "Col1", typeof(SimpleModel), "Column1");
            model.Col2.Verify(model, typeof(RefSimpleModel), "Col2", typeof(SimpleModel), "Column2");
        }

        [TestMethod]
        public void Model_RegisterColumn_reference_to_existing_accessor_two_levels()
        {
            var model = new RefSimpleModel2();
            model.Col1.Verify(model, typeof(RefSimpleModel2), "Col1", typeof(SimpleModel), "Column1");
            model.Col2.Verify(model, typeof(RefSimpleModel2), "Col2", typeof(SimpleModel), "Column2");
        }

        [TestMethod]
        public void Model_RegisterColumnList()
        {
            var model = new ColumnListModel();
            Assert.AreEqual(2, model.Cols.Count);
            model.Cols.Verify(model, typeof(ColumnListModel), "Cols");
            model.Cols[0].Verify(model, typeof(ColumnListModel), "Cols_0");
            model.Cols[1].Verify(model, typeof(ColumnListModel), "Cols_1", typeof(SimpleModel), "Column1");
        }

        #region RegisterChildModel

        private class RecursiveModel : SimpleModelBase
        {
            public static readonly Accessor<RecursiveModel, RecursiveModel> ChildModelAccessor = RegisterChildModel((RecursiveModel x) => x.ChildModel,
                x => x.ParentKey);

            public RecursiveModel ChildModel { get; private set; }
        }

        [TestMethod]
        public void Model_RegisterChildModel()
        {
            var model = new RecursiveModel();
            
            Assert.IsFalse(model.AreChildModelsInitialized);
            Assert.IsNull(model.ChildModel);

            model.EnsureChildModelsInitialized();
            Assert.IsTrue(model.AreChildModelsInitialized);

            var childModel = model.ChildModel;

            // assert ParentModelColumnMappings
            Assert.AreEqual(model, childModel.GetParentModel());
            Assert.AreEqual(1, childModel.ParentRelationship.Count);
            Assert.AreEqual(childModel.ParentId, ((DbColumnExpression)childModel.ParentRelationship[0].Source).Column);
            Assert.AreEqual(model.Id, childModel.ParentRelationship[0].TargetColumn);

            // assert ChildColumns
            model.ChildModels.Verify(childModel);
        }

        #endregion

        #region Clone

        private class CloneModel : SimpleModelBase
        {
            public static Accessor<CloneModel, ColumnList<_Int32>> ColumnListAccessor = RegisterColumnList((CloneModel x) => x.ColumnList);

            public static readonly Accessor<CloneModel, CloneModel> ChildModelAccessor = RegisterChildModel((CloneModel x) => x.ChildModel,
                x => x.ParentKey);

            public ColumnList<_Int32> ColumnList { get; private set; }

            public CloneModel ChildModel { get; set; }
        }

        [TestMethod]
        public void Model_Clone()
        {
            var model1 = new CloneModel();
            DataSource ds1 = new Mock<DataSource>(model1).Object;
            Init(model1, ds1);

            var clone1 = Model.Clone<CloneModel>(model1, true);
            Verify(clone1);
            Verify(model1, clone1);
        }

        private static void Init(CloneModel model, DataSource ds)
        {
            model.ColumnList.Add<_Int32>();
            model.ColumnList.Add<_Int32>();
            model.EnsureChildModelsInitialized();
            model.SetDataSource(ds);
        }

        private static void Verify(CloneModel x)
        {
            Assert.AreEqual(x, x.PrimaryKey.ParentModel);
            Assert.AreEqual(x, x.ParentKey.ParentModel);

            foreach (var column in x.Columns)
                Assert.AreEqual(x, column.ParentModel);
        }

        private static void Verify(CloneModel x, CloneModel y)
        {
            Assert.AreEqual(x.Columns.Count, y.Columns.Count);
            Assert.AreEqual(x.DataSource, y.DataSource);
        }

        #endregion

        #region Constraints

        private sealed class TempModel : Model<TempModel.Key>
        {
            public sealed class Key : ModelKey
            {
                public Key(_Int32 id)
                {
                    Id = id;
                }

                public _Int32 Id { get; private set; }
            }

            public static readonly Accessor<TempModel, _Int32> IdAccessor = RegisterColumn((TempModel x) => x.Id);
            public static readonly Accessor<TempModel, _String> NameAccessor = RegisterColumn((TempModel x) => x.Name);
            public static readonly Accessor<TempModel, _Int32> Unique1Accessor = RegisterColumn((TempModel x) => x.Unique1);
            public static readonly Accessor<TempModel, _Int32> Unique2Accessor = RegisterColumn((TempModel x) => x.Unique2);

            public TempModel()
            {
                _primaryKey = new Key(Id);
                FkRef = new Key(Unique1);
                Name.DefaultValue("DEFAULT NAME");
                Unique("UQ_Temp", false, Unique1, Unique2.Desc());
                Check("CK_Temp", Name.IsNotNull());
                DbSession.ForeignKey(null, FkRef, this, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction);
            }

            private Key _primaryKey;
            public sealed override Key PrimaryKey
            {
                get { return _primaryKey; }
            }

            public _Int32 Id { get; private set; }

            public new _String Name { get; private set; }

            public _Int32 Unique1 { get; private set; }

            public _Int32 Unique2 { get; private set; }

            public Key FkRef { get; private set; }

        }

        [TestMethod]
        public void Model_GenerateCreateTableSql_permanent_table()
        {
            var temp = new TempModel();
            var tableName = "Test";
            DbTable<TempModel>.Create(temp, new Mock<DbSession>().Object, tableName);
            var sqlBuilder = new IndentedStringBuilder();

            temp.GenerateCreateTableSql(sqlBuilder, SqlVersion.Sql11, tableName, false);
            var expectedSql =
@"CREATE TABLE [Test] (
    [Id] INT NOT NULL,
    [Name] NVARCHAR(4000) NULL DEFAULT(N'DEFAULT NAME'),
    [Unique1] INT NULL,
    [Unique2] INT NULL

    PRIMARY KEY CLUSTERED ([Id]),
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

            temp.GenerateCreateTableSql(sqlBuilder, SqlVersion.Sql11, "#Temp", true);
            var expectedSql =
@"CREATE TABLE [#Temp] (
    [Id] INT NOT NULL,
    [Name] NVARCHAR(4000) NULL DEFAULT(N'DEFAULT NAME'),
    [Unique1] INT NULL,
    [Unique2] INT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([Id]),
    UNIQUE NONCLUSTERED ([Unique1], [Unique2] DESC),
    CHECK ([Name] IS NOT NULL),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);
";
            Assert.AreEqual(expectedSql, sqlBuilder.ToString());
        }

        #endregion

        [TestMethod]
        public void Model_Columns_DbColumnName_suffix_assigned()
        {
            var model = new SimpleModel();
            model.Columns.Seal();
            Assert.AreEqual("DbColumnName", model.Columns[0].DbColumnName);
            Assert.AreEqual("DbColumnName1", model.Columns[1].DbColumnName);
        }

        [TestMethod]
        public void Model_Columns_indexer_by_name()
        {
            var model = new SimpleModel();
            model.Columns.Seal();
            Assert.AreSame(model.Column1, model.Columns["Column1"]);
            Assert.AreSame(model.Column2, model.Columns["Column2"]);
            Assert.IsNull(model.Columns["DbColumnName"]);
            Assert.IsNull(model.Columns["DbColumnName1"]);
        }

        [TestMethod]
        public void Model_ValidationRules_correctly_initialized()
        {
            var model = new SimpleModel();
            Assert.AreEqual(2, model.ValidationRules.Count);
        }
    }
}
