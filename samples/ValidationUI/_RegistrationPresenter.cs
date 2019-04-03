using DevZest.Data.Presenters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ValidationUI
{
    internal abstract class _RegistrationPresenter : _LoginPresenter
    {
        protected Scalar<string> _userName { get; private set; }
        protected Scalar<string> _passwordConfirmation { get; private set; }
        protected Scalar<Interests> _interests { get; private set; }

        protected _RegistrationPresenter()
        {
            _userName = NewScalar(string.Empty).AddValidator(ValidateUserNameRequired);
            _passwordConfirmation = NewScalar(string.Empty).AddValidator(ValidatePasswordConfirmationLength);
            _interests = NewScalar(Interests.None).AddValidator(ValidateInterests);
        }

        private static string ValidateUserNameRequired(string value)
        {
            return string.IsNullOrEmpty(value) ? "Field 'User Name' is required." : null;
        }

        private static string ValidatePasswordConfirmationLength(string value)
        {
            return value != null && (value.Length < 6 || value.Length > 20) ? "Field 'Password Confirmation' must be a string with minimum length of 6 and maximumn length of 20." : null;
        }

        private static string ValidateInterests(Interests value)
        {
            var count = 0;
            while (value != 0)
            {
                value = value & (value - 1);
                count++;
            }
            return count < 3 ? "At least 3 interests must be selected." : null;
        }

        protected override IScalarValidationErrors ValidateScalars(IScalarValidationErrors result)
        {
            if (_password.GetValue() != _passwordConfirmation.GetValue())
                result = result.Add(new ScalarValidationError("Passwords do not match.", _password.Union(_passwordConfirmation).Seal()));
            return result.Seal();
        }

        protected Func<Task<string>> ValidateUserNameFunc
        {
            get { return ValidateUserName; }
        }

        private Task<string> ValidateUserName()
        {
            return RegistrationWindow.PerformValidateUserName(_userName.GetValue());
        }
    }
}
