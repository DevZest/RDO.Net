using DevZest.Data;
using DevZest.Data.Presenters;

namespace ValidationUI
{
    partial class RegistrationWindow
    {
        private const string LABEL_FORMAT = "{0}:";

        private sealed class DefaultPresenter : DataPresenter<Registration>
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
                    .AddBinding(0, 8, 2, 8, _.BindToValidationErrorsControl().WithAutoSizeWaiver(AutoSizeWaiver.Width));
            }
        }

        private sealed class VerbosePresenter : DataPresenter<Registration>
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
                    .AddBinding(1, 1, 2, 1, userName.Input.BindToValidationErrorsControl().WithAutoSizeOrder(1))
                    .AddBinding(1, 3, 2, 3, emailAddress.Input.BindToValidationErrorsControl().WithAutoSizeOrder(1))
                    .AddBinding(1, 5, 2, 5, password.Input.BindToValidationErrorsControl().WithAutoSizeOrder(1))
                    .AddBinding(1, 7, 2, 7, confirmPassword.Input.BindToValidationErrorsControl().WithAutoSizeOrder(1))
                    .AddBinding(1, 8, 2, 8, passwordMismatch.Input.BindToValidationErrorsControl().WithAutoSizeOrder(1))
                    .AddBinding(1, 13, 2, 13, interestsValidation.Input.BindToValidationErrorsControl().WithAutoSizeOrder(1))

                    ;
            }
        }
    }
}
