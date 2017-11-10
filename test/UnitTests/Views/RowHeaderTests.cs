using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Views
{
    [TestClass]
    public class RowHeaderTests
    {
        [TestMethod]
        public void RowHeader_Styles_Flat_Style_is_not_null()
        {
            Assert.IsNotNull(RowHeader.Styles.Flat.GetOrLoad());
        }
    }
}
