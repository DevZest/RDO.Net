using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    [TestClass]
    public class GridEntryTests
    {
        const string INITIALIZED = "Initialized";
        const string CLEANUP = "Cleanup";
        const string SOURCE = "Source";
        const string SOURCE_CHANGED = "Source changed";
        const string TARGET = "Target";
        const string TARGET_CHANGED = "Target changed";

        [TestMethod]
        public void ScalarEntryBuilder_Bind()
        {
            string source = SOURCE;

            var builder = new ScalarEntry.Builder<TextBlock>(default(DataSetPresenterBuilderRange));
            builder.Initialize(x => x.Text = INITIALIZED)
                .Bind(x => x.Text = source)
                .Cleanup(x => x.Text = CLEANUP);

            var entry = builder.Entry;
            var element = (TextBlock)entry.Generate();
            Assert.IsTrue(element.GetGridEntry() == entry);

            entry.Initialize(element);
            Assert.AreEqual(INITIALIZED, element.Text);

            entry.UpdateTarget(element);
            Assert.AreEqual(SOURCE, element.Text);

            source = SOURCE_CHANGED;
            entry.UpdateTarget(element);
            Assert.AreEqual(SOURCE_CHANGED, element.Text);

            entry.Cleanup(element);
            Assert.AreEqual(CLEANUP, element.Text);
        }

        [TestMethod]
        public void ScalarEntryBuilder_BindToSource()
        {
            string source = SOURCE;

            var builder = new ScalarEntry.Builder<TextBlock>(default(DataSetPresenterBuilderRange));
            builder.Initialize(x => x.Text = INITIALIZED)
                .BindToSource(x => source = x.Text, BindingTrigger.Initialized, BindingTrigger.PropertyChanged(TextBlock.TextProperty))
                .Cleanup(x => x.Text = CLEANUP);

            var entry = builder.Entry;
            var element = (TextBlock)entry.Generate();
            Assert.IsTrue(element.GetGridEntry() == entry);

            entry.Initialize(element);
            Assert.AreEqual(INITIALIZED, element.Text);
            Assert.AreEqual(INITIALIZED, source);

            element.Text = TARGET;
            entry.UpdateSource(element);
            Assert.AreEqual(TARGET, source);

            element.Text = TARGET_CHANGED;
            entry.UpdateSource(element);
            Assert.AreEqual(TARGET_CHANGED, source);

            entry.Cleanup(element);
            Assert.AreEqual(CLEANUP, element.Text);
            Assert.AreEqual(TARGET_CHANGED, source);
        }
    }
}
