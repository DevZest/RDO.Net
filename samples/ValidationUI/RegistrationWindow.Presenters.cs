using DevZest.Data;
using DevZest.Data.Presenters;

namespace ValidationUI
{
    partial class RegistrationWindow
    {
        private sealed class DefaultPresenter : DataPresenter<Registration>
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
                    .AddBinding(0, 0, _.EmailAddress.BindToLabel(emailAddress, "{0}:"))
                    .AddBinding(0, 1, _.Password.BindToLabel(password, "{0}:"))
                    .AddBinding(0, 2, _.ConfirmPassword.BindToLabel(confirmPassword, "{0}:"))
                    .AddBinding(1, 0, emailAddress)
                    .AddBinding(1, 1, password)
                    .AddBinding(1, 2, confirmPassword);
            }
        }

        private sealed class VerbosePresenter : DataPresenter<Registration>
        {
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var emailAddress = _.EmailAddress.BindToTextBox();
                var password = _.Password.BindToPasswordBox();
                var confirmPassword = _.ConfirmPassword.BindToPasswordBox();
                var passwordMismatch = new RowBinding[] { password, confirmPassword }.BindToValidationPlaceholder();
                builder
                    .GridColumns("Auto", "*")
                    .GridRows("Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto", "Auto")
                    .AddBinding(1, 2, 1, 5, passwordMismatch)
                    .AddBinding(1, 6, passwordMismatch.Input.BindToValidationErrorsControl().WithAutoSizeOrder(1))
                    .AddBinding(0, 0, _.EmailAddress.BindToLabel(emailAddress, "{0}:"))
                    .AddBinding(0, 2, _.Password.BindToLabel(password, "{0}:"))
                    .AddBinding(0, 4, _.ConfirmPassword.BindToLabel(confirmPassword, "{0}:"))
                    .AddBinding(1, 0, emailAddress).AddBinding(1, 1, emailAddress.Input.BindToValidationErrorsControl())
                    .AddBinding(1, 2, password).AddBinding(1, 3, password.Input.BindToValidationErrorsControl())
                    .AddBinding(1, 4, confirmPassword).AddBinding(1, 5, confirmPassword.Input.BindToValidationErrorsControl());
            }
        }
    }
}
