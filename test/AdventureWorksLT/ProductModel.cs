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

        public sealed class Key : Model<PK>
        {
            static Key()
            {
                RegisterColumn((Key _) => _.ProductModelID, _ProductModelID);
            }

            protected override PK CreatePrimaryKey()
            {
                return new PK(ProductModelID);
            }

            public _Int32 ProductModelID { get; private set; }
        }

        public class Ref : Ref<PK>
        {
            static Ref()
            {
                RegisterColumn((Ref _) => _.ProductModelID, _ProductModelID);
            }

            public _Int32 ProductModelID { get; private set; }

            protected override PK CreatePrimaryKey()
            {
                return new PK(ProductModelID);
            }
        }

        public class Lookup : Projection
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

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(ProductModelID);
        }

        [Identity(1, 1)]
        public _Int32 ProductModelID { get; private set; }

        [UdtName]
        public _String Name { get; private set; }

        public _SqlXml CatalogDescription { get; private set; }
    }
}
