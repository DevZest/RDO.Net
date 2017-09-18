using DevZest.Data;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class ProductModel : BaseModel<ProductModel.Key>
    {
        public sealed class Key : KeyBase
        {
            public Key(_Int32 productModelID)
            {
                ProductModelID = productModelID;
            }

            public _Int32 ProductModelID { get; private set; }
        }

        public class Ref : Model<Key>
        {
            public static readonly Mounter<_Int32> _ProductModelID = RegisterColumn((Ref _) => _.ProductModelID);

            public Ref()
            {
                _primaryKey = new Key(ProductModelID);
            }

            private readonly Key _primaryKey;
            public sealed override Key PrimaryKey
            {
                get { return _primaryKey; }
            }

            public _Int32 ProductModelID { get; private set; }
        }

        public static readonly Mounter<_Int32> _ProductModelID = RegisterColumn((ProductModel _) => _.ProductModelID, Ref._ProductModelID);
        public static readonly Mounter<_String> _Name = RegisterColumn((ProductModel _) => _.Name);
        public static readonly Mounter<_SqlXml> _CatalogDescription = RegisterColumn((ProductModel _) => _.CatalogDescription);

        public ProductModel()
        {
            _primaryKey = new Key(ProductModelID);
        }

        private Key _primaryKey;
        public sealed override Key PrimaryKey
        {
            get { return _primaryKey; }
        }

        [Identity(1, 1)]
        public _Int32 ProductModelID { get; private set; }

        [UdtName]
        public _String Name { get; private set; }

        public _SqlXml CatalogDescription { get; private set; }
    }
}
