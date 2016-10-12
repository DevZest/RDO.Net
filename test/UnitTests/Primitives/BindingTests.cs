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
        public void ScalarBinding_OnRefresh()
        {
            string source = SOURCE;

            var scalarBinding = new ScalarBinding<TextBlock>()
            {
                OnSetup = x => x.Text = INITIALIZED,
                OnRefresh = x => x.Text = source,
                OnCleanup = x => x.Text = CLEANUP
            };
            var element = (TextBlock)scalarBinding.Setup();
            Assert.IsTrue(element.GetBinding() == scalarBinding);
            Assert.AreEqual(SOURCE, element.Text);

            source = SOURCE_CHANGED;
            scalarBinding.Refresh(element);
            Assert.AreEqual(SOURCE_CHANGED, element.Text);

            scalarBinding.Cleanup(element);
            Assert.AreEqual(CLEANUP, element.Text);
        }
    }
}
