using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    [DbCompositeIndex(IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion, Description = "Nonclustered index.")]
    public class Address : BaseModel<Address.PK>
    {
        private const string IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion = nameof(IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion);

        [DbPrimaryKey("PK_Address_AddressID", Description = "Primary key (clustered) constraint")]
        public sealed class PK : PrimaryKey
        {
            public static IDataValues Const(int addressID)
            {
                return DataValues.Create(_Int32.Const(addressID));
            }

            public PK(_Int32 addressID)
                : base(addressID)
            {
            }

            public _Int32 AddressID
            {
                get { return GetColumn<_Int32>(0); }
            }
        }

        public sealed class Key : Key<PK>
        {
            static Key()
            {
                RegisterColumn((Key _) => _.AddressID, _AddressID);
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

            protected override PK GetForeignKey()
            {
                return new PK(AddressID);
            }
        }

        public class Lookup : ColumnGroup
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

        public static readonly Mounter<_Int32> _AddressID;
        public static readonly Mounter<_String> _AddressLine1;
        public static readonly Mounter<_String> _AddressLine2;
        public static readonly Mounter<_String> _City;
        public static readonly Mounter<_String> _StateProvince;
        public static readonly Mounter<_String> _CountryRegion;
        public static readonly Mounter<_String> _PostalCode;

        static Address()
        {
            _AddressID = RegisterColumn((Address _) => _.AddressID);
            _AddressLine1 = RegisterColumn((Address _) => _.AddressLine1);
            _AddressLine2 = RegisterColumn((Address _) => _.AddressLine2);
            _City = RegisterColumn((Address _) => _.City);
            _StateProvince = RegisterColumn((Address _) => _.StateProvince);
            _CountryRegion = RegisterColumn((Address _) => _.CountryRegion);
            _PostalCode = RegisterColumn((Address _) => _.PostalCode);
        }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(AddressID);
        }

        [Identity(1, 1)]
        [DbColumn(Description = "Primary key for Address records.")]
        public _Int32 AddressID { get; private set; }

        [Required]
        [AsNVarChar(60)]
        [DbColumn(Description = "First street address line.")]
        [DbCompositeIndexMember(IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion, Order = 1)]
        public _String AddressLine1 { get; private set; }

        [AsNVarChar(60)]
        [DbColumn(Description = "Second street address line.")]
        [DbCompositeIndexMember(IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion, Order = 2)]
        public _String AddressLine2 { get; private set; }

        [Required]
        [AsNVarChar(30)]
        [DbColumn(Description = "Name of the city.")]
        [DbCompositeIndexMember(IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion, Order = 3)]
        public _String City { get; private set; }

        [UdtName]
        [DbColumn(Description = "Name of state or province.")]
        [DbCompositeIndexMember(IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion, Order = 4)]
        [DbIndex("IX_Address_StateProvince", Description = "Nonclustered index.")]
        public _String StateProvince { get; private set; }

        [UdtName]
        [DbCompositeIndexMember(IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion, Order = 5)]
        public _String CountryRegion { get; private set; }

        [Required]
        [AsNVarChar(15)]
        [DbColumn(Description = "Postal code for the street address.")]
        public _String PostalCode { get; private set; }
    }
}
