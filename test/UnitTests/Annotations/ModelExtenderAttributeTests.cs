using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class ModelExtenderAttributeTests
    {
        private class Header : Model
        {
            public class Ext : ColumnContainer
            {
            }

            static Header()
            {
                RegisterChildModel((Header _) => _.Details);
            }

            public virtual Detail Details { get; private set; }
        }

        private class Detail : Model
        {
            public class Ext : ColumnContainer
            {
            }
        }

        [ModelExtender(typeof(Header.Ext))]
        private class HeaderWithExt : Header
        {
            [ModelExtender(typeof(Detail.Ext))]
            public override Detail Details => base.Details;
        }

        [TestMethod]
        public void ModeExtenderAttribute()
        {
            var headerWithExt = new HeaderWithExt();
            headerWithExt.EnsureInitialized(false);
            Assert.IsNotNull(headerWithExt.GetExtender<Header.Ext>());
            Assert.IsNotNull(headerWithExt.Details.GetExtender<Detail.Ext>());
        }
    }
}
