using DevZest.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Samples.AdventureWorksLT
{
    [TestClass]
    public class CustomerTests
    {
        [TestMethod]
        public void Customer_ContactPerson()
        {
            var dataSet = DataSet<Customer>.Create();
            {
                var _ = dataSet._;
                Assert.IsNotNull(_.ContactPerson);
            }
            dataSet.AddRow((_, dataRow) =>
            {
                _.FirstName[dataRow] = "James";
                _.LastName[dataRow] = "Bond";
                _.Title[dataRow] = "Mr.";
            });
            {
                var _ = dataSet._;
                Assert.AreEqual("BOND, James (Mr.)", _.ContactPerson[0]);
            }
        }
    }
}
