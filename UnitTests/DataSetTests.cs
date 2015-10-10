using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DevZest.Data
{
    [TestClass]
    public class DataSetTests
    {
        private class SimpleModel : SimpleModelBase
        {
            public static readonly Accessor<SimpleModel, SimpleModel> ChildAccessor = RegisterChildModel((SimpleModel x) => x.Child,
                x => x.ParentKey);

            public SimpleModel Child { get; private set; }
        }

        [TestMethod]
        public void DataSet_new_row()
        {
            int count = 3;
            var dataSet = GetDataSet(count);
            var model = dataSet._;
            var childModel = model.Child;
            var grandChildModel = childModel.Child;

            VerifyDataSet(dataSet, count);
            for (int i = 0; i < dataSet.Count; i++)
            {
                var children = dataSet[i].Children(childModel);
                VerifyDataSet(children, count);
                for (int j = 0; j < children.Count; j++)
                {
                    var grandChildren = children[j].Children(grandChildModel);
                    VerifyDataSet(grandChildren, count);
                }
            }

            Assert.AreEqual(count, model.DataSet.Count);
            Assert.AreEqual(count * count, childModel.DataSet.Count);
            Assert.AreEqual(count * count * count, grandChildModel.DataSet.Count);
        }

        private void VerifyDataSet(DataSet<SimpleModel> dataSet, int count)
        {
            Assert.AreEqual(count == 0, dataSet._.DesignMode);
            Assert.AreEqual(count, dataSet.Count);
            var model = dataSet._;
            for (int i = 0; i < count; i++)
                Assert.AreEqual(i, model.Id[dataSet[i]]);
        }

        private DataSet<SimpleModel> GetDataSet(int count)
        {
            return SimpleModelBase.GetDataSet<SimpleModel>(count, x => x.Child, AddRows);
        }

        private void AddRows(DataSet<SimpleModel> dataSet, int count)
        {
            var model = dataSet._;
            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet.AddRow();
                model.Id[dataRow] = i;
            }
        }

        [TestMethod]
        public void DataSet_clear_all_rows()
        {
            int count = 3;
            var dataSet = GetDataSet(count);

            var model = dataSet._;
            var childModel = model.Child;
            var grandChildModel = childModel.Child;
            var modelRows = ToList(model.DataSet);
            Assert.AreEqual(count, modelRows.Count);
            var childModelRows = ToList(childModel.DataSet);
            Assert.AreEqual(count * count, childModelRows.Count);
            var grandChildModelRows = ToList(grandChildModel.DataSet);
            Assert.AreEqual(count * count * count, grandChildModelRows.Count);

            dataSet.Clear();

            Assert.AreEqual(0, model.DataSet.Count);
            Assert.AreEqual(0, childModel.DataSet.Count);
            Assert.AreEqual(0, grandChildModel.DataSet.Count);

            VerifyDisposed(modelRows);
            VerifyDisposed(childModelRows);
            VerifyDisposed(grandChildModelRows);
        }

        [TestMethod]
        public void DataSet_remove_row()
        {
            int count = 3;
            var dataSet = GetDataSet(count);
            var model = dataSet._;

            var modelRows = new List<DataRow> { dataSet[0] };
            var childModelRows = new List<DataRow>();
            var grandChildModelRows = new List<DataRow>();
            {
                var children = dataSet[0].Children(model.Child);
                foreach (var child in children)
                {
                    childModelRows.Add(child);

                    var grandChildren = child.Children(model.Child.Child);
                    foreach (var grandChild in grandChildren)
                    {
                        Assert.AreEqual(child, grandChild.Parent);
                        grandChildModelRows.Add(grandChild);
                    }
                }
            }
            Assert.AreEqual(1, modelRows.Count);
            Assert.AreEqual(count, childModelRows.Count);
            Assert.AreEqual(count * count, grandChildModelRows.Count);

            dataSet.Remove(dataSet[0]);

            VerifyDisposed(modelRows);
            VerifyDisposed(childModelRows);
            VerifyDisposed(grandChildModelRows);

            Assert.AreEqual(count - 1, model.DataSet.Count);
            for (int i = 0; i < dataSet.Count; i++)
                Assert.AreEqual(i + 1, model.Id[dataSet[i]]);
            Assert.AreEqual((count - 1) * count, model.Child.DataSet.Count);
            Assert.AreEqual((count - 1) * count * count, model.Child.Child.DataSet.Count);
        }

        private static List<DataRow> ToList(IDataSet dataSet)
        {
            var result = new List<DataRow>();
            for (int i = 0; i < dataSet.Count; i++)
                result.Add(dataSet[i]);

            return result;
        }

        private static void VerifyDisposed(List<DataRow> rows)
        {
            foreach (var row in rows)
            {
                Assert.AreEqual(null, row.Model);
                Assert.AreEqual(-1, row.Ordinal);
                Assert.AreEqual(null, row.Parent);
            }
        }

        [TestMethod]
        public void DataSet_serialize_deserialize_json()
        {
            var result = DataSet<ProductCategory>.ParseJson(Strings.ProductCategoriesJson);

            var childModel = result._.SubCategories;
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(3, result[0].Children(childModel).Count);
            Assert.AreEqual(14, result[1].Children(childModel).Count);
            Assert.AreEqual(8, result[2].Children(childModel).Count);
            Assert.AreEqual(12, result[3].Children(childModel).Count);

            Assert.AreEqual(Strings.ProductCategoriesJson.Trim(), result.ToString().Trim());
        }
    }
}
