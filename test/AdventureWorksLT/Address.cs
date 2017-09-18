using DevZest.Data;
using DevZest.Data.SqlServer;
using System;

namespace DevZest.Samples.AdventureWorksLT
{
    public class Address : BaseModel<Address.Key>
    {
        public sealed class Key : KeyBase
        {
            public Key(_Int32 addressID)
            {
                AddressID = addressID;
            }

            public _Int32 AddressID { get; private set; }
        }

        public static readonly Mounter<_Int32> _AddressID = RegisterColumn((Address x) => x.AddressID);
        public static readonly Mounter<_String> _AddressLine1 = RegisterColumn((Address x) => x.AddressLine1);
        public static readonly Mounter<_String> _AddressLine2 = RegisterColumn((Address x) => x.AddressLine2);
        public static readonly Mounter<_String> _City = RegisterColumn((Address x) => x.City);
        public static readonly Mounter<_String> _StateProvince = RegisterColumn((Address x) => x.StateProvince);
        public static readonly Mounter<_String> _CountryRegion = RegisterColumn((Address x) => x.CountryRegion);
        public static readonly Mounter<_String> _PostalCode = RegisterColumn((Address x) => x.PostalCode);

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

        [Required]
        [AsNVarChar(60)]
        public _String AddressLine1 { get; private set; }

        [AsNVarChar(60)]
        public _String AddressLine2 { get; private set; }

        [Required]
        [AsNVarChar(30)]
        public _String City { get; private set; }

        [UdtName]
        public _String StateProvince { get; private set; }

        [UdtName]
        public _String CountryRegion { get; private set; }

        [Required]
        [AsNVarChar(15)]
        public _String PostalCode { get; private set; }
    }
}
