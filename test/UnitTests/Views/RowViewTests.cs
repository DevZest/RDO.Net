using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Views
{
    [TestClass]
    public class RowViewTests
    {
        [TestMethod]
        public void RowView_selectable_style_is_not_null()
        {
            Assert.IsNotNull(RowView.SelectableStyleKey.Style);
        }
    }
}
