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
            public static IDataValues Const(int productModelID)
            {
                return DataValues.Create(_Int32.Const(productModelID));
            }

            public PK(_Int32 productModelID)
                : base(productModelID)
            {
            }

            public _Int32 ProductModelID
            {
                get { return GetColumn<_Int32>(0); }
            }
        }

        public class Key : Key<PK>
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
                Register((Ref _) => _.ProductModelID, _ProductModelID);
            }

            public _Int32 ProductModelID { get; private set; }

            protected override PK GetForeignKey()
            {
                return new PK(ProductModelID);
            }
        }

        public class Lookup : ColumnGroup
        {
            static Lookup()
            {
                Register((Lookup _) => _.Name, _Name);
            }

            public _String Name { get; private set; }
        }

        protected static readonly Mounter<_Int32> _ProductModelID = RegisterColumn((ProductModel _) => _.ProductModelID);
        protected static readonly Mounter<_String> _Name = RegisterColumn((ProductModel _) => _.Name);
        protected static readonly Mounter<_SqlXml> _CatalogDescription = RegisterColumn((ProductModel _) => _.CatalogDescription);

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
