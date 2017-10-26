using DevZest.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Samples.AdventureWorksLT
{
    [TestClass]
    public class LocalColumnManagerTests
    {
        [TestMethod]
        public void LocalColumnManager()
        {
            var dataSet = DataSet<Customer>.New(CustomerContactPerson.Initializer);
            {
                var _ = dataSet._;
                Assert.IsNotNull(_.GetContactPerson());
            }
            dataSet.AddRow((_, dataRow) =>
            {
                _.FirstName[dataRow] = "James";
                _.LastName[dataRow] = "Bond";
                _.Title[dataRow] = "Mr.";
            });
            {
                var _ = dataSet._;
                Assert.AreEqual("BOND, James (Mr.)", _.GetContactPerson()[0]);
            }
        }
    }
}
