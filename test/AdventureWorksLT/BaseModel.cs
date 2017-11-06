using DevZest.Data;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public abstract class BaseModel<T> : Model<T>
        where T : KeyBase
    {
        public static readonly Mounter<_Guid> _RowGuid = RegisterColumn((BaseModel<T> x) => x.RowGuid, c => c.SetDefault(DevZest.Data.Functions.NewGuid()));
        public static readonly Mounter<_DateTime> _ModifiedDate = RegisterColumn((BaseModel<T> x) => x.ModifiedDate, x => x.SetDefault(DevZest.Data.Functions.GetDate()));

        [Required]
        public _Guid RowGuid { get; private set; }

        [Required]
        [AsDateTime]
        public _DateTime ModifiedDate { get; private set; }

    }
}
