using DevZest.Data.Presenters;

namespace ValidationUI
{
    partial class DefaultUserRegisterWindow
    {
        private sealed class Presenter : DataPresenter<NewUser>
        {
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var emailAddress = _.EmailAddress.BindToTextBox();
                var password = _.Password.BindToPasswordBox();
                var confirmPassword = _.ConfirmPassword.BindToPasswordBox();
                builder
                    .GridColumns("Auto", "*")
                    .GridRows("Auto", "Auto", "Auto")
                    .AddBinding(1, 1, 1, 2, new RowBinding[] { password, confirmPassword } .BindToValidationPlaceholder())
                    .AddBinding(0, 0, _.EmailAddress.BindToLabel(emailAddress, "{0}:")).AddBinding(1, 0, emailAddress)
                    .AddBinding(0, 1, _.Password.BindToLabel(password, "{0}:")).AddBinding(1, 1, password)
                    .AddBinding(0, 2, _.ConfirmPassword.BindToLabel(confirmPassword, "{0}:")).AddBinding(1, 2, confirmPassword);
            }
        }
    }
}
