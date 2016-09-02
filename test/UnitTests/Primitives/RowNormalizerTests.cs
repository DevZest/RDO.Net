using DevZest.Data.Windows.Helpers;
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
    }
}
