using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class ColumnInitializerAttributeTests
    {
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Id);
            }

            public _Int32 Id { get; private set; }

            [ColumnInitializer(nameof(Id))]
            private static void InitializeId(_Int32 column)
            {
                column.Nullable(false);
            }
        }

        [TestMethod]
        public void ColumnInitializer()
        {
            var _ = new TestModel();
            Assert.IsFalse(_.Id.IsNullable);
        }
    }
}
