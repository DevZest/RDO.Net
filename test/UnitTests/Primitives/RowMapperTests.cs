using DevZest.Data;
using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Windows.Primitives
{
    [TestClass]
    public class RowMapperTests
    {
        private sealed class ConcreteRowMapper : RowMapper
        {
            public ConcreteRowMapper(Template template, DataSet dataSet, DataRowFilter where, Func<Model, ColumnSort[]> orderBy)
                : base(template, dataSet, where, orderBy)
            {
            }

            public ConcreteRowMapper SetupOnRowAdded(Action<RowPresenter, int> onRowAdded)
            {
                _onRowAdded = onRowAdded;
                return this;
            }

            Action<RowPresenter, int> _onRowAdded;
            protected override void OnRowAdded(RowPresenter row, int index)
            {
                if (_onRowAdded != null)
                    _onRowAdded(row, index);
            }

            public ConcreteRowMapper SetupOnRowRemoved(Action<RowPresenter, int> onRowRemoved)
            {
                _onRowRemoved = onRowRemoved;
                return this;
            }

            public ConcreteRowMapper SetupOnRowMoved(Action<RowPresenter, int, int> onRowMoved)
            {
                _onRowMoved = onRowMoved;
                return this;
            }

            Action<RowPresenter, int> _onRowRemoved;
            protected override void OnRowRemoved(RowPresenter parent, int index)
            {
                if (_onRowRemoved != null)
                    _onRowRemoved(parent, index);
            }

            Action<RowPresenter, int, int> _onRowMoved;
            protected override void OnRowMoved(RowPresenter row, int oldIndex, int newIndex)
            {
                if (_onRowMoved != null)
                    _onRowMoved(row, oldIndex, newIndex);
            }

            public ConcreteRowMapper SetupOnRowUpdated(Action<RowPresenter> onRowUpdated)
            {
                _onRowUpdated = onRowUpdated;
                return this;
            }

            Action<RowPresenter> _onRowUpdated;
            protected override void OnRowUpdated(RowPresenter row)
            {
                if (_onRowUpdated != null)
                    _onRowUpdated(row);
            }
        }

        private static ConcreteRowMapper CreateRowMapper<T>(DataSet<T> dataSet, DataRowFilter where = null, Func<T, ColumnSort[]> orderBy = null)
            where T : Model, new()
        {
            var template = new Template();
            return new ConcreteRowMapper(template, dataSet, where, DataPresenter<T>.Wrap(orderBy));
        }

        private static ConcreteRowMapper CreateRecursiveRowMapper<T>(DataSet<T> dataSet, int hierarchicalModelOrdinal = 0, DataRowFilter where = null,
            Func<T, ColumnSort[]> orderBy = null)
            where T : Model, new()
        {
            var template = new Template();
            template.RecursiveModelOrdinal = hierarchicalModelOrdinal;
            return new ConcreteRowMapper(template, dataSet, where, DataPresenter<T>.Wrap(orderBy));
        }

        private void Verify(RowPresenter row, DataRow dataRow, DataSet childDataSet = null, params int[] childIndexes)
        {
            Assert.AreEqual(dataRow, row.DataRow);
            if (dataRow.ParentDataRow == null)
                Assert.AreEqual(null, row.Parent);
            else
                Assert.AreEqual(dataRow.ParentDataRow, row.Parent.DataRow);

            if (childIndexes == null)
                childIndexes = Array<int>.Empty;

            Assert.AreEqual(childIndexes.Length, row.Children.Count);
            for (int i = 0; i < row.Children.Count; i++)
            {
                Assert.AreEqual(row, row.Children[i].Parent);
                Assert.AreEqual(childDataSet[childIndexes[i]], row.Children[i].DataRow);
            }
        }

        [TestMethod]
        public void RowMapper_Initialize_simple()
        {
            var dataSet = DataSetMock.ProductCategories(3);
            var rowMapper = CreateRowMapper(dataSet);
            var rows = rowMapper.Rows;
            Assert.AreEqual(3, rows.Count);
            Verify(rows[0], dataSet[0]);
            Verify(rows[1], dataSet[1]);
            Verify(rows[2], dataSet[2]);
        }

        [TestMethod]
        public void RowMapper_OnDataRowAdded_simple()
        {
            var dataSet = DataSetMock.ProductCategories(3);
            RowPresenter addedRow = null;
            int addedRowIndex = -1;
            var rowMapper = CreateRowMapper(dataSet).SetupOnRowAdded((row, index) =>
            {
                addedRow = row;
                addedRowIndex = index;
            });
            dataSet.AddRow();
            var rows = rowMapper.Rows;
            Assert.AreEqual(4, rows.Count);
            Verify(rows[0], dataSet[0]);
            Verify(rows[1], dataSet[1]);
            Verify(rows[2], dataSet[2]);
            Verify(rows[3], dataSet[3]);

            Assert.AreEqual(rows[3], addedRow);
            Assert.AreEqual(3, addedRowIndex);
        }

        [TestMethod]
        public void RowMapper_OnDataRowRemoved_simple()
        {
            var dataSet = DataSetMock.ProductCategories(3);
            RowPresenter removedRowParent = null;
            int removedRowIndex = -1;
            var rowMapper = CreateRowMapper(dataSet).SetupOnRowRemoved((parent, index) =>
            {
                removedRowParent = parent;
                removedRowIndex = index;
            });

            dataSet.RemoveAt(0);

            var rows = rowMapper.Rows;
            Assert.AreEqual(2, rows.Count);
            Verify(rows[0], dataSet[0]);
            Verify(rows[1], dataSet[1]);

            Assert.AreEqual(null, removedRowParent);
            Assert.AreEqual(0, removedRowIndex);
        }

        [TestMethod]
        public void RowMapper_OnDataRowUpdated_simple()
        {
            var dataSet = DataSetMock.ProductCategories(3);
            RowPresenter rowUpdated = null;
            bool rowMoved = false;
            var rowMapper = CreateRowMapper(dataSet)
                .SetupOnRowUpdated(row => rowUpdated = row)
                .SetupOnRowMoved((parent, oldIndex, newIndex) =>
                {
                    rowMoved = true;
                });
            dataSet._.Name[0] = "Updated Name";

            var rows = rowMapper.Rows;
            Assert.AreEqual(3, rows.Count);
            Assert.AreEqual(rows[0], rowUpdated);
            Assert.AreEqual(false, rowMoved);
        }

        [TestMethod]
        public void RowMapper_Initialize_recursive()
        {
            var dataSet = DataSetMock.ProductCategories(3);
            var rowMapper = CreateRecursiveRowMapper(dataSet);
            var rows = rowMapper.Rows;
            Assert.AreEqual(3, rows.Count);
            Verify(rows[0], dataSet[0], dataSet.SubCategories(0), 0, 1, 2);
            Verify(rows[0].Children[0], dataSet.SubCategories(0)[0], dataSet.SubCategories(0).SubCategories(0), 0, 1, 2);
            Verify(rows[0].Children[0].Children[0], dataSet.SubCategories(0).SubCategories(0)[0]);
            Verify(rows[0].Children[0].Children[1], dataSet.SubCategories(0).SubCategories(0)[1]);
            Verify(rows[0].Children[0].Children[2], dataSet.SubCategories(0).SubCategories(0)[2]);
            Verify(rows[0].Children[1], dataSet.SubCategories(0)[1], dataSet.SubCategories(0).SubCategories(1), 0, 1, 2);
            Verify(rows[0].Children[1].Children[0], dataSet.SubCategories(0).SubCategories(1)[0]);
            Verify(rows[0].Children[1].Children[1], dataSet.SubCategories(0).SubCategories(1)[1]);
            Verify(rows[0].Children[1].Children[2], dataSet.SubCategories(0).SubCategories(1)[2]);
            Verify(rows[0].Children[2], dataSet.SubCategories(0)[2], dataSet.SubCategories(0).SubCategories(2), 0, 1, 2);
            Verify(rows[0].Children[2].Children[0], dataSet.SubCategories(0).SubCategories(2)[0]);
            Verify(rows[0].Children[2].Children[1], dataSet.SubCategories(0).SubCategories(2)[1]);
            Verify(rows[0].Children[2].Children[2], dataSet.SubCategories(0).SubCategories(2)[2]);

            Verify(rows[1], dataSet[1], dataSet.SubCategories(1), 0, 1, 2);
            Verify(rows[1].Children[0], dataSet.SubCategories(1)[0], dataSet.SubCategories(1).SubCategories(0), 0, 1, 2);
            Verify(rows[1].Children[0].Children[0], dataSet.SubCategories(1).SubCategories(0)[0]);
            Verify(rows[1].Children[0].Children[1], dataSet.SubCategories(1).SubCategories(0)[1]);
            Verify(rows[1].Children[0].Children[2], dataSet.SubCategories(1).SubCategories(0)[2]);
            Verify(rows[1].Children[1], dataSet.SubCategories(1)[1], dataSet.SubCategories(1).SubCategories(1), 0, 1, 2);
            Verify(rows[1].Children[1].Children[0], dataSet.SubCategories(1).SubCategories(1)[0]);
            Verify(rows[1].Children[1].Children[1], dataSet.SubCategories(1).SubCategories(1)[1]);
            Verify(rows[1].Children[1].Children[2], dataSet.SubCategories(1).SubCategories(1)[2]);
            Verify(rows[1].Children[2], dataSet.SubCategories(1)[2], dataSet.SubCategories(1).SubCategories(2), 0, 1, 2);
            Verify(rows[1].Children[2].Children[0], dataSet.SubCategories(1).SubCategories(2)[0]);
            Verify(rows[1].Children[2].Children[1], dataSet.SubCategories(1).SubCategories(2)[1]);
            Verify(rows[1].Children[2].Children[2], dataSet.SubCategories(1).SubCategories(2)[2]);

            Verify(rows[2], dataSet[2], dataSet.SubCategories(2), 0, 1, 2);
            Verify(rows[2].Children[0], dataSet.SubCategories(2)[0], dataSet.SubCategories(2).SubCategories(0), 0, 1, 2);
            Verify(rows[2].Children[0].Children[0], dataSet.SubCategories(2).SubCategories(0)[0]);
            Verify(rows[2].Children[0].Children[1], dataSet.SubCategories(2).SubCategories(0)[1]);
            Verify(rows[2].Children[0].Children[2], dataSet.SubCategories(2).SubCategories(0)[2]);
            Verify(rows[2].Children[1], dataSet.SubCategories(2)[1], dataSet.SubCategories(2).SubCategories(1), 0, 1, 2);
            Verify(rows[2].Children[1].Children[0], dataSet.SubCategories(2).SubCategories(1)[0]);
            Verify(rows[2].Children[1].Children[1], dataSet.SubCategories(2).SubCategories(1)[1]);
            Verify(rows[2].Children[1].Children[2], dataSet.SubCategories(2).SubCategories(1)[2]);
            Verify(rows[2].Children[2], dataSet.SubCategories(2)[2], dataSet.SubCategories(2).SubCategories(2), 0, 1, 2);
            Verify(rows[2].Children[2].Children[0], dataSet.SubCategories(2).SubCategories(2)[0]);
            Verify(rows[2].Children[2].Children[1], dataSet.SubCategories(2).SubCategories(2)[1]);
            Verify(rows[2].Children[2].Children[2], dataSet.SubCategories(2).SubCategories(2)[2]);
        }

        [TestMethod]
        public void RowMapper_OnRowAdded_recursive()
        {
            var dataSet = DataSetMock.ProductCategories(3);
            RowPresenter addedRow = null;
            int addedRowIndex = -1;
            var rowMapper = CreateRecursiveRowMapper(dataSet).SetupOnRowAdded((row, index) =>
            {
                addedRow = row;
                addedRowIndex = index;
            });

            // add row to the leaf level, this should trigger data changed events wiring to the next level.
            var newRow = dataSet.SubCategories(0).SubCategories(0).SubCategories(0).AddRow();

            var rows = rowMapper.Rows;
            Assert.AreEqual(1, rows[0].Children[0].Children[0].Children.Count);
            Verify(rows[0].Children[0].Children[0].Children[0], newRow);
            Assert.AreEqual(newRow, addedRow.DataRow);
            Assert.AreEqual(0, addedRowIndex);

            // add another row to the leaf level, to verify data changed events correctly wired.
            var newRow2 = dataSet.SubCategories(0).SubCategories(0).SubCategories(0).SubCategories(0).AddRow();

            Assert.AreEqual(1, rows[0].Children[0].Children[0].Children[0].Children.Count);
            Verify(rows[0].Children[0].Children[0].Children[0].Children[0], newRow2);
            Assert.AreEqual(newRow2, addedRow.DataRow);
            Assert.AreEqual(0, addedRowIndex);
        }

        [TestMethod]
        public void RowMapper_OnDataRowRemoved_recursive()
        {
            var dataSet = DataSetMock.ProductCategories(3);
            RowPresenter removedRowParent = null;
            int removedRowIndex = -1;
            var rowMapper = CreateRecursiveRowMapper(dataSet).SetupOnRowRemoved((parent, index) =>
            {
                removedRowParent = parent;
                removedRowIndex = index;
            });

            dataSet.SubCategories(1).RemoveAt(1);

            var rows = rowMapper.Rows;
            Assert.AreEqual(2, rows[1].Children.Count);
            Assert.AreEqual(rows[1], removedRowParent);
            Assert.AreEqual(1, removedRowIndex);
        }

        [TestMethod]
        public void RowMapper_Initialize_recursive_query()
        {
            var dataSet = DataSetMock.ProductCategories(3);
            var _ = dataSet._;
            var rowMapper = CreateRecursiveRowMapper(dataSet, 0, _.Where(Condition1), x => new ColumnSort[] { x.Name.Desc() });

            var rows = rowMapper.Rows;
            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual("Name-1", rows[0].GetValue(_.Name));
            Assert.AreEqual(2, rows[0].Children.Count);
            Assert.AreEqual("Name-1-2", rows[0].Children[0].GetValue(_.Name));
            Assert.AreEqual("Name-1-1", rows[0].Children[1].GetValue(_.Name));
        }

        private static bool Condition1(ProductCategory _, DataRow dataRow)
        {
            return _.Name[dataRow] == "Name-1-1" || _.Name[dataRow] == "Name-1-2";
        }

        [TestMethod]
        public void RowMapper_OnDataRowInserted_recursive_query()
        {
            var dataSet = DataSetMock.ProductCategories(3);
            var _ = dataSet._;
            var rowMapper = CreateRecursiveRowMapper(dataSet, 0, _.Where(Condition2), x => new ColumnSort[] { x.Name.Desc() });

            dataSet.SubCategories(0).AddRow((__, x) => __.Name[x] = "Name-1-4");

            var rows = rowMapper.Rows;
            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual("Name-1", rows[0].GetValue(_.Name));
            Assert.AreEqual(3, rows[0].Children.Count);
            Assert.AreEqual("Name-1-4", rows[0].Children[0].GetValue(_.Name));
            Assert.AreEqual("Name-1-2", rows[0].Children[1].GetValue(_.Name));
            Assert.AreEqual("Name-1-1", rows[0].Children[2].GetValue(_.Name));
        }

        private static bool Condition2(ProductCategory _, DataRow dataRow)
        {
            return _.Name[dataRow] == "Name-1-1" || _.Name[dataRow] == "Name-1-2" || _.Name[dataRow] == "Name-1-4";
        }

        [TestMethod]
        public void RowMapper_OnDataRowRemoved_recursive_query()
        {
            var dataSet = DataSetMock.ProductCategories(3);
            var _ = dataSet._;
            var rowMapper = CreateRecursiveRowMapper(dataSet, 0, _.Where(Condition1), x => new ColumnSort[] { x.Name.Desc() });

            dataSet.SubCategories(0).RemoveAt(1);

            var rows = rowMapper.Rows;
            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual("Name-1", rows[0].GetValue(_.Name));
            Assert.AreEqual(1, rows[0].Children.Count);
            Assert.AreEqual("Name-1-1", rows[0].Children[0].GetValue(_.Name));

            dataSet.SubCategories(0).RemoveAt(0);
            Assert.AreEqual(0, rows.Count);
        }

        [TestMethod]
        public void RowMapper_OnDataRowUpdated_recursive_query()
        {
            var dataSet = DataSetMock.ProductCategories(3);
            var _ = dataSet._;
            var rowMapper = CreateRecursiveRowMapper(dataSet, 0, _.Where(Condition2), x => new ColumnSort[] { x.Name.Desc() });

            var subCategories = dataSet.SubCategories(0);
            subCategories._.Name[subCategories[0]] = "Name-1-4";

            var rows = rowMapper.Rows;
            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual("Name-1", rows[0].GetValue(_.Name));
            Assert.AreEqual(2, rows[0].Children.Count);
            Assert.AreEqual("Name-1-4", rows[0].Children[0].GetValue(_.Name));
            Assert.AreEqual("Name-1-2", rows[0].Children[1].GetValue(_.Name));

            subCategories._.Name[subCategories[0]] = "Name-1-5";
            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual("Name-1", rows[0].GetValue(_.Name));
            Assert.AreEqual(1, rows[0].Children.Count);
            Assert.AreEqual("Name-1-2", rows[0].Children[0].GetValue(_.Name));

            subCategories._.Name[subCategories[2]] = "Name-1-4";
            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual("Name-1", rows[0].GetValue(_.Name));
            Assert.AreEqual(2, rows[0].Children.Count);
            Assert.AreEqual("Name-1-4", rows[0].Children[0].GetValue(_.Name));
            Assert.AreEqual("Name-1-2", rows[0].Children[1].GetValue(_.Name));
        }
    }
}