using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class TemplateItemTests
    {
        const string INITIALIZED = "Initialized";
        const string CLEANUP = "Cleanup";
        const string SOURCE = "Source";
        const string SOURCE_CHANGED = "Source changed";

        [TestMethod]
        public void ScalarItemBuilder_OnRefresh()
        {
            string source = SOURCE;

            var builder = new ScalarItem.Builder<TextBlock>(null);
            builder.OnMount((v, p) => v.Text = INITIALIZED)
                .OnRefresh((v, p) => v.Text = source)
                .OnUnmount((v, p) => v.Text = CLEANUP);

            var item = builder.TemplateItem;
            var element = (TextBlock)item.Mount(null);
            Assert.IsTrue(element.GetTemplateItem() == item);
            Assert.AreEqual(SOURCE, element.Text);

            source = SOURCE_CHANGED;
            item.Refresh(element);
            Assert.AreEqual(SOURCE_CHANGED, element.Text);

            item.Unmount(element);
            Assert.AreEqual(CLEANUP, element.Text);
        }
    }
}
