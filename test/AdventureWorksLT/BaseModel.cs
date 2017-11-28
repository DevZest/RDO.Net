using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public abstract class BaseModel<T> : Model<T>
        where T : KeyBase
    {
        public static readonly Mounter<_Guid> _RowGuid = RegisterColumn((BaseModel<T> x) => x.RowGuid);
        public static readonly Mounter<_DateTime> _ModifiedDate = RegisterColumn((BaseModel<T> x) => x.ModifiedDate);

        [Required]
        public _Guid RowGuid { get; private set; }

        [Required]
        [AsDateTime]
        public _DateTime ModifiedDate { get; private set; }

        [ColumnInitializer(nameof(RowGuid))]
        private static void InitializeRowGuid(_Guid rowGuid)
        {
            rowGuid.SetDefault(Functions.NewGuid());
        }

        [ColumnInitializer(nameof(ModifiedDate))]
        private static void InitializeModifiedDate(_DateTime modifiedDate)
        {
            modifiedDate.SetDefault(Functions.GetDate());
        }
    }
}
