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

        private sealed class ScalarPresenter : DataPresenter<DummyModel>
        {
            Scalar<string> _emailAddress;
            Scalar<string> _password;

            public ScalarPresenter()
            {
                _emailAddress = NewScalar<string>().AddValidator(ValidateEmailAddressRequired).AddValidator(ValidateEmailAddressMaxLength);
                _password = NewScalar<string>().AddValidator(ValidatePasswordRequired).AddValidator(ValidatePasswordLength);
            }

            private static string ValidateEmailAddressRequired(string value)
            {
                return string.IsNullOrEmpty(value) ? "Field 'Email Address' is required." : null;

            }

            private static string ValidateEmailAddressMaxLength(string value)
            {
                return value != null && value.Length > 20 ? "Field 'Email Address' must be a string with maximumn length of 20." : null;
            }

            private static string ValidatePasswordRequired(string value)
            {
                return string.IsNullOrEmpty(value) ? "Field 'Password' is required." : null;
            }

            private static string ValidatePasswordLength(string value)
            {
                return value != null && (value.Length < 6 || value.Length > 20) ? "Field 'Password' must be a string with minimum length of 6 and maximumn length of 20." : null;
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var emailAddress = _emailAddress.BindToTextBox();
                var password = _password.BindToPasswordBox();
                builder
                    .WithRowValidationMode(ValidationMode.Implicit)
                        .GridColumns("Auto", "*", "20")
                        .GridRows("Auto", "Auto")
                        .AddBinding(0, 0, @"Email Address:".BindToLabel(emailAddress)).AddBinding(1, 0, emailAddress).AddBinding(2, 0, emailAddress.Input.BindToValidityIndicator())
                        .AddBinding(0, 1, @"Password:".BindToLabel(password)).AddBinding(1, 1, password).AddBinding(2, 1, password.Input.BindToValidityIndicator());
            }
        }
    }
}
