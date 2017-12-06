using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public abstract class BaseModel<T> : Model<T>
        where T : PrimaryKey
    {
        public static readonly Mounter<_Guid> _RowGuid = RegisterColumn((BaseModel<T> x) => x.RowGuid);
        public static readonly Mounter<_DateTime> _ModifiedDate = RegisterColumn((BaseModel<T> x) => x.ModifiedDate);

        [Required]
        [AutoGuid(Name = "DF_%_rowguid")]
        [Unique(Name = "AK_%_rowguid")]
        [Description("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.")]
        public _Guid RowGuid { get; private set; }

        [Required]
        [AsDateTime]
        [AutoDateTime(Name = "DF_%_ModifiedDate")]
        [Description("Date and time the record was last updated.")]
        public _DateTime ModifiedDate { get; private set; }
    }
}
