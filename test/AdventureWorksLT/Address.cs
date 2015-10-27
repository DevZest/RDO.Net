using DevZest.Data;
using DevZest.Data.SqlServer;
using System;

namespace DevZest.Samples.AdventureWorksLT
{
    public class Address : BaseModel<Address.Key>
    {
        public sealed class Key : ModelKey
        {
            public Key(_Int32 addressID)
            {
                AddressID = addressID;
            }

            public _Int32 AddressID { get; private set; }
        }

        public static readonly Accessor<Address, _Int32> AddressIDAccessor = RegisterColumn((Address x) => x.AddressID);
        public static readonly Accessor<Address, _String> AddressLine1Accessor = RegisterColumn((Address x) => x.AddressLine1);
        public static readonly Accessor<Address, _String> AddressLine2Accessor = RegisterColumn((Address x) => x.AddressLine2);
        public static readonly Accessor<Address, _String> CityAccessor = RegisterColumn((Address x) => x.City);
        public static readonly Accessor<Address, _String> StateProvinceAccessor = RegisterColumn((Address x) => x.StateProvince);
        public static readonly Accessor<Address, _String> CountryRegionAccessor = RegisterColumn((Address x) => x.CountryRegion);
        public static readonly Accessor<Address, _String> PostalCodeAccessor = RegisterColumn((Address x) => x.PostalCode);

        public Address()
        {
            _primaryKey = new Key(AddressID);
        }

        Key _primaryKey;
        public sealed override Key PrimaryKey
        {
            get { return _primaryKey; }
        }

        [Identity(1, 1)]
        public _Int32 AddressID { get; private set; }

        [Nullable(false)]
        [AsNVarChar(60)]
        public _String AddressLine1 { get; private set; }

        [Nullable(true)]
        [AsNVarChar(60)]
        public _String AddressLine2 { get; private set; }

        [Nullable(false)]
        [AsNVarChar(30)]
        public _String City { get; private set; }

        [UdtName]
        public _String StateProvince { get; private set; }

        [UdtName]
        public _String CountryRegion { get; private set; }

        [Nullable(false)]
        [AsNVarChar(15)]
        public _String PostalCode { get; private set; }
    }
}
