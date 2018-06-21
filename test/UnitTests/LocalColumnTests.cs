using DevZest.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class LocalColumnTests : SimpleModelDataSetHelper
    {
        [TestMethod]
        public void LocalColumn_DefaultValue()
        {
            int count = 3;
            var dataSet = GetDataSet(count, _ => _.LocalColumn.SetDefaultValue(2, null, null));
            var localColumn = dataSet._.LocalColumn;
            Assert.AreEqual(2, localColumn[dataSet[0]]);
            Assert.AreEqual(2, localColumn[dataSet[1]]);
            Assert.AreEqual(2, localColumn[dataSet[2]]);
            localColumn[0] = 4;
            localColumn[1] = 5;
            localColumn[2] = 6;
            Assert.AreEqual(4, localColumn[dataSet[0]]);
            Assert.AreEqual(5, localColumn[dataSet[1]]);
            Assert.AreEqual(6, localColumn[dataSet[2]]);

            dataSet.Insert(0, new DataRow());
            Assert.AreEqual(2, localColumn[dataSet[0]]);
            Assert.AreEqual(4, localColumn[dataSet[1]]);
            Assert.AreEqual(5, localColumn[dataSet[2]]);
            Assert.AreEqual(6, localColumn[dataSet[3]]);

            dataSet.RemoveAt(2);
            Assert.AreEqual(2, localColumn[dataSet[0]]);
            Assert.AreEqual(4, localColumn[dataSet[1]]);
            Assert.AreEqual(6, localColumn[dataSet[2]]);
        }

        [TestMethod]
        public void LocalColumn_ComputedAs_1_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, x => x.LocalColumn.ComputedAs(x.Id, Calculate1, false));
            var _ = dataSet._;
            var localColumn = _.LocalColumn;
            Assert.AreEqual(localColumn[0], _.Id[0]);
            Assert.AreEqual(localColumn[1], _.Id[1]);
            Assert.AreEqual(localColumn[2], _.Id[2]);

            _.Id[1] = 5;
            Assert.AreEqual(5, localColumn[1]);
        }

        private static int Calculate1(DataRow dataRow, _Int32 id)
        {
            return id[dataRow].Value;
        }

        [TestMethod]
        public void LocalColumn_ComputedAs_2_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, x => x.LocalColumn.ComputedAs(x.Id, x.Id, Calculate2, false));
            var _ = dataSet._;
            var localColumn = _.LocalColumn;
            Assert.AreEqual(0, localColumn[0]);
            Assert.AreEqual(2, localColumn[1]);
            Assert.AreEqual(4, localColumn[2]);

            _.Id[1] = 5;
            Assert.AreEqual(10, localColumn[1]);
        }

        private static int Calculate2(DataRow dataRow, _Int32 id1, _Int32 id2)
        {
            return id1[dataRow].Value + id2[dataRow].Value;
        }

        [TestMethod]
        public void LocalColumn_ComputedAs_3_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, x => x.LocalColumn.ComputedAs(x.Id, x.Id, x.Id, Calculate3, false));
            var _ = dataSet._;
            var localColumn = _.LocalColumn;
            Assert.AreEqual(0, localColumn[0]);
            Assert.AreEqual(3, localColumn[1]);
            Assert.AreEqual(6, localColumn[2]);

            _.Id[1] = 5;
            Assert.AreEqual(15, localColumn[1]);
        }

        private static int Calculate3(DataRow dataRow, _Int32 id1, _Int32 id2, _Int32 id3)
        {
            return id1[dataRow].Value + id2[dataRow].Value + id3[dataRow].Value;
        }

        [TestMethod]
        public void LocalColumn_ComputedAs_4_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, x => x.LocalColumn.ComputedAs(x.Id, x.Id, x.Id, x.Id, Calculate4, false));
            var _ = dataSet._;
            var localColumn = _.LocalColumn;
            Assert.AreEqual(0, localColumn[0]);
            Assert.AreEqual(4, localColumn[1]);
            Assert.AreEqual(8, localColumn[2]);

            _.Id[1] = 5;
            Assert.AreEqual(20, localColumn[1]);
        }

        private static int Calculate4(DataRow dataRow, _Int32 id1, _Int32 id2, _Int32 id3, _Int32 id4)
        {
            return id1[dataRow].Value + id2[dataRow].Value + id3[dataRow].Value + id4[dataRow].Value;
        }

        [TestMethod]
        public void LocalColumn_ComputedAs_5_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, x => x.LocalColumn.ComputedAs(x.Id, x.Id, x.Id, x.Id, x.Id, Calculate5, false));
            var _ = dataSet._;
            var localColumn = _.LocalColumn;
            Assert.AreEqual(0, localColumn[0]);
            Assert.AreEqual(5, localColumn[1]);
            Assert.AreEqual(10, localColumn[2]);

            _.Id[1] = 5;
            Assert.AreEqual(25, localColumn[1]);
        }

        private static int Calculate5(DataRow dataRow, _Int32 id1, _Int32 id2, _Int32 id3, _Int32 id4, _Int32 id5)
        {
            return id1[dataRow].Value + id2[dataRow].Value + id3[dataRow].Value + id4[dataRow].Value + id5[dataRow].Value;
        }

        [TestMethod]
        public void LocalColumn_ComputedAs_6_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, x => x.LocalColumn.ComputedAs(x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, Calculate6, false));
            var _ = dataSet._;
            var localColumn = _.LocalColumn;
            Assert.AreEqual(0, localColumn[0]);
            Assert.AreEqual(6, localColumn[1]);
            Assert.AreEqual(12, localColumn[2]);

            _.Id[1] = 5;
            Assert.AreEqual(30, localColumn[1]);
        }

        private static int Calculate6(DataRow dataRow, _Int32 id1, _Int32 id2, _Int32 id3, _Int32 id4, _Int32 id5, _Int32 id6)
        {
            return id1[dataRow].Value + id2[dataRow].Value + id3[dataRow].Value + id4[dataRow].Value + id5[dataRow].Value + id6[dataRow].Value;
        }

        [TestMethod]
        public void LocalColumn_ComputedAs_7_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, x => x.LocalColumn.ComputedAs(x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, Calculate7, false));
            var _ = dataSet._;
            var localColumn = _.LocalColumn;
            Assert.AreEqual(0, localColumn[0]);
            Assert.AreEqual(7, localColumn[1]);
            Assert.AreEqual(14, localColumn[2]);

            _.Id[1] = 5;
            Assert.AreEqual(35, localColumn[1]);
        }

        private static int Calculate7(DataRow dataRow, _Int32 id1, _Int32 id2, _Int32 id3, _Int32 id4, _Int32 id5, _Int32 id6, _Int32 id7)
        {
            return id1[dataRow].Value + id2[dataRow].Value + id3[dataRow].Value + id4[dataRow].Value + id5[dataRow].Value + id6[dataRow].Value + id7[dataRow].Value;
        }

        [TestMethod]
        public void LocalColumn_ComputedAs_8_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, x => x.LocalColumn.ComputedAs(x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, Calculate8, false));
            var _ = dataSet._;
            var localColumn = _.LocalColumn;
            Assert.AreEqual(0, localColumn[0]);
            Assert.AreEqual(8, localColumn[1]);
            Assert.AreEqual(16, localColumn[2]);

            _.Id[1] = 5;
            Assert.AreEqual(40, localColumn[1]);
        }

        private static int Calculate8(DataRow dataRow, _Int32 id1, _Int32 id2, _Int32 id3, _Int32 id4, _Int32 id5, _Int32 id6, _Int32 id7, _Int32 id8)
        {
            return id1[dataRow].Value + id2[dataRow].Value + id3[dataRow].Value + id4[dataRow].Value + id5[dataRow].Value + id6[dataRow].Value
                + id7[dataRow].Value + id8[dataRow].Value;
        }

        [TestMethod]
        public void LocalColumn_ComputedAs_9_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, x => x.LocalColumn.ComputedAs(x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, Calculate9, false));
            var _ = dataSet._;
            var localColumn = _.LocalColumn;
            Assert.AreEqual(0, localColumn[0]);
            Assert.AreEqual(9, localColumn[1]);
            Assert.AreEqual(18, localColumn[2]);

            _.Id[1] = 5;
            Assert.AreEqual(45, localColumn[1]);
        }

        private static int Calculate9(DataRow dataRow, _Int32 id1, _Int32 id2, _Int32 id3, _Int32 id4, _Int32 id5, _Int32 id6, _Int32 id7, _Int32 id8, _Int32 id9)
        {
            return id1[dataRow].Value + id2[dataRow].Value + id3[dataRow].Value + id4[dataRow].Value + id5[dataRow].Value + id6[dataRow].Value
                + id7[dataRow].Value + id8[dataRow].Value + id9[dataRow].Value;
        }

        [TestMethod]
        public void LocalColumn_ComputedAs_10_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, x => x.LocalColumn.ComputedAs(x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, Calculate10, false));
            var _ = dataSet._;
            var localColumn = _.LocalColumn;
            Assert.AreEqual(0, localColumn[0]);
            Assert.AreEqual(10, localColumn[1]);
            Assert.AreEqual(20, localColumn[2]);

            _.Id[1] = 5;
            Assert.AreEqual(50, localColumn[1]);
        }

        private static int Calculate10(DataRow dataRow, _Int32 id1, _Int32 id2, _Int32 id3, _Int32 id4, _Int32 id5, _Int32 id6, _Int32 id7,
            _Int32 id8, _Int32 id9, _Int32 id10)
        {
            return id1[dataRow].Value + id2[dataRow].Value + id3[dataRow].Value + id4[dataRow].Value + id5[dataRow].Value + id6[dataRow].Value
                + id7[dataRow].Value + id8[dataRow].Value + id9[dataRow].Value + id10[dataRow].Value;
        }

        [TestMethod]
        public void LocalColumn_ComputedAs_11_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, x => x.LocalColumn.ComputedAs(x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, Calculate11, false));
            var _ = dataSet._;
            var localColumn = _.LocalColumn;
            Assert.AreEqual(0, localColumn[0]);
            Assert.AreEqual(11, localColumn[1]);
            Assert.AreEqual(22, localColumn[2]);

            _.Id[1] = 5;
            Assert.AreEqual(55, localColumn[1]);
        }

        private static int Calculate11(DataRow dataRow, _Int32 id1, _Int32 id2, _Int32 id3, _Int32 id4, _Int32 id5, _Int32 id6, _Int32 id7,
            _Int32 id8, _Int32 id9, _Int32 id10, _Int32 id11)
        {
            return id1[dataRow].Value + id2[dataRow].Value + id3[dataRow].Value + id4[dataRow].Value + id5[dataRow].Value + id6[dataRow].Value
                + id7[dataRow].Value + id8[dataRow].Value + id9[dataRow].Value + id10[dataRow].Value + id11[dataRow].Value;
        }

        [TestMethod]
        public void LocalColumn_ComputedAs_12_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, x => x.LocalColumn.ComputedAs(x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, x.Id, Calculate12, false));
            var _ = dataSet._;
            var localColumn = _.LocalColumn;
            Assert.AreEqual(0, localColumn[0]);
            Assert.AreEqual(12, localColumn[1]);
            Assert.AreEqual(24, localColumn[2]);

            _.Id[1] = 5;
            Assert.AreEqual(60, localColumn[1]);
        }

        private static int Calculate12(DataRow dataRow, _Int32 id1, _Int32 id2, _Int32 id3, _Int32 id4, _Int32 id5, _Int32 id6, _Int32 id7,
            _Int32 id8, _Int32 id9, _Int32 id10, _Int32 id11, _Int32 id12)
        {
            return id1[dataRow].Value + id2[dataRow].Value + id3[dataRow].Value + id4[dataRow].Value + id5[dataRow].Value + id6[dataRow].Value
                + id7[dataRow].Value + id8[dataRow].Value + id9[dataRow].Value + id10[dataRow].Value + id11[dataRow].Value + id12[dataRow].Value;
        }
    }
}
