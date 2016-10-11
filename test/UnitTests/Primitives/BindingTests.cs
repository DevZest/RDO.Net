using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class BindingTests
    {
        const string INITIALIZED = "Initialized";
        const string CLEANUP = "Cleanup";
        const string SOURCE = "Source";
        const string SOURCE_CHANGED = "Source changed";

        [TestMethod]
        public void ScalarBindingBuilder_OnRefresh()
        {
            string source = SOURCE;

            var builder = new ScalarBinding.Builder<TextBlock>(null);
            builder.OnSetup(v => v.Text = INITIALIZED)
                .OnRefresh(v => v.Text = source)
                .OnCleanup(v => v.Text = CLEANUP);

            var item = builder.TemplateItem;
            var element = (TextBlock)item.Setup();
            Assert.IsTrue(element.GetBinding() == item);
            Assert.AreEqual(SOURCE, element.Text);

            source = SOURCE_CHANGED;
            item.Refresh(element);
            Assert.AreEqual(SOURCE_CHANGED, element.Text);

            item.Cleanup(element);
            Assert.AreEqual(CLEANUP, element.Text);
        }
    }
}
