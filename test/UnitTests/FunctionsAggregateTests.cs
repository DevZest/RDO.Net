using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class FunctionsAggregateTests
    {
        private const string STR_PADDING = "0000";

        private enum RowValueType
        {
            Default,
            Sum,
            Average
        }

        private class SimpleModel : SimpleModelBase
        {
            public static readonly Mounter<SimpleModel> _Child = RegisterChildModel((SimpleModel x) => x.Child, x => x.ParentKey);

            public static readonly Mounter<_Int32> _Int32Column = RegisterColumn((SimpleModel x) => x.Int32Column);

            public static readonly Mounter<_Int64> _Int64Column = RegisterColumn((SimpleModel x) => x.Int64Column);

            public static readonly Mounter<_Decimal> _DecimalColumn = RegisterColumn((SimpleModel x) => x.DecimalColumn);

            public static readonly Mounter<_Double> _DoubleColumn = RegisterColumn((SimpleModel x) => x.DoubleColumn);

            public static readonly Mounter<_Single> _SingleColumn = RegisterColumn((SimpleModel x) => x.SingleColumn);

            public static readonly Mounter<_String> _StringColumn = RegisterColumn((SimpleModel x) => x.StringColumn);

            public SimpleModel Child { get; private set; }

            public _Int32 Int32Column { get; private set; }

            public _Int64 Int64Column { get; private set; }

            public _Decimal DecimalColumn { get; private set; }

            public _Single SingleColumn { get; private set; }

            public _Double DoubleColumn { get; private set; }

            public _String StringColumn { get; private set; }
        }

        private static DataSet<SimpleModel> GetDataSet(int count)
        {
            return GetDataSet(count, RowValueType.Default);
        }

        private static DataSet<SimpleModel> GetDataSet(int count, RowValueType rowValueType)
        {
            return SimpleModelBase.GetDataSet<SimpleModel>(count, x => x.Child, (d, c) => AddRows(d, c, rowValueType));
        }

        private static void AddRows(DataSet<SimpleModel> dataSet, int count, RowValueType rowValueType)
        {
            var _ = dataSet._;
            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet.AddRow();
                int ordinal = dataRow.Ordinal;
                _.Id[dataRow] = ordinal;
                if (i % count == 1)
                    SetDataRowValues(_, dataRow, null);
                else if (rowValueType == RowValueType.Default)
                    SetDataRowValues(_, dataRow, ordinal);
                else if (rowValueType == RowValueType.Sum)
                    SetDataRowValues(_, dataRow, 1);
                else if (rowValueType == RowValueType.Average)
                    SetDataRowValues(_, dataRow, 2);
            }
        }

        private static void SetDataRowValues(SimpleModel _, DataRow dataRow, int? value)
        {
            _.Int32Column[dataRow] = value;
            _.Int64Column[dataRow] = value;
            _.DoubleColumn[dataRow] = value;
            _.SingleColumn[dataRow] = value;
            _.DecimalColumn[dataRow] = value;
            _.StringColumn[dataRow] = !value.HasValue ? null : value.GetValueOrDefault().ToString(STR_PADDING);
        }

        [TestMethod]
        public void Functions_First()
        {
            int count = 3;

            var dataSet = GetDataSet(count);
            var _ = dataSet._;
            var first1 = _.Id.First();
            ((DbFunctionExpression)first1.DbExpression).Verify(FunctionKeys.First, dataSet._.Id);
            Assert.AreEqual(0, first1.Eval());

            var first2 = _.Child.Id.First();
            ((DbFunctionExpression)first2.DbExpression).Verify(FunctionKeys.First, dataSet._.Child.Id);
            Assert.AreEqual(0, first2.Eval());

            var first3 = _.Child.Child.Id.First();
            ((DbFunctionExpression)first3.DbExpression).Verify(FunctionKeys.First, dataSet._.Child.Child.Id);
            Assert.AreEqual(0, first3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(i * count, first2[dataRow]);
                var childDataSet = dataRow.GetChildDataSet(_.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(i * count * count + j * count, first3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Last()
        {
            int count = 3;

            var dataSet = GetDataSet(count);
            var _ = dataSet._;

            var last1 = _.Id.Last();
            ((DbFunctionExpression)last1.DbExpression).Verify(FunctionKeys.Last, dataSet._.Id);
            Assert.AreEqual(count - 1, last1.Eval());

            var last2 = _.Child.Id.Last();
            ((DbFunctionExpression)last2.DbExpression).Verify(FunctionKeys.Last, dataSet._.Child.Id);
            Assert.AreEqual(count * count - 1, last2.Eval());

            var last3 = _.Child.Child.Id.Last();
            ((DbFunctionExpression)last3.DbExpression).Verify(FunctionKeys.Last, dataSet._.Child.Child.Id);
            Assert.AreEqual(count * count * count - 1, last3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual((i + 1) * count - 1, last2[dataRow]);
                var childDataSet = dataRow.GetChildDataSet(_.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(i * count * count + (j + 1) * count - 1, last3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Count()
        {
            int count = 3;

            var dataSet = GetDataSet(count);
            var _ = dataSet._;

            var count1 = _.Int32Column.Count();
            ((DbFunctionExpression)count1.DbExpression).Verify(FunctionKeys.Count, dataSet._.Int32Column);
            Assert.AreEqual(count - 1, count1.Eval());

            var count2 = _.Child.Int32Column.Count();
            ((DbFunctionExpression)count2.DbExpression).Verify(FunctionKeys.Count, dataSet._.Child.Int32Column);
            Assert.AreEqual(count * (count - 1), count2.Eval());

            var count3 = _.Child.Child.Int32Column.Count();
            ((DbFunctionExpression)count3.DbExpression).Verify(FunctionKeys.Count, dataSet._.Child.Child.Int32Column);
            Assert.AreEqual(count * count * (count - 1), count3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(count - 1, count2[dataRow]);
                var childDataSet = dataRow.GetChildDataSet(_.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(count - 1, count3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_CountRows()
        {
            int count = 3;

            var dataSet = GetDataSet(count);
            var _ = dataSet._;

            var count1 = _.Int32Column.CountRows();
            ((DbFunctionExpression)count1.DbExpression).Verify(FunctionKeys.CountRows, dataSet._.Int32Column);
            Assert.AreEqual(count, count1.Eval());

            var count2 = _.Child.Int32Column.CountRows();
            ((DbFunctionExpression)count2.DbExpression).Verify(FunctionKeys.CountRows, dataSet._.Child.Int32Column);
            Assert.AreEqual(count * count, count2.Eval());

            var count3 = _.Child.Child.Int32Column.CountRows();
            ((DbFunctionExpression)count3.DbExpression).Verify(FunctionKeys.CountRows, dataSet._.Child.Child.Int32Column);
            Assert.AreEqual(count * count * count, count3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(count, count2[dataRow]);
                var childDataSet = dataRow.GetChildDataSet(_.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(count, count3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Sum_Int32()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Sum);
            var _ = dataSet._;

            var sum1 = _.Int32Column.Sum();
            ((DbFunctionExpression)sum1.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Int32Column);
            Assert.AreEqual(count - 1, sum1.Eval());

            var sum2 = _.Child.Int32Column.Sum();
            ((DbFunctionExpression)sum2.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.Int32Column);
            Assert.AreEqual(count * (count - 1), sum2.Eval());

            var sum3 = _.Child.Child.Int32Column.Sum();
            ((DbFunctionExpression)sum3.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.Child.Int32Column);
            Assert.AreEqual(count * count * (count - 1), sum3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(count - 1, sum2[dataRow]);
                var childDataSet = dataRow.GetChildDataSet(_.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(count - 1, sum3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Sum_Int64()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Sum);
            var _ = dataSet._;

            var sum1 = _.Int64Column.Sum();
            ((DbFunctionExpression)sum1.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Int64Column);
            Assert.AreEqual(count - 1, sum1.Eval());

            var sum2 = _.Child.Int64Column.Sum();
            ((DbFunctionExpression)sum2.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.Int64Column);
            Assert.AreEqual(count * (count - 1), sum2.Eval());

            var sum3 = _.Child.Child.Int64Column.Sum();
            ((DbFunctionExpression)sum3.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.Child.Int64Column);
            Assert.AreEqual(count * count * (count - 1), sum3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(count - 1, sum2[dataRow]);
                var childDataSet = dataRow.GetChildDataSet(_.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(count - 1, sum3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Sum_Double()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Sum);
            var _ = dataSet._;

            var sum1 = _.DoubleColumn.Sum();
            ((DbFunctionExpression)sum1.DbExpression).Verify(FunctionKeys.Sum, dataSet._.DoubleColumn);
            Assert.AreEqual(count - 1, sum1.Eval());

            var sum2 = _.Child.DoubleColumn.Sum();
            ((DbFunctionExpression)sum2.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.DoubleColumn);
            Assert.AreEqual(count * (count - 1), sum2.Eval());

            var sum3 = _.Child.Child.DoubleColumn.Sum();
            ((DbFunctionExpression)sum3.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.Child.DoubleColumn);
            Assert.AreEqual(count * count * (count - 1), sum3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(count - 1, sum2[dataRow]);
                var childDataSet = dataRow.GetChildDataSet(_.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(count - 1, sum3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Sum_Decimal()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Sum);
            var _ = dataSet._;

            var sum1 = _.DecimalColumn.Sum();
            ((DbFunctionExpression)sum1.DbExpression).Verify(FunctionKeys.Sum, dataSet._.DecimalColumn);
            Assert.AreEqual(count - 1, sum1.Eval());

            var sum2 = _.Child.DecimalColumn.Sum();
            ((DbFunctionExpression)sum2.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.DecimalColumn);
            Assert.AreEqual(count * (count - 1), sum2.Eval());

            var sum3 = _.Child.Child.DecimalColumn.Sum();
            ((DbFunctionExpression)sum3.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.Child.DecimalColumn);
            Assert.AreEqual(count * count * (count - 1), sum3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(count - 1, sum2[dataRow]);
                var childDataSet = dataRow.GetChildDataSet(_.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(count - 1, sum3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Sum_Single()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Sum);
            var _ = dataSet._;

            var sum1 = _.SingleColumn.Sum();
            ((DbFunctionExpression)sum1.DbExpression).Verify(FunctionKeys.Sum, dataSet._.SingleColumn);
            Assert.AreEqual(count - 1, sum1.Eval());

            var sum2 = _.Child.SingleColumn.Sum();
            ((DbFunctionExpression)sum2.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.SingleColumn);
            Assert.AreEqual(count * (count - 1), sum2.Eval());

            var sum3 = _.Child.Child.SingleColumn.Sum();
            ((DbFunctionExpression)sum3.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.Child.SingleColumn);
            Assert.AreEqual(count * count * (count - 1), sum3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(count - 1, sum2[dataRow]);
                var childDataSet = dataRow.GetChildDataSet(_.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(count - 1, sum3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Min()
        {
            int count = 3;

            var dataSet = GetDataSet(count);
            var _ = dataSet._;

            var min1 = _.Id.Min();
            ((DbFunctionExpression)min1.DbExpression).Verify(FunctionKeys.Min, dataSet._.Id);
            Assert.AreEqual(0, min1.Eval());

            var min2 = _.Child.Id.Min();
            ((DbFunctionExpression)min2.DbExpression).Verify(FunctionKeys.Min, dataSet._.Child.Id);
            Assert.AreEqual(0, min2.Eval());

            var min3 = _.Child.Child.Id.Min();
            ((DbFunctionExpression)min3.DbExpression).Verify(FunctionKeys.Min, dataSet._.Child.Child.Id);
            Assert.AreEqual(0, min3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(i * count, min2[dataRow]);
                var childDataSet = dataRow.GetChildDataSet(_.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(i * count * count + j * count, min3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Min_String()
        {
            int count = 3;

            var dataSet = GetDataSet(count);
            var _ = dataSet._;

            var min1 = _.StringColumn.Min();
            ((DbFunctionExpression)min1.DbExpression).Verify(FunctionKeys.Min, dataSet._.StringColumn);
            Assert.AreEqual(((int)0).ToString(STR_PADDING), min1.Eval());

            var min2 = _.Child.StringColumn.Min();
            ((DbFunctionExpression)min2.DbExpression).Verify(FunctionKeys.Min, dataSet._.Child.StringColumn);
            Assert.AreEqual(((int)0).ToString(STR_PADDING), min2.Eval());

            var min3 = _.Child.Child.StringColumn.Min();
            ((DbFunctionExpression)min3.DbExpression).Verify(FunctionKeys.Min, dataSet._.Child.Child.StringColumn);
            Assert.AreEqual(((int)0).ToString(STR_PADDING), min3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual((i * count).ToString(STR_PADDING), min2[dataRow]);
                var childDataSet = dataRow.GetChildDataSet(_.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual((i * count * count + j * count).ToString(STR_PADDING), min3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Max()
        {
            int count = 3;

            var dataSet = GetDataSet(count);
            var _ = dataSet._;

            var max1 = _.Id.Max();
            ((DbFunctionExpression)max1.DbExpression).Verify(FunctionKeys.Max, dataSet._.Id);
            Assert.AreEqual(count - 1, max1.Eval());

            var max2 = _.Child.Id.Max();
            ((DbFunctionExpression)max2.DbExpression).Verify(FunctionKeys.Max, dataSet._.Child.Id);
            Assert.AreEqual(count * count - 1, max2.Eval());

            var max3 = _.Child.Child.Id.Max();
            ((DbFunctionExpression)max3.DbExpression).Verify(FunctionKeys.Max, dataSet._.Child.Child.Id);
            Assert.AreEqual(count * count * count - 1, max3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual((i + 1) * count - 1, max2[dataRow]);
                var childDataSet = dataRow.GetChildDataSet(_.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(i * count * count + (j + 1) * count - 1, max3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Max_String()
        {
            int count = 3;

            var dataSet = GetDataSet(count);
            var _ = dataSet._;
            var max1 = _.StringColumn.Max();
            ((DbFunctionExpression)max1.DbExpression).Verify(FunctionKeys.Max, dataSet._.StringColumn);
            Assert.AreEqual((count - 1).ToString(STR_PADDING), max1.Eval());

            var max2 = _.Child.StringColumn.Max();
            ((DbFunctionExpression)max2.DbExpression).Verify(FunctionKeys.Max, dataSet._.Child.StringColumn);
            Assert.AreEqual((count * count - 1).ToString(STR_PADDING), max2.Eval());

            var max3 = _.Child.Child.StringColumn.Max();
            ((DbFunctionExpression)max3.DbExpression).Verify(FunctionKeys.Max, dataSet._.Child.Child.StringColumn);
            Assert.AreEqual((count * count * count - 1).ToString(STR_PADDING), max3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(((i + 1) * count - 1).ToString(STR_PADDING), max2[dataRow]);
                var childDataSet = dataRow.GetChildDataSet(_.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual((i * count * count + (j + 1) * count - 1).ToString(STR_PADDING), max3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Average_Decimal()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Average);
            var _ = dataSet._;

            var avg1 = dataSet._.DecimalColumn.Average();
            ((DbFunctionExpression)avg1.DbExpression).Verify(FunctionKeys.Average, dataSet._.DecimalColumn);
            Assert.AreEqual(2, avg1.Eval());

            var avg2 = dataSet._.Child.DecimalColumn.Average();
            ((DbFunctionExpression)avg2.DbExpression).Verify(FunctionKeys.Average, dataSet._.Child.DecimalColumn);
            Assert.AreEqual(2, avg2.Eval());

            var avg3 = dataSet._.Child.Child.DecimalColumn.Average();
            ((DbFunctionExpression)avg3.DbExpression).Verify(FunctionKeys.Average, dataSet._.Child.Child.DecimalColumn);
            Assert.AreEqual(2, avg3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(2, avg2[dataRow]);
                var childDataSet = dataRow.GetChildDataSet(_.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(2, avg3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Average_Double()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Average);
            var _ = dataSet._;

            var avg1 = dataSet._.DoubleColumn.Average();
            ((DbFunctionExpression)avg1.DbExpression).Verify(FunctionKeys.Average, dataSet._.DoubleColumn);
            Assert.AreEqual(2, avg1.Eval());

            var avg2 = dataSet._.Child.DoubleColumn.Average();
            ((DbFunctionExpression)avg2.DbExpression).Verify(FunctionKeys.Average, dataSet._.Child.DoubleColumn);
            Assert.AreEqual(2, avg2.Eval());

            var avg3 = dataSet._.Child.Child.DoubleColumn.Average();
            ((DbFunctionExpression)avg3.DbExpression).Verify(FunctionKeys.Average, dataSet._.Child.Child.DoubleColumn);
            Assert.AreEqual(2, avg3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(2, avg2[dataRow]);
                var childDataSet = dataRow.GetChildDataSet(_.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(2, avg3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Average_Int32()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Average);
            var _ = dataSet._;

            var avg1 = dataSet._.Int32Column.Average();
            ((DbFunctionExpression)avg1.DbExpression).Verify(FunctionKeys.Average, dataSet._.Int32Column);
            Assert.AreEqual(2, avg1.Eval());

            var avg2 = dataSet._.Child.Int32Column.Average();
            ((DbFunctionExpression)avg2.DbExpression).Verify(FunctionKeys.Average, dataSet._.Child.Int32Column);
            Assert.AreEqual(2, avg2.Eval());

            var avg3 = dataSet._.Child.Child.Int32Column.Average();
            ((DbFunctionExpression)avg3.DbExpression).Verify(FunctionKeys.Average, dataSet._.Child.Child.Int32Column);
            Assert.AreEqual(2, avg3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(2, avg2[dataRow]);
                var childDataSet = dataRow.GetChildDataSet(_.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(2, avg3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Average_Int64()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Average);
            var _ = dataSet._;

            var avg1 = dataSet._.Int64Column.Average();
            ((DbFunctionExpression)avg1.DbExpression).Verify(FunctionKeys.Average, dataSet._.Int64Column);
            Assert.AreEqual(2, avg1.Eval());

            var avg2 = dataSet._.Child.Int64Column.Average();
            ((DbFunctionExpression)avg2.DbExpression).Verify(FunctionKeys.Average, dataSet._.Child.Int64Column);
            Assert.AreEqual(2, avg2.Eval());

            var avg3 = dataSet._.Child.Child.Int64Column.Average();
            ((DbFunctionExpression)avg3.DbExpression).Verify(FunctionKeys.Average, dataSet._.Child.Child.Int64Column);
            Assert.AreEqual(2, avg3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(2, avg2[dataRow]);
                var childDataSet = dataRow.GetChildDataSet(_.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(2, avg3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Average_Single()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Average);
            var _ = dataSet._;

            var avg1 = dataSet._.SingleColumn.Average();
            ((DbFunctionExpression)avg1.DbExpression).Verify(FunctionKeys.Average, dataSet._.SingleColumn);
            Assert.AreEqual(2, avg1.Eval());

            var avg2 = dataSet._.Child.SingleColumn.Average();
            ((DbFunctionExpression)avg2.DbExpression).Verify(FunctionKeys.Average, dataSet._.Child.SingleColumn);
            Assert.AreEqual(2, avg2.Eval());

            var avg3 = dataSet._.Child.Child.SingleColumn.Average();
            ((DbFunctionExpression)avg3.DbExpression).Verify(FunctionKeys.Average, dataSet._.Child.Child.SingleColumn);
            Assert.AreEqual(2, avg3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(2, avg2[dataRow]);
                var childDataSet = dataRow.GetChildDataSet(_.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(2, avg3[childDataRow]);
                }
            }
        }
    }
}
