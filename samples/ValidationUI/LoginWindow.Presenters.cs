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
                    .AddBinding(0, 0, _.EmailAddress.BindToLabel(emailAddress, "{0}:")).AddBinding(1, 0, emailAddress)
                    .AddBinding(0, 1, _.Password.BindToLabel(password, "{0}:")).AddBinding(1, 1, password);
            }
        }

        private sealed class ScalarPresenter : ScalarPresenterBase
        {
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var emailAddress = _emailAddress.BindToTextBox();
                var password = _password.BindToPasswordBox();
                builder
                    .WithScalarValidationMode(ValidationMode.Implicit)
                        .GridColumns("Auto", "*", "20")
                        .GridRows("Auto", "Auto", "Auto")
                        .RowRange(0, 2, 2, 2)
                        .AddBinding(0, 0, @"Email Address:".BindToLabel(emailAddress)).AddBinding(1, 0, emailAddress)
                        .AddBinding(0, 1, @"Password:".BindToLabel(password)).AddBinding(1, 1, password);
            }
        }
    }
}
