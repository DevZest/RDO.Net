using DevZest.Data.Presenters;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ValidationUI
{
    internal abstract class _LoginPresenter : SimplePresenter
    {
        protected Scalar<string> _emailAddress { get; private set; }
        protected Scalar<string> _password { get; private set; }

        protected _LoginPresenter()
        {
            _emailAddress = NewScalar(string.Empty).AddValidator(ValidateEmailAddress).AddValidator(ValidateEmailAddressMaxLength);
            _password = NewScalar(string.Empty).AddValidator(ValidatePasswordLength);
        }

        private static string ValidateEmailAddress(string value)
        {
            return !value.IsValidEmailAddress() ? "Field 'Email Address' is not a valid email address." : null;
        }

        private static string ValidateEmailAddressMaxLength(string value)
        {
            return value != null && value.Length > 20 ? "Field 'Email Address' must be a string with maximumn length of 20." : null;
        }

        private static string ValidatePasswordLength(string value)
        {
            return value != null && (value.Length < 6 || value.Length > 20) ? "Field 'Password' must be a string with minimum length of 6 and maximumn length of 20." : null;
        }
    }
}
