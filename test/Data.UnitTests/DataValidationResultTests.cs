using DevZest.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class DataValidationResultTests : SimpleModelDataSetHelper
    {
        [TestMethod]
        public void DataValidationResult_ToJsonString()
        {
            {
                var result = DataValidationResults.Empty;
                Assert.AreEqual("[]", result.ToJsonString(true));
            }

            {
                var dataSet = GetDataSet(2);
                var result = DataValidationResults.Empty
                    .Add(new DataValidationResult(dataSet[0], new DataValidationError("This is an error message", dataSet._.Id)))
                    .Add(new DataValidationResult(dataSet[1], new DataValidationError("This is another error message", dataSet._.Id)));
                var expectedJson =
@"[
   {
      ""DataRow"" : ""/[0]"",
      ""Errors"" : [
         {
            ""Message"" : ""This is an error message"",
            ""Source"" : ""Id""
         }
      ]
   },
   {
      ""DataRow"" : ""/[1]"",
      ""Errors"" : [
         {
            ""Message"" : ""This is another error message"",
            ""Source"" : ""Id""
         }
      ]
   }
]";
                Assert.AreEqual(expectedJson, result.ToJsonString(true));
            }
        }

        [TestMethod]
        public void DataValidationResult_ParseJson()
        {
            {
                var dataSet = GetDataSet(3);
                var results = DataValidationResults.ParseJson(dataSet, "[]");
                Assert.AreEqual(results, DataValidationResults.Empty);
            }

            {
                var dataSet = GetDataSet(3);
                var json =
@"[
   {
      ""DataRow"" : ""/[0]"",
      ""Errors"" : [
         {
            ""Message"" : ""This is an error message"",
            ""Source"" : ""Id""
         }
      ]
   },
   {
      ""DataRow"" : ""/[1]"",
      ""Errors"" : [
         {
            ""Message"" : ""This is another error message"",
            ""Source"" : ""Id""
         }
      ]
   },
   {
      ""DataRow"" : ""/[2]"",
      ""Errors"" : [
         {
            ""Message"" : ""This is another error message"",
            ""Source"" : ""Id""
         }
      ]
   }
]";
                var result = DataValidationResults.ParseJson(dataSet, json);
                Assert.AreEqual(3, result.Count);
            }
        }
    }
}
