using DevZest.Data;
using DevZest.Data.Annotations;

namespace ValidationUI
{
    [CustomValidator(nameof(VAL_PasswordConfirmation))]
    [CustomValidator(nameof(VAL_Interests))]
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

        private const string ERR_PASSWORD_MISMATCH = "Passwords do not match";

        [_CustomValidator]
        private CustomValidatorEntry VAL_Interests
        {
            get
            {
                string Validate(DataRow dataRow)
                {
                    return IsValidInterests(Interests, dataRow) ? null : "At least 3 interests must be selected.";
                }

                IColumns GetSourceColumns()
                {
                    return Interests;
                }

                return new CustomValidatorEntry(Validate, GetSourceColumns);
            }
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
        
        [_CustomValidator]
        private CustomValidatorEntry VAL_PasswordConfirmation
        {
            get
            {
                string Validate(DataRow dataRow)
                {
                    return PasswordConfirmation[dataRow] == Password[dataRow] ? null : ERR_PASSWORD_MISMATCH;
                }

                IColumns GetSourceColumns()
                {
                    return Password.Union(PasswordConfirmation);
                }

                return new CustomValidatorEntry(Validate, GetSourceColumns);
            }
        }
    }
}
