using DevZest.Data;
using DevZest.Data.Annotations;

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
        [Display(Name = "Confirm Password:")]
        [DefaultValue("")]
        public _String ConfirmPassword { get; private set; }
    }
}
