using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace RazorPagesMovie.Models
{
    public class Movie : Model
    {
        public static readonly Mounter<_Int32> _ID = RegisterColumn((Movie _) => _.ID);
        public static readonly Mounter<_String> _Title = RegisterColumn((Movie _) => _.Title);
        public static readonly Mounter<_DateTime> _ReleaseDate = RegisterColumn((Movie _) => _.ReleaseDate);
        public static readonly Mounter<_String> _Genre = RegisterColumn((Movie _) => _.Genre);
        public static readonly Mounter<_Decimal> _Price = RegisterColumn((Movie _) => _.Price);

        [Identity(1, 1)]
        public _Int32 ID { get; private set; }

        [StringLength(60, MinimumLength = 3)]
        [Required]
        [SqlNVarChar(60)]
        public _String Title { get; private set; }

        [Display(Name = "Release Date")]
        public _DateTime ReleaseDate { get; private set; }

        [RegularExpression(@"^[A-Z]+[a-zA-Z""'\s-]*$")]
        [Required]
        [StringLength(30)]
        [SqlNVarChar(30)]
        public _String Genre { get; private set; }

        [SqlMoney]
        public _Decimal Price { get; private set; }
    }
}