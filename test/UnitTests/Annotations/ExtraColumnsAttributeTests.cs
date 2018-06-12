using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class ExtraColumnsAttributeTests
    {
        private class Header : Model
        {
            public class Ext : LeafProjection
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
            public class Ext : LeafProjection
            {
            }
        }

        [ExtraColumns(typeof(Header.Ext))]
        private class HeaderWithExt : Header
        {
            [ExtraColumns(typeof(Detail.Ext))]
            public override Detail Details => base.Details;
        }

        [TestMethod]
        public void ExtraColumnsAttribute()
        {
            var headerWithExt = new HeaderWithExt();
            headerWithExt.EnsureInitialized(false);
            Assert.IsNotNull(headerWithExt.GetExtraColumns<Header.Ext>());
            Assert.IsNotNull(headerWithExt.Details.GetExtraColumns<Detail.Ext>());
        }
    }
}
