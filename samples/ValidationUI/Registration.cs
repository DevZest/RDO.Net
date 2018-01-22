using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.Presenters;

namespace ValidationUI
{
    public class Registration : Login
    {
        static Registration()
        {
            RegisterColumn((Registration _) => _.PasswordConfirmation);
        }

        [Required]
        [StringLength(20, MinimumLength = 6)]
        [Display(Name = "Confirm Password")]
        [DefaultValue("")]
        public _String PasswordConfirmation { get; private set; }

        private IColumns _passwordMismatchErrorSource;
        public IColumns PasswordMismatchErrorSource
        {
            get { return _passwordMismatchErrorSource ?? (_passwordMismatchErrorSource = Password.Union(PasswordConfirmation).Seal()); }
        }

        private const string ERR_PASSWORD_MISMATCH = "Passwords do not match";

        [ModelValidator]
        private DataValidationError ValidatePasswordConfirmation(DataRow dataRow)
        {
            return PasswordConfirmation[dataRow] == Password[dataRow] ? null : new DataValidationError(ERR_PASSWORD_MISMATCH, PasswordMismatchErrorSource);
        }
    }
}
