using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevZest.Data.Primitives;
using Moq;
using DevZest.Data.Helpers;
using DevZest.Data.SqlServer;
using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Annotations;

namespace DevZest.Data
{
    [TestClass]
    public class ModelTests
    {
        private class SimpleModel : Model
        {
            public static readonly Mounter<_Int32> _Column1 = RegisterColumn((SimpleModel x) => x.Column1);

            public static readonly Mounter<_Int32> _Column2 = RegisterColumn((SimpleModel x) => x.Column2);

            [Required]
            [DbColumn("DbColumnName")]
            public _Int32 Column1 { get; private set; }

            [Required]
            [DbColumn("DbColumnName")]
            public _Int32 Column2 { get; private set; }
        }

        private class RefSimpleModel : Model
        {
            public static readonly Mounter<_Int32> _Column1 = RegisterColumn((RefSimpleModel x) => x.Col1, SimpleModel._Column1);

            public static readonly Mounter<_Int32> _Column2 = RegisterColumn((RefSimpleModel x) => x.Col2, SimpleModel._Column2);

            public _Int32 Col1 { get; private set; }

            public _Int32 Col2 { get; private set; }
        }

        private class RefSimpleModel2 : Model
        {
            public static readonly Mounter<_Int32> _Column1 = RegisterColumn((RefSimpleModel2 x) => x.Col1, RefSimpleModel._Column1);

            public static readonly Mounter<_Int32> _Column2 = RegisterColumn((RefSimpleModel2 x) => x.Col2, RefSimpleModel._Column2);

            public _Int32 Col1 { get; private set; }

            public _Int32 Col2 { get; private set; }
        }

        private class ColumnListModel : Model
        {
            public static readonly Mounter<ColumnList<_Int32>> _Cols = RegisterColumnList((ColumnListModel x) => x.Cols);

            public ColumnListModel()
            {
                Cols.Add<_Int32>();
                Cols.Add(SimpleModel._Column1, true);
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
        public void Model_RegisterColumn_reference_to_existing_property()
        {
            var model = new RefSimpleModel();
            model.Col1.Verify(model, typeof(RefSimpleModel), "Col1", typeof(SimpleModel), "Column1");
            model.Col2.Verify(model, typeof(RefSimpleModel), "Col2", typeof(SimpleModel), "Column2");
        }

        [TestMethod]
        public void Model_RegisterColumn_reference_to_existing_property_two_levels()
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
            public static readonly Mounter<RecursiveModel> _ChildModel = RegisterChildModel((RecursiveModel x) => x.ChildModel, x => x.ParentKey);

            public RecursiveModel ChildModel { get; private set; }
        }

        [TestMethod]
        public void Model_RegisterChildModel()
        {
            var model = new RecursiveModel();
            
            Assert.IsFalse(model.IsInitialized);
            Assert.IsNull(model.ChildModel);

            model.EnsureInitialized(false);
            Assert.IsTrue(model.IsInitialized);

            var childModel = model.ChildModel;

            // assert ParentModelColumnMappings
            Assert.AreEqual(model, childModel.GetParent());
            Assert.AreEqual(1, childModel.ParentRelationship.Count);
            Assert.AreEqual(childModel.ParentId, ((DbColumnExpression)childModel.ParentRelationship[0].SourceExpression).Column);
            Assert.AreEqual(model.Id, childModel.ParentRelationship[0].Target);

            // assert ChildColumns
            model.ChildModels.Verify(childModel);
        }

        private sealed class LocalModel : Model
        {
            static LocalModel()
            {
                RegisterChildModel((LocalModel x) => x.Child);
            }

            public Column<int> Column { get; private set; }

            public ChildLocalModel Child { get; private set; }

            protected override void OnChildDataSetsCreated()
            {
                Column = DataSetContainer.CreateLocalColumn<int>(this);
                Child.Initialize(this);
                base.OnChildDataSetsCreated();
            }
        }
        
        private sealed class ChildLocalModel : Model
        {
            public Column<int> Column1 { get; private set; }
            public Column<int> Column2 { get; private set; }

            internal void Initialize(LocalModel localModel)
            {
                Column1 = DataSetContainer.CreateLocalColumn(this, localModel.Column, GetColumn1Value);
                Column2 = DataSetContainer.CreateLocalColumn(this, Column1, GetColumn2Value);
            }

            private static int GetColumn1Value(DataRow dataRow, Column<int> column)
            {
                return column[dataRow];
            }

            private static int GetColumn2Value(DataRow dataRow, Column<int> column1)
            {
                return column1[dataRow] * 2;
            }
        }

        [TestMethod]
        public void Model_RegisterChildMolde_local()
        {
            var dataSet = DataSet<LocalModel>.New();
            var _ = dataSet._;
            dataSet.AddRow();
            _.Column[0] = 3;
            var children = dataSet[0].Children(_.Child);
            children.AddRow();
            children.AddRow();
            Assert.AreEqual(3, _.Child.Column1[children[0]]);
            Assert.AreEqual(6, _.Child.Column2[children[1]]);
        }

        #endregion

        #region Clone

        private class CloneModel : SimpleModelBase
        {
            public static Mounter<ColumnList<_Int32>> _ColumnList = RegisterColumnList((CloneModel x) => x.ColumnList);

