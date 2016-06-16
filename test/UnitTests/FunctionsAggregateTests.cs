using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using DevZest.Data.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class FunctionsAggregateTests : ColumnConverterTestsBase
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
            public static readonly Accessor<SimpleModel, SimpleModel> ChildAccessor = RegisterChildModel((SimpleModel x) => x.Child,
                x => x.ParentKey);

            public static readonly Accessor<SimpleModel, _Int32> Int32ColumnAccessor = RegisterColumn((SimpleModel x) => x.Int32Column);

            public static readonly Accessor<SimpleModel, _Int64> Int64ColumnAccessor = RegisterColumn((SimpleModel x) => x.Int64Column);

            public static readonly Accessor<SimpleModel, _Decimal> DecimalColumnAccessor = RegisterColumn((SimpleModel x) => x.DecimalColumn);

            public static readonly Accessor<SimpleModel, _Double> DoubleColumnAccessor = RegisterColumn((SimpleModel x) => x.DoubleColumn);

            public static readonly Accessor<SimpleModel, _Single> SingleColumnAccessor = RegisterColumn((SimpleModel x) => x.SingleColumn);

            public static readonly Accessor<SimpleModel, _String> StringColumnAccessor = RegisterColumn((SimpleModel x) => x.StringColumn);

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
            var model = dataSet._;
            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet.AddRow();
                int ordinal = dataRow.Ordinal;
                model.Id[dataRow] = ordinal;
                if (i % count == 1)
                    SetDataRowValues(model, dataRow, null);
                else if (rowValueType == RowValueType.Default)
                    SetDataRowValues(model, dataRow, ordinal);
                else if (rowValueType == RowValueType.Sum)
                    SetDataRowValues(model, dataRow, 1);
                else if (rowValueType == RowValueType.Average)
                    SetDataRowValues(model, dataRow, 2);
            }
        }

        private static void SetDataRowValues(SimpleModel model, DataRow dataRow, int? value)
        {
            model.Int32Column[dataRow] = value;
            model.Int64Column[dataRow] = value;
            model.DoubleColumn[dataRow] = value;
            model.SingleColumn[dataRow] = value;
            model.DecimalColumn[dataRow] = value;
            model.StringColumn[dataRow] = !value.HasValue ? null : value.GetValueOrDefault().ToString(STR_PADDING);
        }

        [TestMethod]
        public void Functions_First()
        {
            int count = 3;

            var dataSet = GetDataSet(count);
            var x = dataSet._;
            var first1 = x.Id.First();
            ((DbFunctionExpression)first1.DbExpression).Verify(FunctionKeys.First, dataSet._.Id);
            Assert.AreEqual(0, first1.Eval());

            var first2 = x.Child.Id.First();
            ((DbFunctionExpression)first2.DbExpression).Verify(FunctionKeys.First, dataSet._.Child.Id);
            Assert.AreEqual(0, first2.Eval());

            var first3 = x.Child.Child.Id.First();
            ((DbFunctionExpression)first3.DbExpression).Verify(FunctionKeys.First, dataSet._.Child.Child.Id);
            Assert.AreEqual(0, first3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(i * count, first2[dataRow]);
                var childDataSet = dataRow.Children(x.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(i * count * count + j * count, first3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_First_Converter()
        {
            var column = GetDataSet(3)._.Id.First();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_First, json);

            var fromJsonColumn = (_Int32)Column.FromJson(GetDataSet(3)._, json);
            Assert.AreEqual(0, fromJsonColumn.Eval());
        }

        [TestMethod]
        public void Functions_Last()
        {
            int count = 3;

            var dataSet = GetDataSet(count);
            var model = dataSet._;

            var last1 = model.Id.Last();
            ((DbFunctionExpression)last1.DbExpression).Verify(FunctionKeys.Last, dataSet._.Id);
            Assert.AreEqual(count - 1, last1.Eval());

            var last2 = model.Child.Id.Last();
            ((DbFunctionExpression)last2.DbExpression).Verify(FunctionKeys.Last, dataSet._.Child.Id);
            Assert.AreEqual(count * count - 1, last2.Eval());

            var last3 = model.Child.Child.Id.Last();
            ((DbFunctionExpression)last3.DbExpression).Verify(FunctionKeys.Last, dataSet._.Child.Child.Id);
            Assert.AreEqual(count * count * count - 1, last3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual((i + 1) * count - 1, last2[dataRow]);
                var childDataSet = dataRow.Children(model.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(i * count * count + (j + 1) * count - 1, last3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Last_Converter()
        {
            var column = GetDataSet(3)._.Id.Last();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_Last, json);

            var fromJsonColumn = (_Int32)Column.FromJson(GetDataSet(3)._, json);
            Assert.AreEqual(2, fromJsonColumn.Eval());
        }

        [TestMethod]
        public void Functions_Count()
        {
            int count = 3;

            var dataSet = GetDataSet(count);
            var model = dataSet._;

            var count1 = model.Int32Column.Count();
            ((DbFunctionExpression)count1.DbExpression).Verify(FunctionKeys.Count, dataSet._.Int32Column);
            Assert.AreEqual(count - 1, count1.Eval());

            var count2 = model.Child.Int32Column.Count();
            ((DbFunctionExpression)count2.DbExpression).Verify(FunctionKeys.Count, dataSet._.Child.Int32Column);
            Assert.AreEqual(count * (count - 1), count2.Eval());

            var count3 = model.Child.Child.Int32Column.Count();
            ((DbFunctionExpression)count3.DbExpression).Verify(FunctionKeys.Count, dataSet._.Child.Child.Int32Column);
            Assert.AreEqual(count * count * (count - 1), count3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(count - 1, count2[dataRow]);
                var childDataSet = dataRow.Children(model.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(count - 1, count3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Count_Converter()
        {
            var column = GetDataSet(3)._.Int32Column.Count();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_Count, json);

            var fromJsonColumn = (_Int32)Column.FromJson(GetDataSet(5)._, json);
            Assert.AreEqual(4, fromJsonColumn.Eval());
        }

        [TestMethod]
        public void Functions_CountRows()
        {
            int count = 3;

            var dataSet = GetDataSet(count);
            var model = dataSet._;

            var count1 = model.Int32Column.CountRows();
            ((DbFunctionExpression)count1.DbExpression).Verify(FunctionKeys.CountRows, dataSet._.Int32Column);
            Assert.AreEqual(count, count1.Eval());

            var count2 = model.Child.Int32Column.CountRows();
            ((DbFunctionExpression)count2.DbExpression).Verify(FunctionKeys.CountRows, dataSet._.Child.Int32Column);
            Assert.AreEqual(count * count, count2.Eval());

            var count3 = model.Child.Child.Int32Column.CountRows();
            ((DbFunctionExpression)count3.DbExpression).Verify(FunctionKeys.CountRows, dataSet._.Child.Child.Int32Column);
            Assert.AreEqual(count * count * count, count3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(count, count2[dataRow]);
                var childDataSet = dataRow.Children(model.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(count, count3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_CountRows_Converter()
        {
            var column = GetDataSet(3)._.Int32Column.CountRows();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_CountRows, json);

            var fromJsonColumn = (_Int32)Column.FromJson(GetDataSet(5)._, json);
            Assert.AreEqual(5, fromJsonColumn.Eval());
        }

        [TestMethod]
        public void Functions_Sum_Int32()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Sum);
            var model = dataSet._;

            var sum1 = model.Int32Column.Sum();
            ((DbFunctionExpression)sum1.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Int32Column);
            Assert.AreEqual(count - 1, sum1.Eval());

            var sum2 = model.Child.Int32Column.Sum();
            ((DbFunctionExpression)sum2.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.Int32Column);
            Assert.AreEqual(count * (count - 1), sum2.Eval());

            var sum3 = model.Child.Child.Int32Column.Sum();
            ((DbFunctionExpression)sum3.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.Child.Int32Column);
            Assert.AreEqual(count * count * (count - 1), sum3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(count - 1, sum2[dataRow]);
                var childDataSet = dataRow.Children(model.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(count - 1, sum3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Sum_Int32_Converter()
        {
            var column = GetDataSet(3, RowValueType.Sum)._.Int32Column.Sum();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_Sum_Int32, json);

            var fromJsonColumn = (_Int32)Column.FromJson(GetDataSet(5, RowValueType.Sum)._, json);
            Assert.AreEqual(4, fromJsonColumn.Eval());
        }

        [TestMethod]
        public void Functions_Sum_Int64()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Sum);
            var model = dataSet._;

            var sum1 = model.Int64Column.Sum();
            ((DbFunctionExpression)sum1.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Int64Column);
            Assert.AreEqual(count - 1, sum1.Eval());

            var sum2 = model.Child.Int64Column.Sum();
            ((DbFunctionExpression)sum2.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.Int64Column);
            Assert.AreEqual(count * (count - 1), sum2.Eval());

            var sum3 = model.Child.Child.Int64Column.Sum();
            ((DbFunctionExpression)sum3.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.Child.Int64Column);
            Assert.AreEqual(count * count * (count - 1), sum3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(count - 1, sum2[dataRow]);
                var childDataSet = dataRow.Children(model.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(count - 1, sum3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Sum_Int64_Converter()
        {
            var column = GetDataSet(3, RowValueType.Sum)._.Int64Column.Sum();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_Sum_Int64, json);

            var fromJsonColumn = (_Int64)Column.FromJson(GetDataSet(5, RowValueType.Sum)._, json);
            Assert.AreEqual(4, fromJsonColumn.Eval());
        }

        [TestMethod]
        public void Functions_Sum_Double()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Sum);
            var model = dataSet._;

            var sum1 = model.DoubleColumn.Sum();
            ((DbFunctionExpression)sum1.DbExpression).Verify(FunctionKeys.Sum, dataSet._.DoubleColumn);
            Assert.AreEqual(count - 1, sum1.Eval());

            var sum2 = model.Child.DoubleColumn.Sum();
            ((DbFunctionExpression)sum2.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.DoubleColumn);
            Assert.AreEqual(count * (count - 1), sum2.Eval());

            var sum3 = model.Child.Child.DoubleColumn.Sum();
            ((DbFunctionExpression)sum3.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.Child.DoubleColumn);
            Assert.AreEqual(count * count * (count - 1), sum3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(count - 1, sum2[dataRow]);
                var childDataSet = dataRow.Children(model.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(count - 1, sum3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Sum_Double_Converter()
        {
            var column = GetDataSet(3, RowValueType.Sum)._.DoubleColumn.Sum();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_Sum_Double, json);

            var fromJsonColumn = (_Double)Column.FromJson(GetDataSet(5, RowValueType.Sum)._, json);
            Assert.AreEqual(4, fromJsonColumn.Eval());
        }

        [TestMethod]
        public void Functions_Sum_Decimal()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Sum);
            var model = dataSet._;

            var sum1 = model.DecimalColumn.Sum();
            ((DbFunctionExpression)sum1.DbExpression).Verify(FunctionKeys.Sum, dataSet._.DecimalColumn);
            Assert.AreEqual(count - 1, sum1.Eval());

            var sum2 = model.Child.DecimalColumn.Sum();
            ((DbFunctionExpression)sum2.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.DecimalColumn);
            Assert.AreEqual(count * (count - 1), sum2.Eval());

            var sum3 = model.Child.Child.DecimalColumn.Sum();
            ((DbFunctionExpression)sum3.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.Child.DecimalColumn);
            Assert.AreEqual(count * count * (count - 1), sum3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(count - 1, sum2[dataRow]);
                var childDataSet = dataRow.Children(model.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(count - 1, sum3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Sum_Decimal_Converter()
        {
            var column = GetDataSet(3, RowValueType.Sum)._.DecimalColumn.Sum();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_Sum_Decimal, json);

            var fromJsonColumn = (_Decimal)Column.FromJson(GetDataSet(5, RowValueType.Sum)._, json);
            Assert.AreEqual(4, fromJsonColumn.Eval());
        }

        [TestMethod]
        public void Functions_Sum_Single()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Sum);
            var model = dataSet._;

            var sum1 = model.SingleColumn.Sum();
            ((DbFunctionExpression)sum1.DbExpression).Verify(FunctionKeys.Sum, dataSet._.SingleColumn);
            Assert.AreEqual(count - 1, sum1.Eval());

            var sum2 = model.Child.SingleColumn.Sum();
            ((DbFunctionExpression)sum2.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.SingleColumn);
            Assert.AreEqual(count * (count - 1), sum2.Eval());

            var sum3 = model.Child.Child.SingleColumn.Sum();
            ((DbFunctionExpression)sum3.DbExpression).Verify(FunctionKeys.Sum, dataSet._.Child.Child.SingleColumn);
            Assert.AreEqual(count * count * (count - 1), sum3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(count - 1, sum2[dataRow]);
                var childDataSet = dataRow.Children(model.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(count - 1, sum3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Sum_Single_Converter()
        {
            var column = GetDataSet(3, RowValueType.Sum)._.SingleColumn.Sum();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_Sum_Single, json);

            var fromJsonColumn = (_Single)Column.FromJson(GetDataSet(5, RowValueType.Sum)._, json);
            Assert.AreEqual(4, fromJsonColumn.Eval());
        }

        [TestMethod]
        public void Functions_Min()
        {
            int count = 3;

            var dataSet = GetDataSet(count);
            var model = dataSet._;

            var min1 = model.Id.Min();
            ((DbFunctionExpression)min1.DbExpression).Verify(FunctionKeys.Min, dataSet._.Id);
            Assert.AreEqual(0, min1.Eval());

            var min2 = model.Child.Id.Min();
            ((DbFunctionExpression)min2.DbExpression).Verify(FunctionKeys.Min, dataSet._.Child.Id);
            Assert.AreEqual(0, min2.Eval());

            var min3 = model.Child.Child.Id.Min();
            ((DbFunctionExpression)min3.DbExpression).Verify(FunctionKeys.Min, dataSet._.Child.Child.Id);
            Assert.AreEqual(0, min3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(i * count, min2[dataRow]);
                var childDataSet = dataRow.Children(model.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(i * count * count + j * count, min3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Min_Converter()
        {
            var column = GetDataSet(3)._.Id.Min();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_Min, json);

            var fromJsonColumn = (_Int32)Column.FromJson(GetDataSet(3)._, json);
            Assert.AreEqual(0, fromJsonColumn.Eval());
        }

        [TestMethod]
        public void Functions_Min_String()
        {
            int count = 3;

            var dataSet = GetDataSet(count);
            var model = dataSet._;

            var min1 = model.StringColumn.Min();
            ((DbFunctionExpression)min1.DbExpression).Verify(FunctionKeys.Min, dataSet._.StringColumn);
            Assert.AreEqual(((int)0).ToString(STR_PADDING), min1.Eval());

            var min2 = model.Child.StringColumn.Min();
            ((DbFunctionExpression)min2.DbExpression).Verify(FunctionKeys.Min, dataSet._.Child.StringColumn);
            Assert.AreEqual(((int)0).ToString(STR_PADDING), min2.Eval());

            var min3 = model.Child.Child.StringColumn.Min();
            ((DbFunctionExpression)min3.DbExpression).Verify(FunctionKeys.Min, dataSet._.Child.Child.StringColumn);
            Assert.AreEqual(((int)0).ToString(STR_PADDING), min3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual((i * count).ToString(STR_PADDING), min2[dataRow]);
                var childDataSet = dataRow.Children(model.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual((i * count * count + j * count).ToString(STR_PADDING), min3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Min_String_Converter()
        {
            var column = GetDataSet(3)._.StringColumn.Min();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_Min_String, json);

            var fromJsonColumn = (_String)Column.FromJson(GetDataSet(3)._, json);
            Assert.AreEqual("0000", fromJsonColumn.Eval());
        }

        [TestMethod]
        public void Functions_Max()
        {
            int count = 3;

            var dataSet = GetDataSet(count);
            var model = dataSet._;

            var max1 = model.Id.Max();
            ((DbFunctionExpression)max1.DbExpression).Verify(FunctionKeys.Max, dataSet._.Id);
            Assert.AreEqual(count - 1, max1.Eval());

            var max2 = model.Child.Id.Max();
            ((DbFunctionExpression)max2.DbExpression).Verify(FunctionKeys.Max, dataSet._.Child.Id);
            Assert.AreEqual(count * count - 1, max2.Eval());

            var max3 = model.Child.Child.Id.Max();
            ((DbFunctionExpression)max3.DbExpression).Verify(FunctionKeys.Max, dataSet._.Child.Child.Id);
            Assert.AreEqual(count * count * count - 1, max3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual((i + 1) * count - 1, max2[dataRow]);
                var childDataSet = dataRow.Children(model.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(i * count * count + (j + 1) * count - 1, max3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Max_Converter()
        {
            var column = GetDataSet(3)._.Id.Max();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_Max, json);

            var fromJsonColumn = (_Int32)Column.FromJson(GetDataSet(3)._, json);
            Assert.AreEqual(2, fromJsonColumn.Eval());
        }

        [TestMethod]
        public void Functions_Max_String()
        {
            int count = 3;

            var dataSet = GetDataSet(count);
            var model = dataSet._;
            var max1 = model.StringColumn.Max();
            ((DbFunctionExpression)max1.DbExpression).Verify(FunctionKeys.Max, dataSet._.StringColumn);
            Assert.AreEqual((count - 1).ToString(STR_PADDING), max1.Eval());

            var max2 = model.Child.StringColumn.Max();
            ((DbFunctionExpression)max2.DbExpression).Verify(FunctionKeys.Max, dataSet._.Child.StringColumn);
            Assert.AreEqual((count * count - 1).ToString(STR_PADDING), max2.Eval());

            var max3 = model.Child.Child.StringColumn.Max();
            ((DbFunctionExpression)max3.DbExpression).Verify(FunctionKeys.Max, dataSet._.Child.Child.StringColumn);
            Assert.AreEqual((count * count * count - 1).ToString(STR_PADDING), max3.Eval());

            for (int i = 0; i < count; i++)
            {
                var dataRow = dataSet[i];
                Assert.AreEqual(((i + 1) * count - 1).ToString(STR_PADDING), max2[dataRow]);
                var childDataSet = dataRow.Children(model.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual((i * count * count + (j + 1) * count - 1).ToString(STR_PADDING), max3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Max_String_Converter()
        {
            var column = GetDataSet(3)._.StringColumn.Max();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_Max_String, json);

            var fromJsonColumn = (_String)Column.FromJson(GetDataSet(3)._, json);
            Assert.AreEqual("0002", fromJsonColumn.Eval());
        }

        [TestMethod]
        public void Functions_Average_Decimal()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Average);
            var model = dataSet._;

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
                var childDataSet = dataRow.Children(model.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(2, avg3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Average_Decimal_Converter()
        {
            var column = GetDataSet(3, RowValueType.Average)._.DecimalColumn.Average();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_Average_Decimal, json);

            var fromJsonColumn = (_Decimal)Column.FromJson(GetDataSet(3, RowValueType.Average)._, json);
            Assert.AreEqual(2, fromJsonColumn.Eval());
        }

        [TestMethod]
        public void Functions_Average_Double()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Average);
            var model = dataSet._;

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
                var childDataSet = dataRow.Children(model.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(2, avg3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Average_Double_Converter()
        {
            var column = GetDataSet(3, RowValueType.Average)._.DoubleColumn.Average();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_Average_Double, json);

            var fromJsonColumn = (_Double)Column.FromJson(GetDataSet(3, RowValueType.Average)._, json);
            Assert.AreEqual(2, fromJsonColumn.Eval());
        }

        [TestMethod]
        public void Functions_Average_Int32()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Average);
            var model = dataSet._;

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
                var childDataSet = dataRow.Children(model.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(2, avg3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Average_Int32_Converter()
        {
            var column = GetDataSet(3, RowValueType.Average)._.Int32Column.Average();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_Average_Int32, json);

            var fromJsonColumn = (_Double)Column.FromJson(GetDataSet(3, RowValueType.Average)._, json);
            Assert.AreEqual(2, fromJsonColumn.Eval());
        }

        [TestMethod]
        public void Functions_Average_Int64()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Average);
            var model = dataSet._;

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
                var childDataSet = dataRow.Children(model.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(2, avg3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Average_Int64_Converter()
        {
            var column = GetDataSet(3, RowValueType.Average)._.Int64Column.Average();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_Average_Int64, json);

            var fromJsonColumn = (_Double)Column.FromJson(GetDataSet(3, RowValueType.Average)._, json);
            Assert.AreEqual(2, fromJsonColumn.Eval());
        }

        [TestMethod]
        public void Functions_Average_Single()
        {
            int count = 3;

            var dataSet = GetDataSet(count, RowValueType.Average);
            var model = dataSet._;

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
                var childDataSet = dataRow.Children(model.Child);
                for (int j = 0; j < count; j++)
                {
                    var childDataRow = childDataSet[j];
                    Assert.AreEqual(2, avg3[childDataRow]);
                }
            }
        }

        [TestMethod]
        public void Functions_Average_Single_Converter()
        {
            var column = GetDataSet(3, RowValueType.Average)._.SingleColumn.Average();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_Average_Single, json);

            var fromJsonColumn = (_Single)Column.FromJson(GetDataSet(3, RowValueType.Average)._, json);
            Assert.AreEqual(2, fromJsonColumn.Eval());
        }
    }
}
