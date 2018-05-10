using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class ProductModel : BaseModel<ProductModel.PK>
    {
        [DbPrimaryKey("PK_ProductModel_ProductModelID", Description = "Clustered index created by a primary key constraint.")]
        public sealed class PK : PrimaryKey
        {
            public PK(_Int32 productModelID)
            {
                ProductModelID = productModelID;
            }

            public _Int32 ProductModelID { get; private set; }
        }

        public static IDataValues GetKey(int productModelId)
        {
            return DataValues.Create(_Int32.Const(productModelId));
        }

        public class Key : Model<PK>
        {
            static Key()
            {
                RegisterColumn((Key _) => _.ProductModelID, _ProductModelID);
            }

            private PK _primaryKey;
            public sealed override PK PrimaryKey
            {
                get { return _primaryKey ?? (_primaryKey = new PK(ProductModelID)); }
            }

            public _Int32 ProductModelID { get; private set; }
        }

        public class Lookup : ModelExtender
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

        private PK _primaryKey;
        public sealed override PK PrimaryKey
        {
            get { return _primaryKey ?? (_primaryKey = new PK(ProductModelID)); }
        }

        [Identity(1, 1)]
        public _Int32 ProductModelID { get; private set; }

        [UdtName]
        public _String Name { get; private set; }

        public _SqlXml CatalogDescription { get; private set; }
    }
}
