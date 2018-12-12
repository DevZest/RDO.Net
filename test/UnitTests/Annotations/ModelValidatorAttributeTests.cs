using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class ModelValidatorAttributeTests
    {
        private const string ERR_MESSAGE = "Confirm password different from password";

        [Validator(nameof(ValidateConfirmPassword))]
        private class User : Model
        {
            static User()
            {
                RegisterColumn((User _) => _.Password);
                RegisterColumn((User _) => _.ConfirmPassword);
            }

            public _String Password { get; private set; }

            public _String ConfirmPassword { get; private set; }

            private DataValidationError ValidateConfirmPassword(DataRow dataRow)
            {
                return ConfirmPassword[dataRow] == Password[dataRow] ? null : new DataValidationError(ERR_MESSAGE, ConfirmPassword);
            }
        }

        [TestMethod]
        public void ModelValidatorAttribute()
        {
            {
                var dataSet = DataSet<User>.Create();
                var dataRow = dataSet.AddRow((_, row) =>
                {
                    _.Password[row] = "password";
                    _.ConfirmPassword[row] = "password";
                });
                var messages = dataSet._.Validate(dataRow);
                Assert.AreEqual(0, messages.Count);
            }

            {
                var dataSet = DataSet<User>.Create();
                var dataRow = dataSet.AddRow((_, row) =>
                {
                    _.Password[row] = "password";
                    _.ConfirmPassword[row] = "another password";
                });
                var messages = dataSet._.Validate(dataRow);
                Assert.AreEqual(1, messages.Count);
                Assert.AreEqual(ERR_MESSAGE, messages[0].Message);
            }
        }
    }
}
