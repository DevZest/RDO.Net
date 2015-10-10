using DevZest.Data;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public abstract class BaseModel<T> : Model<T>
        where T : ModelKey
    {
        public static readonly Accessor<BaseModel<T>, _Guid> RowGuidAccessor = RegisterColumn((BaseModel<T> x) => x.RowGuid, c => c.Default(DevZest.Data.Functions.NewGuid()));
        public static readonly Accessor<BaseModel<T>, _DateTime> ModifiedDateAccessor = RegisterColumn((BaseModel<T> x) => x.ModifiedDate, x => x.Default(DevZest.Data.Functions.GetDate()));

        [Nullable(false)]
        public _Guid RowGuid { get; private set; }

        [Nullable(false)]
        [AsDateTime]
        public _DateTime ModifiedDate { get; private set; }

    }
}
