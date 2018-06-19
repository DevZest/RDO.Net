using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class ColumnTranslateToTests
    {
        private class SimpleModel : SimpleModelBase
        {
            public static readonly Mounter<SimpleModel> _Child = RegisterChildModel((SimpleModel x) => x.Child, x => x.ParentKey);

            public static readonly Mounter<_Int32> _Int32Column = RegisterColumn((SimpleModel x) => x.Int32Column);

            public SimpleModel Child { get; private set; }

            public _Int32 Int32Column { get; private set; }
        }

        private static DataSet<SimpleModel> GetDataSet(int count)
        {
            return SimpleModelBase.GetDataSet<SimpleModel>(count, x => x.Child, (d, c) => AddRows(d, c));
        }

        private static void AddRows(DataSet<SimpleModel> dataSet, int count)
        {
            var _ = dataSet._;
            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet.AddRow();
                int ordinal = dataRow.Ordinal;
                _.Id[dataRow] = ordinal;
                _.Int32Column[dataRow] = 1;
            }
        }

        [TestMethod]
        public void Column_TranslateTo_moded_member()
        {
            var dataSet1 = GetDataSet(3);
            var id1 = dataSet1._.Id;
            var dataSet2 = GetDataSet(5);
            var id2 = id1.TranslateTo(dataSet2._);
            Assert.AreEqual(id2, dataSet2._.Id);
        }

        [TestMethod]
        public void Column_TranslateTo_function()
        {
            var dataSet1 = GetDataSet(3);
            var sum1 = dataSet1._.Int32Column.Sum();
            Assert.AreEqual(3, sum1.Eval());

            var dataSet2 = GetDataSet(5);
            var sum2 = sum1.TranslateTo(dataSet2._);
            Assert.AreEqual(5, sum2.Eval());
        }

        [TestMethod]
        public void Column_TranslateTo_unary()
        {
            var dataSet1 = GetDataSet(3);
            var column1 = -dataSet1._.Int32Column;
            var dataSet2 = GetDataSet(5);
            var column2 = column1.TranslateTo(dataSet2._);
            Assert.AreEqual(-1, column2[4]);
        }

        [TestMethod]
        public void Column_TranslateTo_binary()
        {
            var dataSet1 = GetDataSet(3);
            var column1 = dataSet1._.Id + dataSet1._.Int32Column;
            var dataSet2 = GetDataSet(5);
            var column2 = column1.TranslateTo(dataSet2._);
            Assert.AreEqual(5, column2[4]);
        }

        [TestMethod]
        public void Column_TranslateTo_cast()
        {
            var dataSet1 = GetDataSet(3);
            var column1 = (_String)dataSet1._.Int32Column;
            var dataSet2 = GetDataSet(5);
            var column2 = column1.TranslateTo(dataSet2._);
            Assert.AreEqual("1", column2[4]);
        }
    }
}
