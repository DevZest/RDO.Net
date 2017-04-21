using DevZest.Data;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public abstract class BaseModel<T> : Model<T>
        where T : ModelKey
    {
        public static readonly Property<_Guid> _RowGuid = RegisterColumn((BaseModel<T> x) => x.RowGuid, c => c.Default(DevZest.Data.Functions.NewGuid()));
        public static readonly Property<_DateTime> _ModifiedDate = RegisterColumn((BaseModel<T> x) => x.ModifiedDate, x => x.Default(DevZest.Data.Functions.GetDate()));

        [Required]
        public _Guid RowGuid { get; private set; }

        [Required]
        [AsDateTime]
        public _DateTime ModifiedDate { get; private set; }

    }
}
