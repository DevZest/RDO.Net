using DevZest.Data;
using DevZest.Data.Presenters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ValidationUI
{
    partial class RegistrationWindow
    {
        private const string LABEL_FORMAT = "{0}:";

        private static string[] s_takenUserNames = new string[] { "paul", "john", "tony" };
        private static string[] s_errorNames = new string[] { "error" };
        private static int s_retryCount = 0;

        private static async Task<string> PerformValidateUserName(string userName)
        {
            await Task.Delay(1000);
            if (s_takenUserNames.Any(x => x == userName))
                return string.Format("User name '{0}' is taken.", userName);

            if (s_errorNames.Any(x => x == userName))
            {
                s_retryCount++;
                if (s_retryCount % 2 == 1)
                    throw new InvalidOperationException("An exception is thrown");
            }

            return null;
        }

        private abstract class PresenterBase : DataPresenter<Registration>
        {
            protected Func<DataRow, Task<string>> ValidateUserNameFunc
            {
                get { return ValidateUserName; }
            }

            private Task<string> ValidateUserName(DataRow dataRow)
            {
                return PerformValidateUserName(_.UserName[dataRow]);
            }
        }

        private sealed class DefaultPresenter : PresenterBase
        {
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var userName = _.UserName.BindToTextBox();
                var emailAddress = _.EmailAddress.BindToTextBox();
                var password = _.Password.BindToPasswordBox();
                var confirmPassword = _.PasswordConfirmation.BindToPasswordBox();
                var passwordMismatch = new RowBinding[] { password, confirmPassword }.BindToValidationPlaceholder();
                var interests1 = _.Interests.BindToCheckBox(Interests.Books);
                var interests2 = _.Interests.BindToCheckBox(Interests.Comics);
                var interests3 = _.Interests.BindToCheckBox(Interests.Hunting);
                var interests4 = _.Interests.BindToCheckBox(Interests.Movies);
                var interests5 = _.Interests.BindToCheckBox(Interests.Music);
                var interests6 = _.Interests.BindToCheckBox(Interests.Physics);
                var interests7 = _.Interests.BindToCheckBox(Interests.Shopping);
                var interests8 = _.Interests.BindToCheckBox(Interests.Sports);
                var interestsValidation = new RowBinding[] { interests1, interests2, interests3, interests4, interests5, interests6, interests7, interests8 }.BindToValidationPlaceholder();
                builder
                    .GridColumns("Auto", "*", "*")
                    .GridRows("Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto")
                    .AddBinding(1, 2, 2, 3, passwordMismatch)
                    .AddBinding(1, 4, 2, 7, interestsValidation)
                    .AddBinding(0, 0, _.UserName.BindToLabel(userName, LABEL_FORMAT))
                    .AddBinding(0, 1, _.EmailAddress.BindToLabel(emailAddress, LABEL_FORMAT))
                    .AddBinding(0, 2, _.Password.BindToLabel(password, LABEL_FORMAT))
                    .AddBinding(0, 3, _.PasswordConfirmation.BindToLabel(confirmPassword, LABEL_FORMAT))
                    .AddBinding(0, 4, 0, 7, _.Interests.BindToLabel(interests1, LABEL_FORMAT))
                    .AddBinding(1, 0, 2, 0, userName)
                    .AddBinding(1, 1, 2, 1, emailAddress)
                    .AddBinding(1, 2, 2, 2, password)
                    .AddBinding(1, 3, 2, 3, confirmPassword)
                    .AddBinding(1, 4, interests1)
                    .AddBinding(2, 4, interests2)
                    .AddBinding(1, 5, interests3)
                    .AddBinding(2, 5, interests4)
                    .AddBinding(1, 6, interests5)
                    .AddBinding(2, 6, interests6)
                    .AddBinding(1, 7, interests7)
                    .AddBinding(2, 7, interests8)
                    .AddBinding(0, 8, 2, 8, _.BindToValidationErrorsControl().WithAutoSizeWaiver(AutoSizeWaiver.Width))
                    .AddAsyncValidator(userName.Input, ValidateUserNameFunc);
            }
        }

        private sealed class VerbosePresenter : PresenterBase
        {
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var userName = _.UserName.BindToTextBox();
                var emailAddress = _.EmailAddress.BindToTextBox();
                var password = _.Password.BindToPasswordBox();
                var confirmPassword = _.PasswordConfirmation.BindToPasswordBox();
                var passwordMismatch = new RowBinding[] { password, confirmPassword }.BindToValidationPlaceholder();
                var interests1 = _.Interests.BindToCheckBox(Interests.Books);
                var interests2 = _.Interests.BindToCheckBox(Interests.Comics);
                var interests3 = _.Interests.BindToCheckBox(Interests.Hunting);
                var interests4 = _.Interests.BindToCheckBox(Interests.Movies);
                var interests5 = _.Interests.BindToCheckBox(Interests.Music);
                var interests6 = _.Interests.BindToCheckBox(Interests.Physics);
                var interests7 = _.Interests.BindToCheckBox(Interests.Shopping);
                var interests8 = _.Interests.BindToCheckBox(Interests.Sports);
                var interestsValidation = new RowBinding[] { interests1, interests2, interests3, interests4, interests5, interests6, interests7, interests8 }.BindToValidationPlaceholder();
                builder
                    .GridColumns("Auto", "*", "*")
                    .GridRows("Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto")
                    .AddBinding(1, 4, 2, 7, passwordMismatch)
                    .AddBinding(1, 9, 2, 12, interestsValidation)
                    .AddBinding(0, 0, _.UserName.BindToLabel(userName, LABEL_FORMAT))
                    .AddBinding(0, 2, _.EmailAddress.BindToLabel(emailAddress, LABEL_FORMAT))
                    .AddBinding(0, 4, _.Password.BindToLabel(password, LABEL_FORMAT))
                    .AddBinding(0, 6, _.PasswordConfirmation.BindToLabel(confirmPassword, LABEL_FORMAT))
                    .AddBinding(0, 9, 0, 12, _.Interests.BindToLabel(interests1, LABEL_FORMAT))
                    .AddBinding(1, 0, 2, 0, userName)
                    .AddBinding(1, 2, 2, 2, emailAddress)
                    .AddBinding(1, 4, 2, 4, password)
                    .AddBinding(1, 6, 2, 6, confirmPassword)
                    .AddBinding(1, 9, interests1)
                    .AddBinding(2, 9, interests2)
                    .AddBinding(1, 10, interests3)
                    .AddBinding(2, 10, interests4)
                    .AddBinding(1, 11, interests5)
                    .AddBinding(2, 11, interests6)
                    .AddBinding(1, 12, interests7)
                    .AddBinding(2, 12, interests8)
                    .AddBinding(1, 1, 2, 1, userName.Input.BindToValidationErrorsControl())
                    .AddBinding(1, 3, 2, 3, emailAddress.Input.BindToValidationErrorsControl())
                    .AddBinding(1, 5, 2, 5, password.Input.BindToValidationErrorsControl())
                    .AddBinding(1, 7, 2, 7, confirmPassword.Input.BindToValidationErrorsControl())
                    .AddBinding(1, 8, 2, 8, passwordMismatch.Input.BindToValidationErrorsControl())
                    .AddBinding(1, 13, 2, 13, interestsValidation.Input.BindToValidationErrorsControl())
                    .AddAsyncValidator(userName.Input, ValidateUserNameFunc);
            }
        }

        private abstract class ScalarPresenter : ScalarPresenterBase
        {
            protected Scalar<string> _userName { get; private set; }
            protected Scalar<string> _passwordConfirmation { get; private set; }
            protected Scalar<Interests> _interests { get; private set; }

            protected ScalarPresenter()
            {
                _userName = NewScalar(string.Empty).AddValidator(ValidateUserNameRequired);
                _passwordConfirmation = NewScalar(string.Empty).AddValidator(ValidatePasswordConfirmationLength);
                _interests = NewScalar(Interests.None).AddValidator(ValidateInterests);
            }

            private static string ValidateUserNameRequired(string value)
            {
                return string.IsNullOrEmpty(value) ? "Field 'User Name' is required." : null;
            }

            private static string ValidatePasswordConfirmationLength(string value)
            {
                return value != null && (value.Length < 6 || value.Length > 20) ? "Field 'Password Confirmation' must be a string with minimum length of 6 and maximumn length of 20." : null;
            }

            private static string ValidateInterests(Interests value)
            {
                var count = 0;
                while (value != 0)
                {
                    value = value & (value - 1);
                    count++;
                }
                return count < 3 ? "At least 3 interests must be selected." : null;
            }

            protected override IScalarValidationErrors ValidateScalars(IScalarValidationErrors result)
            {
                if (_password.Value != _passwordConfirmation.Value)
                    result = result.Add(new ScalarValidationError("Passwords do not match.", _password.Union(_passwordConfirmation).Seal()));
                return result.Seal();
            }
        }

        private sealed class DefaultScalarPresenter : ScalarPresenter
        {
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var userName = _userName.BindToTextBox();
                var emailAddress = _emailAddress.BindToTextBox();
                var password = _password.BindToPasswordBox();
                var confirmPassword = _passwordConfirmation.BindToPasswordBox();
                var passwordMismatch = new ScalarBinding[] { password, confirmPassword }.BindToValidationPlaceholder();
                var interests1 = _interests.BindToCheckBox(Interests.Books);
                var interests2 = _interests.BindToCheckBox(Interests.Comics);
                var interests3 = _interests.BindToCheckBox(Interests.Hunting);
                var interests4 = _interests.BindToCheckBox(Interests.Movies);
                var interests5 = _interests.BindToCheckBox(Interests.Music);
                var interests6 = _interests.BindToCheckBox(Interests.Physics);
                var interests7 = _interests.BindToCheckBox(Interests.Shopping);
                var interests8 = _interests.BindToCheckBox(Interests.Sports);
                var interestsValidation = new ScalarBinding[] { interests1, interests2, interests3, interests4, interests5, interests6, interests7, interests8 }.BindToValidationPlaceholder();
                builder
                    .GridColumns("Auto", "*", "*")
                    .GridRows("Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto")
                    .RowRange(0, 9, 2, 9)
                    .AddBinding(1, 2, 2, 3, passwordMismatch)
                    .AddBinding(1, 4, 2, 7, interestsValidation)
                    .AddBinding(0, 0, "User Name:".BindToLabel(userName))
                    .AddBinding(0, 1, "Email Address:".BindToLabel(emailAddress))
                    .AddBinding(0, 2, "Password:".BindToLabel(password))
                    .AddBinding(0, 3, "Confirm Password:".BindToLabel(confirmPassword))
                    .AddBinding(0, 4, 0, 7, "Interests:".BindToLabel(interests1))
                    .AddBinding(1, 0, 2, 0, userName)
                    .AddBinding(1, 1, 2, 1, emailAddress)
                    .AddBinding(1, 2, 2, 2, password)
                    .AddBinding(1, 3, 2, 3, confirmPassword)
                    .AddBinding(1, 4, interests1)
                    .AddBinding(2, 4, interests2)
                    .AddBinding(1, 5, interests3)
                    .AddBinding(2, 5, interests4)
                    .AddBinding(1, 6, interests5)
                    .AddBinding(2, 6, interests6)
                    .AddBinding(1, 7, interests7)
                    .AddBinding(2, 7, interests8)
                    .AddBinding(0, 8, 2, 8, this.BindToValidationErrorsControl().WithAutoSizeWaiver(AutoSizeWaiver.Width));
            }
        }

        private sealed class VerboseScalarPresenter : ScalarPresenter
        {
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var userName = _userName.BindToTextBox();
                var emailAddress = _emailAddress.BindToTextBox();
                var password = _password.BindToPasswordBox();
                var confirmPassword = _passwordConfirmation.BindToPasswordBox();
                var passwordMismatch = new ScalarBinding[] { password, confirmPassword }.BindToValidationPlaceholder();
                var interests1 = _interests.BindToCheckBox(Interests.Books);
                var interests2 = _interests.BindToCheckBox(Interests.Comics);
                var interests3 = _interests.BindToCheckBox(Interests.Hunting);
                var interests4 = _interests.BindToCheckBox(Interests.Movies);
                var interests5 = _interests.BindToCheckBox(Interests.Music);
                var interests6 = _interests.BindToCheckBox(Interests.Physics);
                var interests7 = _interests.BindToCheckBox(Interests.Shopping);
                var interests8 = _interests.BindToCheckBox(Interests.Sports);
                var interestsValidation = new ScalarBinding[] { interests1, interests2, interests3, interests4, interests5, interests6, interests7, interests8 }.BindToValidationPlaceholder();
                builder
                    .GridColumns("Auto", "*", "*")
                    .GridRows("Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto")
                    .RowRange(0, 14, 2, 14)
                    .AddBinding(1, 4, 2, 7, passwordMismatch)
                    .AddBinding(1, 9, 2, 12, interestsValidation)
                    .AddBinding(0, 0, "User Name:".BindToLabel(userName))
                    .AddBinding(0, 2, "Email Address:".BindToLabel(emailAddress))
                    .AddBinding(0, 4, "Password:".BindToLabel(password))
                    .AddBinding(0, 6, "Confirm Password:".BindToLabel(confirmPassword))
                    .AddBinding(0, 9, 0, 12, "Interests:".BindToLabel(interests1))
                    .AddBinding(1, 0, 2, 0, userName)
                    .AddBinding(1, 2, 2, 2, emailAddress)
                    .AddBinding(1, 4, 2, 4, password)
                    .AddBinding(1, 6, 2, 6, confirmPassword)
                    .AddBinding(1, 9, interests1)
                    .AddBinding(2, 9, interests2)
                    .AddBinding(1, 10, interests3)
                    .AddBinding(2, 10, interests4)
                    .AddBinding(1, 11, interests5)
                    .AddBinding(2, 11, interests6)
                    .AddBinding(1, 12, interests7)
                    .AddBinding(2, 12, interests8)
                    .AddBinding(1, 1, 2, 1, userName.Input.BindToValidationErrorsControl())
                    .AddBinding(1, 3, 2, 3, emailAddress.Input.BindToValidationErrorsControl())
                    .AddBinding(1, 5, 2, 5, password.Input.BindToValidationErrorsControl())
                    .AddBinding(1, 7, 2, 7, confirmPassword.Input.BindToValidationErrorsControl())
                    .AddBinding(1, 8, 2, 8, passwordMismatch.Input.BindToValidationErrorsControl())
                    .AddBinding(1, 13, 2, 13, interestsValidation.Input.BindToValidationErrorsControl());
            }
        }
    }
}
