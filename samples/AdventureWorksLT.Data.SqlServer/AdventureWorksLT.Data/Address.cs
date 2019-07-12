using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    [DbIndex(nameof(IX_Address_StateProvince), Description = "Nonclustered index.")]
    [DbIndex(nameof(IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion), Description = "Nonclustered index.")]
    public class Address : BaseModel<Address.PK>
    {
        [DbPrimaryKey("PK_Address_AddressID", Description = "Primary key (clustered) constraint")]
        public sealed class PK : CandidateKey
        {
            public PK(_Int32 addressID)
                : base(addressID)
            {
            }
        }

        public class Key : Key<PK>
        {
            static Key()
            {
                Register((Key _) => _.AddressID, _AddressID);
            }

            protected sealed override PK CreatePrimaryKey()
            {
                return new PK(AddressID);
            }

            public _Int32 AddressID { get; private set; }
        }

        public class Ref : Ref<PK>
        {
            static Ref()
            {
                Register((Ref _) => _.AddressID, _AddressID);
            }

            public _Int32 AddressID { get; private set; }

            protected override PK CreateForeignKey()
            {
                return new PK(AddressID);
            }
        }

        public class Lookup : Projection
        {
            static Lookup()
            {
                Register((Lookup _) => _.AddressLine1, _AddressLine1);
                Register((Lookup _) => _.AddressLine2, _AddressLine2);
                Register((Lookup _) => _.City, _City);
                Register((Lookup _) => _.StateProvince, _StateProvince);
                Register((Lookup _) => _.CountryRegion, _CountryRegion);
                Register((Lookup _) => _.PostalCode, _PostalCode);
            }

            public _String AddressLine1 { get; private set; }
            public _String AddressLine2 { get; private set; }
            public _String City { get; private set; }
            public _String StateProvince { get; private set; }
            public _String CountryRegion { get; private set; }
            public _String PostalCode { get; private set; }
        }

        public static readonly Mounter<_Int32> _AddressID = RegisterColumn((Address _) => _.AddressID);
        public static readonly Mounter<_String> _AddressLine1 = RegisterColumn((Address _) => _.AddressLine1);
        public static readonly Mounter<_String> _AddressLine2 = RegisterColumn((Address _) => _.AddressLine2);
        public static readonly Mounter<_String> _City = RegisterColumn((Address _) => _.City);
        public static readonly Mounter<_String> _StateProvince = RegisterColumn((Address _) => _.StateProvince);
        public static readonly Mounter<_String> _CountryRegion = RegisterColumn((Address _) => _.CountryRegion);
        public static readonly Mounter<_String> _PostalCode = RegisterColumn((Address _) => _.PostalCode);

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(AddressID);
        }

        [Identity]
        [DbColumn(Description = "Primary key for Address records.")]
        public _Int32 AddressID { get; private set; }

        [Required]
        [SqlNVarChar(60)]
        [DbColumn(Description = "First street address line.")]
        public _String AddressLine1 { get; private set; }

        [SqlNVarChar(60)]
        [DbColumn(Description = "Second street address line.")]
        public _String AddressLine2 { get; private set; }

        [Required]
        [SqlNVarChar(30)]
        [DbColumn(Description = "Name of the city.")]
        public _String City { get; private set; }

        [UdtName]
        [DbColumn(Description = "Name of state or province.")]
        public _String StateProvince { get; private set; }

        [UdtName]
        public _String CountryRegion { get; private set; }

        [Required]
        [SqlNVarChar(15)]
        [DbColumn(Description = "Postal code for the street address.")]
        public _String PostalCode { get; private set; }

        [_DbIndex]
        private ColumnSort[] IX_Address_StateProvince
        {
            get { return new ColumnSort[] { StateProvince }; }
        }

        [_DbIndex]
        private ColumnSort[] IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion
        {
            get { return new ColumnSort[] { AddressLine1, AddressLine2, City, StateProvince, CountryRegion }; }
        }
    }
}
