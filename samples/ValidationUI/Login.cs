using DevZest.Data;
using DevZest.Data.Annotations;

namespace ValidationUI
{
    public class Login : Model
    {
        static Login()
        {
            RegisterColumn((Login _) => _.EmailAddress);
            RegisterColumn((Login _) => _.Password);
        }

        [Required]
        [EmailAddress]
        [StringLength(20)]
        [Display(Name = "Email Address")]
        public _String EmailAddress { get; private set; }

        [Required]
        [StringLength(20, MinimumLength = 6)]
        [Display(Name = "Password")]
        [DefaultValue("")]
        public _String Password { get; private set; }
    }
}
