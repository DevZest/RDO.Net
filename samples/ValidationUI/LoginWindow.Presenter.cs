using DevZest.Data.Presenters;

namespace ValidationUI
{
    partial class LoginWindow
    {
        private sealed class Presenter : DataPresenter<Login>
        {
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var emailAddress = _.EmailAddress.BindToTextBox();
                var password = _.Password.BindToPasswordBox();
                builder
                    .WithRowValidationMode(ValidationMode.Implicit)
                    .GridColumns("Auto", "*", "20")
                    .GridRows("Auto", "Auto")
                    .AddBinding(0, 0, _.EmailAddress.BindToLabel(emailAddress, "{0}:")).AddBinding(1, 0, emailAddress).AddBinding(2, 0, emailAddress.Input.BindToValidityIndicator())
                    .AddBinding(0, 1, _.Password.BindToLabel(password, "{0}:")).AddBinding(1, 1, password).AddBinding(2, 1, password.Input.BindToValidityIndicator());
            }
        }
    }
}
