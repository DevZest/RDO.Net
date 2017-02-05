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
                var result = ValidationResult.Empty;
                Assert.AreEqual("[]", result.ToJsonString(true));
            }

            {
                var dataSet = GetDataSet(3);
                var messageId = "MessageId";
                var result = ValidationResult.Empty
                    .Add(new ValidationEntry(dataSet[0], new ValidationMessage(messageId, ValidationSeverity.Error, "This is an error message", dataSet._.Id)))
                    .Add(new ValidationEntry(dataSet[1], new ValidationMessage(messageId, ValidationSeverity.Warning, "This is a warning message", dataSet._.Id)))
                    .Add(new ValidationEntry(dataSet[2], new ValidationMessage(messageId, ValidationSeverity.Warning, "This is a warning message", dataSet._.Id)));
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
                var result = ValidationResult.ParseJson(dataSet, "[]");
                Assert.AreEqual(result, ValidationResult.Empty);
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
                var result = ValidationResult.ParseJson(dataSet, json);
                Assert.AreEqual(3, result.Count);
            }
        }
    }
}
