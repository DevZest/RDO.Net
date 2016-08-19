using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Globalization;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class RowManagerTestsBase
    {
        protected static DataSet<ProductCategory> MockProductCategories(int count, bool multiLevel = true)
        {
            var dataSet = DataSet<ProductCategory>.New();
            var model = dataSet._;

            string namePrefix = "Name";
            AddRows(dataSet, namePrefix, count);
            if (multiLevel)
            {
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
            public ConcreteRowManager(Template template, DataSet dataSet, _Boolean where = null, ColumnSort[] orderBy = null)
                : base(template, dataSet, where, orderBy)
            {
            }

            internal override void Invalidate(RowPresenter row)
            {
            }
        }

        internal static RowManager CreateRowManager<T>(DataSet<T> dataSet, RowPlaceholderStrategy rowPlaceholderStrategy)
            where T : Model, new()
        {
            var template = new Template();
            template.RowPlaceholderStrategy = rowPlaceholderStrategy;
            RowManager result = new ConcreteRowManager(template, dataSet);
            return result;
        }

        internal static RowManager CreateRowManager<T>(DataSet<T> dataSet, int hierarchicalModelOrdinal = 0)
            where T : Model, new()
        {
            var template = new Template();
            template.RecursiveModelOrdinal = hierarchicalModelOrdinal;
            RowManager result = new ConcreteRowManager(template, dataSet);
            return result;
        }

        protected static void VerifyDepths(IReadOnlyList<RowPresenter> rows, params int[] depths)
        {
            Assert.AreEqual(rows.Count, depths.Length);

            for (int i = 0; i < rows.Count; i++)
                Assert.AreEqual(depths[i], rows[i].Depth);
        }

        protected static void VerifyRowOrdinal(IReadOnlyList<RowPresenter> rows)
        {
            for (int i = 0; i < rows.Count; i++)
                Assert.AreEqual(i, rows[i].Index);
        }
    }
}
