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
        const string TARGET = "Target";
        const string TARGET_CHANGED = "Target changed";

        [TestMethod]
        public void DataItemBuilder_Bind()
        {
            string source = SOURCE;

            var builder = new DataItem.Builder<TextBlock>(default(GridRangeBuilder));
            builder.Initialize(x => x.Text = INITIALIZED)
                .Bind((src, x) => x.Text = source)
                .Cleanup(x => x.Text = CLEANUP);

            var item = builder.Item;
            var element = (TextBlock)item.Generate();
            Assert.IsTrue(element.GetTemplateItem() == item);

            item.Initialize(element);
            Assert.AreEqual(SOURCE, element.Text);

            source = SOURCE_CHANGED;
            item.UpdateTarget(element);
            Assert.AreEqual(SOURCE_CHANGED, element.Text);

            item.Cleanup(element);
            Assert.AreEqual(CLEANUP, element.Text);
        }

        [TestMethod]
        public void DataItemBuilder_BindToSource()
        {
            string source = SOURCE;

            var builder = new DataItem.Builder<TextBlock>(default(GridRangeBuilder));
            builder.Initialize(x => x.Text = INITIALIZED)
                .BindToSource((x, src) => source = x.Text, BindingTrigger.Initialized, BindingTrigger.PropertyChanged(TextBlock.TextProperty))
                .Cleanup(x => x.Text = CLEANUP);

            var item = builder.Item;
            var element = (TextBlock)item.Generate();
            Assert.IsTrue(element.GetTemplateItem() == item);

            item.Initialize(element);
            Assert.AreEqual(INITIALIZED, element.Text);
            Assert.AreEqual(INITIALIZED, source);

            element.Text = TARGET;
            item.UpdateSource(element);
            Assert.AreEqual(TARGET, source);

            element.Text = TARGET_CHANGED;
            item.UpdateSource(element);
            Assert.AreEqual(TARGET_CHANGED, source);

            item.Cleanup(element);
            Assert.AreEqual(CLEANUP, element.Text);
            Assert.AreEqual(TARGET_CHANGED, source);
        }
    }
}
