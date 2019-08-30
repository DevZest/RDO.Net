using DevZest.Data;
using DevZest.Data.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;

#if DbInit
using DevZest.Data.DbInit;
#endif

namespace DevZest.Samples.AdventureWorksLT
{
    public sealed class MockProductCategory : DbMock<Db>
    {
        public static Task<Db> CreateAsync(Db db, IProgress<DbInitProgress> progress = null, CancellationToken ct = default(CancellationToken))
        {
            return new MockProductCategory().MockAsync(db, progress, ct);
        }

        private static DataSet<ProductCategory> MockData()
        {
            DataSet<ProductCategory> result = DataSet<ProductCategory>.Create().AddRows(13);
            ProductCategory _ = result._;
            _.SuspendIdentity();
            _.ProductCategoryID[0] = 1;
            _.ProductCategoryID[1] = 2;
            _.ProductCategoryID[2] = 3;
            _.ProductCategoryID[3] = 4;
            _.ProductCategoryID[4] = 5;
            _.ProductCategoryID[5] = 6;
            _.ProductCategoryID[6] = 7;
            _.ProductCategoryID[7] = 8;
            _.ProductCategoryID[8] = 9;
            _.ProductCategoryID[9] = 10;
            _.ProductCategoryID[10] = 11;
            _.ProductCategoryID[11] = 12;
            _.ProductCategoryID[12] = 13;
            _.ParentProductCategoryID[0] = null;
            _.ParentProductCategoryID[1] = null;
            _.ParentProductCategoryID[2] = 1;
            _.ParentProductCategoryID[3] = 1;
            _.ParentProductCategoryID[4] = 1;
            _.ParentProductCategoryID[5] = 1;
            _.ParentProductCategoryID[6] = 2;
            _.ParentProductCategoryID[7] = 6;
            _.ParentProductCategoryID[8] = 6;
            _.ParentProductCategoryID[9] = 6;
            _.ParentProductCategoryID[10] = 7;
            _.ParentProductCategoryID[11] = 7;
            _.ParentProductCategoryID[12] = 7;
            _.Name[0] = "Bikes";
            _.Name[1] = "Other";
            _.Name[2] = "Mountain Bikes";
            _.Name[3] = "Road Bikes";
            _.Name[4] = "Touring Bikes";
            _.Name[5] = "Components";
            _.Name[6] = "Clothing";
            _.Name[7] = "Handlebars";
            _.Name[8] = "Bottom Brackets";
            _.Name[9] = "Brakes";
            _.Name[10] = "Bib-Shorts";
            _.Name[11] = "Caps";
            _.Name[12] = "Gloves";
            _.RowGuid[0] = new Guid("cfbda25c-df71-47a7-b81b-64ee161aa37c");
            _.RowGuid[1] = new Guid("09e91437-ba4f-4b1a-8215-74184fd95db8");
            _.RowGuid[2] = new Guid("2d364ade-264a-433c-b092-4fcbf3804e01");
            _.RowGuid[3] = new Guid("000310c0-bcc8-42c4-b0c3-45ae611af06b");
            _.RowGuid[4] = new Guid("02c5061d-ecdc-4274-b5f1-e91d76bc3f37");
            _.RowGuid[5] = new Guid("c657828d-d808-4aba-91a3-af2ce02300e9");
            _.RowGuid[6] = new Guid("10a7c342-ca82-48d4-8a38-46a2eb089b74");
            _.RowGuid[7] = new Guid("3ef2c725-7135-4c85-9ae6-ae9a3bdd9283");
            _.RowGuid[8] = new Guid("a9e54089-8a1e-4cf5-8646-e3801f685934");
            _.RowGuid[9] = new Guid("d43ba4a3-ef0d-426b-90eb-4be4547dd30c");
            _.RowGuid[10] = new Guid("67b58d2b-5798-4a90-8c6c-5ddacf057171");
            _.RowGuid[11] = new Guid("430dd6a8-a755-4b23-bb05-52520107da5f");
            _.RowGuid[12] = new Guid("92d5657b-0032-4e49-bad5-41a441a70942");
            _.ModifiedDate[0] = Convert.ToDateTime("2002-06-01T00:00:00.000");
            _.ModifiedDate[1] = Convert.ToDateTime("2002-06-01T00:00:00.000");
            _.ModifiedDate[2] = Convert.ToDateTime("2002-06-01T00:00:00.000");
            _.ModifiedDate[3] = Convert.ToDateTime("2002-06-01T00:00:00.000");
            _.ModifiedDate[4] = Convert.ToDateTime("2002-06-01T00:00:00.000");
            _.ModifiedDate[5] = Convert.ToDateTime("2002-06-01T00:00:00.000");
            _.ModifiedDate[6] = Convert.ToDateTime("2002-06-01T00:00:00.000");
            _.ModifiedDate[7] = Convert.ToDateTime("2002-06-01T00:00:00.000");
            _.ModifiedDate[8] = Convert.ToDateTime("2002-06-01T00:00:00.000");
            _.ModifiedDate[9] = Convert.ToDateTime("2002-06-01T00:00:00.000");
            _.ModifiedDate[10] = Convert.ToDateTime("2002-06-01T00:00:00.000");
            _.ModifiedDate[11] = Convert.ToDateTime("2002-06-01T00:00:00.000");
            _.ModifiedDate[12] = Convert.ToDateTime("2002-06-01T00:00:00.000");
            _.ResumeIdentity();
            return result;
        }

        protected override void Initialize()
        {
            Mock(Db.ProductCategory, MockData);
        }
    }
}
