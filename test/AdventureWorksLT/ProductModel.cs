using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class ProductModel : BaseModel<ProductModel.Key>
    {
        public sealed class Key : PrimaryKey
        {
            public Key(_Int32 productModelID)
            {
                ProductModelID = productModelID;
            }

            public _Int32 ProductModelID { get; private set; }
        }

        public class Ref : Model<Key>
        {
            static Ref()
            {
                RegisterColumn((Ref _) => _.ProductModelID, _ProductModelID);
            }

            private Key _primaryKey;
            public sealed override Key PrimaryKey
            {
                get
                {
                    if (_primaryKey == null)
                        _primaryKey = new Key(ProductModelID);
                    return _primaryKey;
                }
            }

            public _Int32 ProductModelID { get; private set; }
        }

        public class Lookup : ModelExtension
        {
            static Lookup()
            {
                RegisterColumn((Lookup _) => _.Name, _Name);
            }

            public _String Name { get; private set; }
        }

        public static readonly Mounter<_Int32> _ProductModelID;
        public static readonly Mounter<_String> _Name;
        public static readonly Mounter<_SqlXml> _CatalogDescription;

        static ProductModel()
        {
            _ProductModelID = RegisterColumn((ProductModel _) => _.ProductModelID);
            _Name = RegisterColumn((ProductModel _) => _.Name);
            _CatalogDescription = RegisterColumn((ProductModel _) => _.CatalogDescription);
        }

        private Key _primaryKey;
        public sealed override Key PrimaryKey
        {
            get
            {
                if (_primaryKey == null)
                    _primaryKey = new Key(ProductModelID);
                return _primaryKey;
            }
        }

        [Identity(1, 1)]
        public _Int32 ProductModelID { get; private set; }

        [UdtName]
        public _String Name { get; private set; }

        public _SqlXml CatalogDescription { get; private set; }
    }
}
