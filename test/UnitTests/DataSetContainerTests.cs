using DevZest.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class DataSetContainerTests : SimpleModelDataSetHelper
    {
        [TestMethod]
        public void DataSetContainer_CreateLocalColumn()
        {
            int count = 3;
            var dataSet = GetDataSet(count, false);
            var localColumn = dataSet.Container.CreateLocalColumn<int>(dataSet._, builder =>
            {
                builder.DefaultValue(2);
            });
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
        public void DataSetContainer_CreateLocalColumn_1_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, false);
            var _ = dataSet._;
            var localColumn = dataSet.Container.CreateLocalColumn(_, _.Id, Calculate1);
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
        public void DataSetContainer_CreateLocalColumn_2_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, false);
            var _ = dataSet._;
            var localColumn = dataSet.Container.CreateLocalColumn(_, _.Id, _.Id, Calculate2);
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
        public void DataSetContainer_CreateLocalColumn_3_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, false);
            var _ = dataSet._;
            var localColumn = dataSet.Container.CreateLocalColumn(_, _.Id, _.Id, _.Id, Calculate3);
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
        public void DataSetContainer_CreateLocalColumn_4_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, false);
            var _ = dataSet._;
            var localColumn = dataSet.Container.CreateLocalColumn(_, _.Id, _.Id, _.Id, _.Id, Calculate4);
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
        public void DataSetContainer_CreateLocalColumn_5_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, false);
            var _ = dataSet._;
            var localColumn = dataSet.Container.CreateLocalColumn(_, _.Id, _.Id, _.Id, _.Id, _.Id, Calculate5);
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
        public void DataSetContainer_CreateLocalColumn_6_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, false);
            var _ = dataSet._;
            var localColumn = dataSet.Container.CreateLocalColumn(_, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, Calculate6);
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
        public void DataSetContainer_CreateLocalColumn_7_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, false);
            var _ = dataSet._;
            var localColumn = dataSet.Container.CreateLocalColumn(_, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, Calculate7);
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
        public void DataSetContainer_CreateLocalColumn_8_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, false);
            var _ = dataSet._;
            var localColumn = dataSet.Container.CreateLocalColumn(_, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, Calculate8);
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
        public void DataSetContainer_CreateLocalColumn_9_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, false);
            var _ = dataSet._;
            var localColumn = dataSet.Container.CreateLocalColumn(_, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, Calculate9);
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
        public void DataSetContainer_CreateLocalColumn_10_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, false);
            var _ = dataSet._;
            var localColumn = dataSet.Container.CreateLocalColumn(_, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, Calculate10);
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
        public void DataSetContainer_CreateLocalColumn_11_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, false);
            var _ = dataSet._;
            var localColumn = dataSet.Container.CreateLocalColumn(_, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, Calculate11);
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
        public void DataSetContainer_CreateLocalColumn_12_input_column()
        {
            int count = 3;
            var dataSet = GetDataSet(count, false);
            var _ = dataSet._;
            var localColumn = dataSet.Container.CreateLocalColumn(_, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, _.Id, Calculate12);
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
