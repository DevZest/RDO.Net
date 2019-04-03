using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class CustomValidatorAttributeTests
    {
        private const string ERR_MESSAGE = "Confirm password different from password";

        [CustomValidator(nameof(VAL_ConfirmPassword))]
        private class User : Model
        {
            static User()
            {
                RegisterColumn((User _) => _.Password);
                RegisterColumn((User _) => _.ConfirmPassword);
            }

            public _String Password { get; private set; }

            public _String ConfirmPassword { get; private set; }

            [_CustomValidator]
            private CustomValidatorEntry VAL_ConfirmPassword
            {
                get
                {
                    string Validate(DataRow dataRow)
                    {
                        return ConfirmPassword[dataRow] == Password[dataRow] ? null : ERR_MESSAGE;
                    }

                    IColumns GetSourceColumns()
                    {
                        return ConfirmPassword;
                    }

                    return new CustomValidatorEntry(Validate, GetSourceColumns);
                }
            }
        }

        [TestMethod]
        public void CustomValidatorAttribute()
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
