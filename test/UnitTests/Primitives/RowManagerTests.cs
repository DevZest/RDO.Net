using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using DevZest.Samples.AdventureWorksLT;
using System.Globalization;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class RowManagerTests
    {
        private static DataSet<ProductCategory> MockProductCategories(int count)
        {
            var dataSet = DataSet<ProductCategory>.New();
            var model = dataSet._;

            string namePrefix = "Name";
            AddRows(dataSet, namePrefix, count);
            for (int i = 0; i < dataSet.Count; i++)
            {
                var children = dataSet[i].Children(model.SubCategories);
                AddRows(children, GetName(namePrefix, i), count);
                for (int j = 0; j < children.Count; j++)
                {
                    var grandChildren = children[j].Children(children._.SubCategories);
                    AddRows(grandChildren, GetName(GetName(namePrefix, i), j), count);
                }
            }

            return dataSet;
        }

        private static void AddRows(DataSet<ProductCategory> dataSet, string namePrefix, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet.AddRow();
                dataSet._.Name[dataRow] = GetName(namePrefix, i);
            }
        }

        private static string GetName(string namePrefix, int index)
        {
            return string.Format("{0}-{1}", namePrefix, (index + 1).ToString(CultureInfo.InvariantCulture));
        }

        private sealed class ConcreteRowManager : RowManager
        {
            public ConcreteRowManager(DataSet dataSet)
                : base(dataSet)
            {
            }

            internal override void InvalidateView()
            {
            }
        }

        private static RowManager CreateRowManager(DataSet<Adhoc> dataSet, EofRowMapping eofRowMapping)
        {
            RowManager result = new ConcreteRowManager(dataSet);
            result.Template.EofRowMapping = eofRowMapping;
            result.Initialize();
            return result;
        }

        [TestMethod]
        public void RowManager_EofRowMapping_Never()
        {
            var dataSet = DataSet<Adhoc>.New();
            var rowManager = CreateRowManager(dataSet, EofRowMapping.Never);

            Assert.AreEqual(0, rowManager.Rows.Count);

            dataSet.AddRow();
            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.IsFalse(rowManager.Rows[0].IsEof);
        }

        [TestMethod]
        public void RowManager_EofRowMapping_Always()
        {
            var dataSet = DataSet<Adhoc>.New();
            var rowManager = CreateRowManager(dataSet, EofRowMapping.Always);

            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.AreEqual(true, rowManager.Rows[0].IsEof);

            dataSet.AddRow();
            Assert.AreEqual(2, rowManager.Rows.Count);
            Assert.IsFalse(rowManager.Rows[0].IsEof);
            Assert.IsTrue(rowManager.Rows[1].IsEof);
        }

        [TestMethod]
        public void RowManager_EofRowMapping_NoData()
        {
            var dataSet = DataSet<Adhoc>.New();
            var rowManager = CreateRowManager(dataSet, EofRowMapping.NoData);

            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.IsTrue(rowManager.Rows[0].IsEof);

            dataSet.AddRow();
            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.IsFalse(rowManager.Rows[0].IsEof);
        }

        [TestMethod]
        public void RowManager_GetHierarchicalDataSet()
        {
            var productCategories = MockProductCategories(3);
            Assert.AreEqual("", productCategories.ToJsonString(true));
        }
    }
}
