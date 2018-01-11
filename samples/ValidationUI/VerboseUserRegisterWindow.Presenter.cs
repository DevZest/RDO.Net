using DevZest.Data.Presenters;

namespace ValidationUI
{
    partial class VerboseUserRegisterWindow
    {
        private sealed class Presenter : DataPresenter<NewUser>
        {
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var emailAddressBinding = _.EmailAddress.BindToTextBox();
                var passwordBinding = _.Password.BindToPasswordBox();
                var confirmPasswordBinding = _.ConfirmPassword.BindToPasswordBox();
                builder
                    .GridColumns("Auto", "*")
                    .GridRows("Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto")
                    .AddBinding(1, 2, 1, 5, _.PasswordMismatchErrorSource.BindToValidationPlaceholder())
                    .AddBinding(1, 6, _.PasswordMismatchErrorSource.BindToValidationMessagesControl().WithAutoSizeOrder(1))
                    .AddBinding(0, 0, _.EmailAddress.BindToLabel(emailAddressBinding)).AddBinding(1, 0, emailAddressBinding)
                    .AddBinding(1, 1, _.EmailAddress.BindToValidationMessagesControl().WithAutoSizeOrder(1))
                    .AddBinding(0, 2, _.Password.BindToLabel(passwordBinding)).AddBinding(1, 2, passwordBinding)
                    .AddBinding(1, 3, _.Password.BindToValidationMessagesControl().WithAutoSizeOrder(1))
                    .AddBinding(0, 4, _.ConfirmPassword.BindToLabel(confirmPasswordBinding)).AddBinding(1, 4, confirmPasswordBinding)
                    .AddBinding(1, 5, _.ConfirmPassword.BindToValidationMessagesControl().WithAutoSizeOrder(1));
            }
        }
    }
}
