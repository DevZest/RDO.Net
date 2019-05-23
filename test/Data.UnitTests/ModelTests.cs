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

        private class ColumnListModel : Model
        {
            static ColumnListModel()
            {
                RegisterColumnList((ColumnListModel x) => x.Cols);
            }

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
                RegisterLocalColumn((LocalModel _) => _.Column);
                RegisterChildModel((LocalModel _) => _.Child);
            }

            public LocalColumn<int> Column { get; private set; }

            public ChildLocalModel Child { get; private set; }

            protected override void OnChildDataSetsCreated()
            {
                Child.Initialize(this);
                base.OnChildDataSetsCreated();
            }
        }
        
        private sealed class ChildLocalModel : Model
        {
            static ChildLocalModel()
            {
                RegisterLocalColumn((ChildLocalModel _) => _.Column1);
                RegisterLocalColumn((ChildLocalModel _) => _.Column2);
            }

            public LocalColumn<int> Column1 { get; private set; }
            public LocalColumn<int> Column2 { get; private set; }

            internal void Initialize(LocalModel localModel)
            {
                Column1.ComputedAs(localModel.Column, GetColumn1Value, false);
                Column2.ComputedAs(Column1, GetColumn2Value, false);
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
            var dataSet = DataSet<LocalModel>.Create();
            var _ = dataSet._;
            dataSet.AddRow();
            _.Column[0] = 3;
            var children = _.Child.GetChildDataSet(0);
            children.AddRow();
            children.AddRow();
            Assert.AreEqual(3, _.Child.Column1[children[0]]);
            Assert.AreEqual(6, _.Child.Column2[children[1]]);
        }

        #endregion

        #region Clone

        private class CloneModel : SimpleModelBase
        {
            public static readonly Mounter<CloneModel> _ChildModel = RegisterChildModel((CloneModel x) => x.ChildModel, x => x.ParentKey);

            static CloneModel()
            {
                RegisterColumnList((CloneModel x) => x.ColumnList);
            }

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

            var clone1 = model1.MakeCopy(true);
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
            var salesOrders = DataSet<SalesOrder>.Create();
            var _ = salesOrders._;
            var dataRowInsertedCount = 0;
            _.AfterDataRowInserted += delegate { dataRowInsertedCount++; };
            var dataRowUpdatedCount = 0;
            _.ValueChanged += delegate { dataRowUpdatedCount++; };
            salesOrders._.SuspendIdentity();
            salesOrders.Add(new DataRow(), x =>
            {
                _.SalesOrderID[x] = 12345;
            });
            salesOrders._.ResumeIdentity();

            Assert.AreEqual("SO12345", _.SalesOrderNumber[0]);
            Assert.AreEqual(1, dataRowInsertedCount);
            Assert.AreEqual(0, dataRowUpdatedCount);
        }

        [TestMethod]
        public void Model_ValueChanged()
        {
            var salesOrders = DataSet<SalesOrder>.Create();
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
            salesOrders._.SuspendIdentity();
            _.SalesOrderID[0] = 12345;
            salesOrders._.ResumeIdentity();

            Assert.AreEqual("SO12345", _.SalesOrderNumber[0]);
            Assert.AreEqual(2, dataRowUpdatedCount);
            Assert.IsTrue(changedColumns.SetEquals(Columns.Empty.Add(_.SalesOrderID).Add(_.SalesOrderNumber)));
        }

        [TestMethod]
        public void Model_DataRowRemoved()
        {
            var salesOrders = DataSet<SalesOrder>.Create();
            var _ = salesOrders._;
            var dataRowRemovedCount = 0;
            _.DataRowRemoved += delegate { dataRowRemovedCount++; };
            salesOrders.Add(new DataRow());

            salesOrders.Remove(salesOrders[0]);
            Assert.AreEqual(1, dataRowRemovedCount);
        }
    }
}
