using DevZest.Data;
using DevZest.Data.Annotations;

namespace ValidationUI
{
    public class User : Model
    {
        static User()
        {
            RegisterColumn((User _) => _.EmailAddress);
            RegisterColumn((User _) => _.Password);
        }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address:")]
        public _String EmailAddress { get; private set; }

        [Required]
        [StringLength(20, MinimumLength = 6)]
        [Display(Name = "Password:")]
        [DefaultValue("")]
        public _String Password { get; private set; }
    }
}
