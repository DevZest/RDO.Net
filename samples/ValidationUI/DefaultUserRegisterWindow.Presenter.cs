using DevZest.Data.Presenters;

namespace ValidationUI
{
    partial class DefaultUserRegisterWindow
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
                    .GridRows("Auto", "Auto", "Auto")
                    .AddBinding(1, 1, 1, 2, _.PasswordMismatchErrorSource.BindToValidationPlaceholder())
                    .AddBinding(0, 0, _.EmailAddress.BindToLabel(emailAddressBinding)).AddBinding(1, 0, emailAddressBinding)
                    .AddBinding(0, 1, _.Password.BindToLabel(passwordBinding)).AddBinding(1, 1, passwordBinding)
                    .AddBinding(0, 2, _.ConfirmPassword.BindToLabel(confirmPasswordBinding)).AddBinding(1, 2, confirmPasswordBinding);
            }
        }
    }
}
