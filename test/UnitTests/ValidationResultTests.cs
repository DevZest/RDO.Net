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
                var result = new ValidationResult();
                Assert.AreEqual("[]", result.ToJsonString(true));
            }

            {
                var result = ValidationResult.New(null);
                Assert.AreEqual("[]", result.ToJsonString(true));
            }

            {
                var result = ValidationResult.New(new ValidationEntry[0]);
                Assert.AreEqual("[]", result.ToJsonString(true));
            }

            {
                var dataSet = GetDataSet(3);
                var validatorId = new ValidatorId(this.GetType(), "ValidatorId");
                var result = ValidationResult.New(new ValidationEntry[]
                {
                    new ValidationEntry(dataSet[0], new ValidationMessage[] { new ValidationMessage(validatorId, ValidationSeverity.Error, dataSet._.Id, "This is an error message") }),
                    new ValidationEntry(dataSet[1], new ValidationMessage[] { new ValidationMessage(validatorId, ValidationSeverity.Warning,  null, "This is a warning message") })
                });
                var expectedJson =
@"[
   {
      ""DataRow"" : ""/[0]"",
      ""Messages"" : [
         {
            ""ValidatorId"" : ""DevZest.Data.ValidationResultTests.ValidatorId"",
            ""Severity"" : ""Error"",
            ""Columns"" : ""Id"",
            ""Description"" : ""This is an error message""
         }
      ]
   },
   {
      ""DataRow"" : ""/[1]"",
      ""Messages"" : [
         {
            ""ValidatorId"" : ""DevZest.Data.ValidationResultTests.ValidatorId"",
            ""Severity"" : ""Warning"",
            ""Columns"" : """",
            ""Description"" : ""This is a warning message""
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
                Assert.IsTrue(result.IsValid);
            }

            {
                var dataSet = GetDataSet(3);
                var json =
@"[
   {
      ""DataRow"" : ""/[0]"",
      ""Messages"" : [
         {
            ""ValidatorId"" : ""DevZest.Data.ValidationResultTests.ValidatorId"",
            ""Severity"" : ""Error"",
            ""Columns"" : ""Id"",
            ""Description"" : ""This is an error message""
         }
      ]
   },
   {
      ""DataRow"" : ""/[1]"",
      ""Messages"" : [
         {
            ""ValidatorId"" : ""DevZest.Data.ValidationResultTests.ValidatorId"",
            ""Severity"" : ""Warning"",
            ""Columns"" : """",
            ""Description"" : ""This is a warning message""
         }
      ]
   }
]";
                var result = ValidationResult.ParseJson(dataSet, json);
                Assert.AreEqual(2, result.Entries.Count);
            }
        }
    }
}
