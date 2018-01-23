using DevZest.Data.Presenters;

namespace ValidationUI
{
    internal abstract class ScalarPresenterBase : DataPresenter<DummyModel>
    {
        protected Scalar<string> _emailAddress;
        protected Scalar<string> _password;

        protected ScalarPresenterBase()
        {
            _emailAddress = NewScalar<string>().AddValidator(ValidateEmailAddressRequired).AddValidator(ValidateEmailAddress).AddValidator(ValidateEmailAddressMaxLength);
            _password = NewScalar<string>().AddValidator(ValidatePasswordRequired).AddValidator(ValidatePasswordLength);
        }

        private static string ValidateEmailAddressRequired(string value)
        {
            return string.IsNullOrEmpty(value) ? "Field 'Email Address' is required." : null;

        }

        private static string ValidateEmailAddress(string value)
        {
            return !value.IsValidEmailAddress() ? "Field 'Email Address' is not a valid email address." : null;
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
    }
}
