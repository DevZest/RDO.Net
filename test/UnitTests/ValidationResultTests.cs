using DevZest.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class ValidationResultTests : SimpleModelDataSetHelper
    {
        [TestMethod]
        public void ValidationResult_ToJsonString()
        {
            {
                var result = DataRowValidationResults.Empty;
                Assert.AreEqual("[]", result.ToJsonString(true));
            }

            {
                var dataSet = GetDataSet(3);
                var messageId = "MessageId";
                var result = DataRowValidationResults.Empty
                    .Add(new DataRowValidationResult(dataSet[0], new ColumnValidationMessage(messageId, ValidationSeverity.Error, "This is an error message", dataSet._.Id)))
                    .Add(new DataRowValidationResult(dataSet[1], new ColumnValidationMessage(messageId, ValidationSeverity.Warning, "This is a warning message", dataSet._.Id)))
                    .Add(new DataRowValidationResult(dataSet[2], new ColumnValidationMessage(messageId, ValidationSeverity.Warning, "This is a warning message", dataSet._.Id)));
                var expectedJson =
@"[
   {
      ""DataRow"" : ""/[0]"",
      ""Messages"" : [
         {
            ""Id"" : ""MessageId"",
            ""Severity"" : ""Error"",
            ""Description"" : ""This is an error message"",
            ""Source"" : ""Id""
         }
      ]
   },
   {
      ""DataRow"" : ""/[1]"",
      ""Messages"" : [
         {
            ""Id"" : ""MessageId"",
            ""Severity"" : ""Warning"",
            ""Description"" : ""This is a warning message"",
            ""Source"" : ""Id""
         }
      ]
   },
   {
      ""DataRow"" : ""/[2]"",
      ""Messages"" : [
         {
            ""Id"" : ""MessageId"",
            ""Severity"" : ""Warning"",
            ""Description"" : ""This is a warning message"",
            ""Source"" : ""Id""
         }
      ]
   }
]";
                Assert.AreEqual(expectedJson, result.ToJsonString(true));
            }
        }

        [TestMethod]
        public void ValidationResult_ParseJson()
        {
            {
                var dataSet = GetDataSet(3);
                var results = DataRowValidationResults.ParseJson(dataSet, "[]");
                Assert.AreEqual(results, DataRowValidationResults.Empty);
            }

            {
                var dataSet = GetDataSet(3);
                var json =
@"[
   {
      ""DataRow"" : ""/[0]"",
      ""Messages"" : [
         {
            ""Id"" : ""MessageId"",
            ""Severity"" : ""Error"",
            ""Description"" : ""This is an error message"",
            ""Source"" : ""Id""
         }
      ]
   },
   {
      ""DataRow"" : ""/[1]"",
      ""Messages"" : [
         {
            ""Id"" : ""MessageId"",
            ""Severity"" : ""Warning"",
            ""Description"" : ""This is a warning message"",
            ""Source"" : ""Id""
         }
      ]
   },
   {
      ""DataRow"" : ""/[2]"",
      ""Messages"" : [
         {
            ""Id"" : ""MessageId"",
            ""Severity"" : ""Warning"",
            ""Description"" : ""This is a warning message"",
            ""Source"" : ""Id""
         }
      ]
   }
]";
                var result = DataRowValidationResults.ParseJson(dataSet, json);
                Assert.AreEqual(3, result.Count);
            }
        }
    }
}
