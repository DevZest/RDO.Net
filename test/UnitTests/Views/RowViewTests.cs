using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Views
{
    [TestClass]
    public class RowViewTests
    {
        [TestMethod]
        public void RowView_Styles_Selectable_Style_is_not_null()
        {
            Assert.IsNotNull(RowView.Styles.Selectable.GetOrLoad());
        }
    }
}
