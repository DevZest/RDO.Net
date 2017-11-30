using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public abstract class BaseModel<T> : Model<T>
        where T : ModelKey
    {
        public static readonly Mounter<_Guid> _RowGuid = RegisterColumn((BaseModel<T> x) => x.RowGuid);
        public static readonly Mounter<_DateTime> _ModifiedDate = RegisterColumn((BaseModel<T> x) => x.ModifiedDate);

        [Required]
        [AutoGuid]
        public _Guid RowGuid { get; private set; }

        [Required]
        [AsDateTime]
        [AutoDateTime]
        public _DateTime ModifiedDate { get; private set; }
    }
}
