using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.Presenters;

namespace ValidationUI
{
    public class NewUser : User
    {
        static NewUser()
        {
            RegisterColumn((NewUser _) => _.ConfirmPassword);
        }

        [Required]
        [StringLength(20, MinimumLength = 6)]
        [Display(Name = "Confirm Password")]
        [DefaultValue("")]
        public _String ConfirmPassword { get; private set; }

        private IColumns _passwordMismatchErrorSource;
        public IColumns PasswordMismatchErrorSource
        {
            get { return _passwordMismatchErrorSource ?? (_passwordMismatchErrorSource = Password.Union(ConfirmPassword).Seal()); }
        }

        private const string ERR_PASSWORD_MISMATCH = "Password and Confirm Password must be identical";

        [ModelValidator]
        private DataValidationError ValidatePassword(DataRow dataRow)
        {
            return ConfirmPassword[dataRow] == Password[dataRow] ? null : new DataValidationError(ERR_PASSWORD_MISMATCH, PasswordMismatchErrorSource);
        }
    }
}