            public static readonly Mounter<CloneModel> _ChildModel = RegisterChildModel((CloneModel x) => x.ChildModel, x => x.ParentKey);

            public ColumnList<_Int32> ColumnList { get; private set; }

            public CloneModel ChildModel { get; set; }
        }

        [TestMethod]
        public void Model_Clone()
        {
            var model1 = new CloneModel();
            var mock = new Mock<DataSource>();
            mock.Setup(x => x.Model).Returns(model1);
            DataSource ds1 = mock.Object;
            Init(model1, ds1);

            var clone1 = Model.Clone<CloneModel>(model1, true);
            Verify(clone1);
            Verify(model1, clone1);
        }

        private static void Init(CloneModel model, DataSource ds)
        {
            model.ColumnList.Add<_Int32>();
            model.ColumnList.Add<_Int32>();
            model.EnsureInitialized(false);
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
            public sealed class Key : PrimaryKey
            {
                public Key(_Int32 id)
                {
                    Id = id;
                }

                public _Int32 Id { get; private set; }
            }

            public static readonly Mounter<_Int32> _Id = RegisterColumn((TempModel x) => x.Id);
            public static readonly Mounter<_String> _Name = RegisterColumn((TempModel x) => x.Name);
            public static readonly Mounter<_Int32> _Unique1 = RegisterColumn((TempModel x) => x.Unique1);
            public static readonly Mounter<_Int32> _Unique2 = RegisterColumn((TempModel x) => x.Unique2);

            public TempModel()
            {
                _primaryKey = new Key(Id);
                FkRef = new Key(Unique1);
                Name.SetDefaultValue("DEFAULT NAME", null, null);
                DbUnique("UQ_Temp", null, false, Unique1, Unique2.Desc());
                DbCheck("CK_Temp", null, Name.IsNotNull());
                this.AddDbTableConstraint(DbSession.DbForeignKey(null, null, FkRef, this, ForeignKeyAction.NoAction, ForeignKeyAction.NoAction), false);
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

            temp.GenerateCreateTableSql(sqlBuilder, SqlVersion.Sql11, tableName, null, false);
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

            temp.GenerateCreateTableSql(sqlBuilder, SqlVersion.Sql11, "#Temp", null, true);
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

        #endregion

        [TestMethod]
        public void Model_Columns_DbColumnName_suffix_assigned()
        {
            var model = new SimpleModel();
            model.Columns.InitDbColumnNames();
            Assert.AreEqual("DbColumnName", model.Columns[0].DbColumnName);
            Assert.AreEqual("DbColumnName1", model.Columns[1].DbColumnName);
        }

        [TestMethod]
        public void Model_Columns_indexer_by_name()
        {
            var model = new SimpleModel();
            model.Columns.InitDbColumnNames();
            Assert.AreSame(model.Column1, model.Columns["Column1"]);
            Assert.AreSame(model.Column2, model.Columns["Column2"]);
            Assert.IsNull(model.Columns["DbColumnName"]);
            Assert.IsNull(model.Columns["DbColumnName1"]);
        }

        [TestMethod]
        public void Model_ValidationRules_correctly_initialized()
        {
            var model = new SimpleModel();
            Assert.AreEqual(2, model.Validators.Count);
        }

        [TestMethod]
        public void Model_DataRowInserted()
        {
            var salesOrders = DataSet<SalesOrder>.New();
            var _ = salesOrders._;
            var dataRowInsertedCount = 0;
            _.AfterDataRowInserted += delegate { dataRowInsertedCount++; };
            var dataRowUpdatedCount = 0;
            _.ValueChanged += delegate { dataRowUpdatedCount++; };
            salesOrders.Add(new DataRow(), x =>
            {
                _.SalesOrderID[x] = 12345;
            });

            Assert.AreEqual("SO12345", _.SalesOrderNumber[0]);
            Assert.AreEqual(1, dataRowInsertedCount);
            Assert.AreEqual(0, dataRowUpdatedCount);
        }

        [TestMethod]
        public void Model_ValueChanged()
        {
            var salesOrders = DataSet<SalesOrder>.New();
            var _ = salesOrders._;
            var dataRowUpdatedCount = 0;
            var changedColumns = Columns.Empty;
            _.ValueChanged += (sender, e) =>
            {
                dataRowUpdatedCount++;
                foreach (var column in e.Columns)
                    changedColumns = changedColumns.Add(column);
            };
            salesOrders.Add(new DataRow());
            _.SalesOrderID[0] = 12345;

            Assert.AreEqual("SO12345", _.SalesOrderNumber[0]);
            Assert.AreEqual(2, dataRowUpdatedCount);
            Assert.IsTrue(changedColumns.SetEquals(Columns.Empty.Add(_.SalesOrderID).Add(_.SalesOrderNumber)));
        }

        [TestMethod]
        public void Model_DataRowRemoved()
        {
            var salesOrders = DataSet<SalesOrder>.New();
            var _ = salesOrders._;
            var dataRowRemovedCount = 0;
            _.DataRowRemoved += delegate { dataRowRemovedCount++; };
            salesOrders.Add(new DataRow());

            salesOrders.Remove(salesOrders[0]);
            Assert.AreEqual(1, dataRowRemovedCount);
        }
    }
}
