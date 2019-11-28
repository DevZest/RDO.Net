using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.CodeAnalysis
{
    [TestClass]
    public class DataSetExchangeTests
    {
        private sealed class SimpleModel : Model
        {
            static SimpleModel()
            {
                RegisterColumn((SimpleModel _) => _.Id);
                RegisterColumn((SimpleModel _) => _.Name);
            }

            public _Int32 Id { get; private set; }
            public _String Name { get; private set; }

            public static DataSet<SimpleModel> MockDataSet(int count)
            {
                var result = DataSet<SimpleModel>.Create();
                for (int i = 0; i < count; i++)
                {
                    result.AddRow();
                    result._.Id[i] = i;
                    result._.Name[i] = string.Format("Name{0}", i);
                }
                return result;
            }
        }

        [TestMethod]
        public void DataSetExchange_GetColumns()
        {
            var dataSet = SimpleModel.MockDataSet(2);

            var columns = DataSetExchange.GetColumns(dataSet);
            var expected =
@"[
   {
      ""Type"" : ""DevZest.Data._Int32"",
      ""Name"" : ""Id""
   },
   {
      ""Type"" : ""DevZest.Data._String"",
      ""Name"" : ""Name""
   }
]";
            Assert.AreEqual(expected, columns.ToJsonString(true));
        }

        [TestMethod]
        public void DataSetExchange_GetDataValues()
        {
            var dataSet = SimpleModel.MockDataSet(2);
            var dataValuesJsonString = DataSetExchange.GetDataValues(dataSet).ToJsonString(true);
            var expected =
@"[
   {
      ""ColumnList"" : [
         0,
         ""Name0""
      ]
   },
   {
      ""ColumnList"" : [
         1,
         ""Name1""
      ]
   }
]";
            Assert.AreEqual(expected, dataValuesJsonString);

            var columnsJsonString = DataSetExchange.GetColumns(dataSet).ToJsonString(true);
            var dataValues = DataSetExchange.GetDataValues(dataValuesJsonString, DataSetExchange.GetColumns(columnsJsonString));
            Assert.AreEqual(expected, dataValues.ToJsonString(true));
        }

        [TestMethod]
        public void DataSetExchange_SetUnsupportedMessage()
        {
            var dataSet = SimpleModel.MockDataSet(2);

            var dataValuesJsonString = DataSetExchange.GetDataValues(dataSet).ToJsonString(true);
            var columnsJsonString = DataSetExchange.GetColumns(dataSet).ToJsonString(true);
            var dataValues = DataSetExchange.GetDataValues(dataValuesJsonString, DataSetExchange.GetColumns(columnsJsonString));
            var message = "Column editing is not supported.";
            dataValues._.ColumnList[1].SetUnsupportedMessage(message);
            Assert.IsFalse(dataValues._.ColumnList[1].IsSupported());
            Assert.AreEqual(message, dataValues._.ColumnList[1].GetUnsupportedMessage());
        }

        [TestMethod]
        public void DataSetExchange_DataValues_GetJsonString()
        {
            var dataSet = SimpleModel.MockDataSet(2);

            var dataValuesJsonString = DataSetExchange.GetDataValues(dataSet).ToJsonString(true);
            var columnsJsonString = DataSetExchange.GetColumns(dataSet).ToJsonString(true);
            var dataValues = DataSetExchange.GetDataValues(dataValuesJsonString, DataSetExchange.GetColumns(columnsJsonString));
            var message = "Column editing is not supported.";
            dataValues._.ColumnList[1].SetUnsupportedMessage(message);
            var expected =
@"[
   {
      ""ColumnList"" : [
         0
      ]
   },
   {
      ""ColumnList"" : [
         1
      ]
   }
]";
            Assert.AreEqual(expected, dataValues.GetJsonString());
        }

        [TestMethod]
        public void DataSetExchange_DataValues_GetColumnFlags()
        {
            var dataSet = SimpleModel.MockDataSet(2);

            var dataValuesJsonString = DataSetExchange.GetDataValues(dataSet).ToJsonString(true);
            var columnsJsonString = DataSetExchange.GetColumns(dataSet).ToJsonString(true);
            var dataValues = DataSetExchange.GetDataValues(dataValuesJsonString, DataSetExchange.GetColumns(columnsJsonString));
            var message = "Column editing is not supported.";
            dataValues._.ColumnList[1].SetUnsupportedMessage(message);
            var expected =
@"[
   {
      ""IsVisible"" : true
   },
   {
      ""IsVisible"" : false
   }
]";
            Assert.AreEqual(expected, dataValues.GetColumnFlags().ToJsonString(true));
        }

        [TestMethod]
        public void DataSetExchange_PreviewJson_strongly_typed()
        {
            var columnFlagsJson =
@"[
   {
      ""IsVisible"" : true
   },
   {
      ""IsVisible"" : false
   }
]";

            var dataValuesJson =
@"[
   {
      ""ColumnList"" : [
         0
      ]
   },
   {
      ""ColumnList"" : [
         1
      ]
   }
]";

            var result = DataSetExchange._PreviewJson<SimpleModel>(columnFlagsJson, dataValuesJson);
            var expected =
@"[
   {
      ""Id"" : 0
   },
   {
      ""Id"" : 1
   }
]";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void DataSetExchange_PreviewJson()
        {
            var columnFlagsJson =
@"[
   {
      ""IsVisible"" : true
   },
   {
      ""IsVisible"" : false
   }
]";

            var dataValuesJson =
@"[
   {
      ""ColumnList"" : [
         0
      ]
   },
   {
      ""ColumnList"" : [
         1
      ]
   }
]";

            var result = DataSetExchange.PreviewJson(typeof(SimpleModel).FullName, columnFlagsJson, dataValuesJson);
            var expected =
@"[
   {
      ""Id"" : 0
   },
   {
      ""Id"" : 1
   }
]";
            Assert.AreEqual(expected, result);
        }
    }
}
