using DevZest.Data;
using DevZest.Data.DbInit;

namespace DevZest.Samples.AdventureWorksLT
{
    public class DbGenerator : DbGenerator<Db>
    {
        private static DataSet<Address> GetAddress()
        {
            var result = DataSet<Address>.Create().AddRows(1);
            var _ = result._;
            _.AddressLine1[0] = "520 Mountainview Ave.";
            _.City[0] = "Valley Cottage";
            _.StateProvince[0] = "NY";
            _.PostalCode[0] = "10989";

            return result;
        }

        protected override void InitializeData()
        {
            SetData(Db.Address, GetAddress);
        }
    }
}
