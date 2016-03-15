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
        public void ScalarItemBuilder_Bind()
        {
            string source = SOURCE;

            var builder = new ScalarItem.Builder<TextBlock>(default(GridRangeBuilder));
            builder.Initialize(x => x.Text = INITIALIZED)
                .Bind((src, x) => x.Text = source)
                .Cleanup(x => x.Text = CLEANUP);

            var unit = builder.Item;
            var element = (TextBlock)unit.Generate();
            Assert.IsTrue(element.GetTemplateItem() == unit);

            unit.Initialize(element);
            Assert.AreEqual(INITIALIZED, element.Text);

            unit.UpdateTarget(element);
            Assert.AreEqual(SOURCE, element.Text);

            source = SOURCE_CHANGED;
            unit.UpdateTarget(element);
            Assert.AreEqual(SOURCE_CHANGED, element.Text);

            unit.Cleanup(element);
            Assert.AreEqual(CLEANUP, element.Text);
        }

        [TestMethod]
        public void ScalarItemBuilder_BindToSource()
        {
            string source = SOURCE;

            var builder = new ScalarItem.Builder<TextBlock>(default(GridRangeBuilder));
            builder.Initialize(x => x.Text = INITIALIZED)
                .BindToSource((x, src) => source = x.Text, BindingTrigger.Initialized, BindingTrigger.PropertyChanged(TextBlock.TextProperty))
                .Cleanup(x => x.Text = CLEANUP);

            var unit = builder.Item;
            var element = (TextBlock)unit.Generate();
            Assert.IsTrue(element.GetTemplateItem() == unit);

            unit.Initialize(element);
            Assert.AreEqual(INITIALIZED, element.Text);
            Assert.AreEqual(INITIALIZED, source);

            element.Text = TARGET;
            unit.UpdateSource(element);
            Assert.AreEqual(TARGET, source);

            element.Text = TARGET_CHANGED;
            unit.UpdateSource(element);
            Assert.AreEqual(TARGET_CHANGED, source);

            unit.Cleanup(element);
            Assert.AreEqual(CLEANUP, element.Text);
            Assert.AreEqual(TARGET_CHANGED, source);
        }
    }
}
