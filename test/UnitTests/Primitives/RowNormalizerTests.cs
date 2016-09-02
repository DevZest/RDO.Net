using DevZest.Data.Windows.Helpers;
using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class RowNormalizerTests
    {
        private sealed class ConcreteRowNormalizer : RowNormalizer
        {
            public ConcreteRowNormalizer(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy)
                : base(template, dataSet, where, orderBy)
            {
            }

            public int RowsChangedCounter { get; set; }

            protected override void OnRowsChanged()
            {
                RowsChangedCounter++;
            }
        }

        private static RowNormalizer CreateRowNormalizer<T>(DataSet<T> dataSet, int hierarchicalModelOrdinal = 0, _Boolean where = null, ColumnSort[] orderBy = null)
            where T : Model, new()
        {
            var template = new Template();
            template.RecursiveModelOrdinal = hierarchicalModelOrdinal;
            return new ConcreteRowNormalizer(template, dataSet, where, orderBy);
        }

        private static RowNormalizer CreateSimpleRowNormalizer<T>(DataSet<T> dataSet, _Boolean where = null, ColumnSort[] orderBy = null)
            where T : Model, new()
        {
            var template = new Template();
            return new ConcreteRowNormalizer(template, dataSet, where, orderBy);
        }


        private static void VerifyDepths(IReadOnlyList<RowPresenter> rows, params int[] depths)
        {
            Assert.AreEqual(rows.Count, depths.Length);

            for (int i = 0; i < rows.Count; i++)
            {
                Assert.AreEqual(i, rows[i].RawIndex);
                Assert.AreEqual(depths[i], rows[i].Depth);
            }
        }

        [TestMethod]
        public void RowNormalizer_Expand_Collapse()
        {
            var productCategories = ProductCategoryDataSet.Mock(3);
            var rowNormalizer = CreateRowNormalizer(productCategories);
            var rows = rowNormalizer.Rows;
            VerifyDepths(rows, 0, 0, 0);

            rows[0].Expand();
            VerifyDepths(rows, 0, 1, 1, 1, 0, 0);

            rows[1].Expand();
            VerifyDepths(rows, 0, 1, 2, 2, 2, 1, 1, 0, 0);

            rows[0].Collapse();
            VerifyDepths(rows, 0, 0, 0);

            rows[0].Expand();
            VerifyDepths(rows, 0, 1, 2, 2, 2, 1, 1, 0, 0);
        }

        [TestMethod]
        public void RowNormalizer_OnRowAdded()
        {
            var dataSet = ProductCategoryDataSet.Mock(3);
            var rowNormalizer = CreateRowNormalizer(dataSet);
            var rows = rowNormalizer.Rows;

            dataSet.SubCategories(0).AddRow();
            VerifyDepths(rows, 0, 0, 0);

            rows[0].Expand();
            VerifyDepths(rows, 0, 1, 1, 1, 1, 0, 0);
            dataSet.SubCategories(0).AddRow();
            VerifyDepths(rows, 0, 1, 1, 1, 1, 1, 0, 0);
        }

        [TestMethod]
        public void RowNormalizer_OnRowRemoved_simple()
        {
            var dataSet = DataSet<SalesOrder>.ParseJson(StringRes.Sales_Order_71774);
            var rowNormalizer = CreateSimpleRowNormalizer(dataSet);
            var rows = rowNormalizer.Rows;

            Assert.AreEqual(1, rows.Count);
            rows[0].Delete();
            Assert.AreEqual(0, rows.Count);
        }

        [TestMethod]
        public void RowNormalizer_OnRowRemoved_recursive()
        {
            var dataSet = ProductCategoryDataSet.Mock(3);
            var rowNormalizer = CreateRowNormalizer(dataSet);
            var rows = rowNormalizer.Rows;

            VerifyDepths(rows, 0, 0, 0);

            rows[0].Delete();
            VerifyDepths(rows, 0, 0);
        }

        [TestMethod]
        public void RowNormalizer_OnRowMoved()
        {
            var dataSet = ProductCategoryDataSet.Mock(3);
            var _ = dataSet._;
            var orderBy = new ColumnSort[] { _.Name.Desc() };
            var rowNormalizer = CreateRowNormalizer(dataSet, 0, null, orderBy);
            var rows = rowNormalizer.Rows;

            rows[0].Expand();
            VerifyDepths(rows, 0, 1, 1, 1, 0, 0);
            Assert.AreEqual("Name-3-3", rows[1].GetValue(_.Name));
            Assert.AreEqual("Name-3-2", rows[2].GetValue(_.Name));
            Assert.AreEqual("Name-3-1", rows[3].GetValue(_.Name));

            dataSet.SubCategories(2).Update(0, (__, x) => __.Name[x] = "Name-3-4");
            VerifyDepths(rows, 0, 1, 1, 1, 0, 0);
            Assert.AreEqual("Name-3-4", rows[1].GetValue(_.Name));
            Assert.AreEqual("Name-3-3", rows[2].GetValue(_.Name));
            Assert.AreEqual("Name-3-2", rows[3].GetValue(_.Name));

            rows[0].Collapse();
            dataSet.SubCategories(2).Update(1, (__, x) => __.Name[x] = "Name-3-5");
            VerifyDepths(rows, 0, 0, 0);
        }
    }
}
