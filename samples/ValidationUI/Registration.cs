using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.Presenters;

namespace ValidationUI
{
    [ModelValidator(nameof(ValidatePasswordConfirmation))]
    [ModelValidator(nameof(ValidateInterests))]
    public class Registration : Login
    {
        static Registration()
        {
            RegisterColumn((Registration _) => _.UserName);
            RegisterColumn((Registration _) => _.PasswordConfirmation);
            RegisterColumn((Registration _) => _.Interests);
        }

        [Required]
        [Display(Name = "User Name")]
        public _String UserName { get; private set; }

        [Required]
        [DefaultValue(typeof(Interests), nameof(ValidationUI.Interests.None))]
        public _ByteEnum<Interests> Interests { get; private set; }

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

        private DataValidationError ValidateInterests(DataRow dataRow)
        {
            return IsValidInterests(Interests, dataRow) ? null : new DataValidationError("At least 3 interests must be selected.", Interests);
        }

        private static bool IsValidInterests(_ByteEnum<Interests> column, DataRow dataRow)
        {
            var interests = column[dataRow];
            if (interests == null)
                return true;

            var value = interests.Value;
            var count = 0;
            while (value != 0)
            {
                value = value & (value - 1);
                count++;
            }
            return count >= 3;
        }

        private DataValidationError ValidatePasswordConfirmation(DataRow dataRow)
        {
            return PasswordConfirmation[dataRow] == Password[dataRow] ? null : new DataValidationError(ERR_PASSWORD_MISMATCH, PasswordMismatchErrorSource);
        }
    }
}
