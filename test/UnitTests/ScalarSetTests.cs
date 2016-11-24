using DevZest.Data.Windows.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Windows
{
    [TestClass]
    public class ScalarSetTests
    {
        [TestMethod]
        public void ScalarSet_New()
        {
            {
                Scalar[] scalars = null;
                Assert.AreEqual(ScalarSet.Empty, ScalarSet.New(scalars));
            }

            {
                Scalar[] scalars = new Scalar[] { null };
                Assert.AreEqual(ScalarSet.Empty, ScalarSet.New(scalars));
            }

            {
                Scalar[] scalars = new Scalar[] { null, null };
                Assert.AreEqual(ScalarSet.Empty, ScalarSet.New(scalars));
            }

            {
                var scalar = new Scalar<int>();
                Assert.AreEqual(scalar, ScalarSet.New(scalar));
            }

            {
                var scalar1 = new Scalar<int>();
                var scalar2 = new Scalar<int>();
                var scalarSet = ScalarSet.New(scalar1, scalar2);
                Assert.AreEqual(2, scalarSet.Count);
                Assert.IsTrue(scalarSet.Contains(scalar1));
                Assert.IsTrue(scalarSet.Contains(scalar2));
            }
        }

        [TestMethod]
        public void ScalarSet_Merge()
        {
            {
                Assert.AreEqual(ScalarSet.Empty, ScalarSet.Empty.Merge(ScalarSet.Empty));
            }

            {
                var scalar1 = new Scalar<int>();
                Assert.AreEqual(scalar1, ScalarSet.Empty.Merge(scalar1));
                Assert.AreEqual(scalar1, scalar1.Merge(ScalarSet.Empty));
            }

            {
                var scalar1 = new Scalar<int>();
                Assert.AreEqual(scalar1, scalar1.Merge(scalar1));
            }

            {
                var scalar1 = new Scalar<int>();
                var scalar2 = new Scalar<int>();
                var scalarSet = scalar1.Merge(scalar2);
                Assert.AreEqual(2, scalarSet.Count);
                Assert.IsTrue(scalarSet.Contains(scalar1));
                Assert.IsTrue(scalarSet.Contains(scalar2));
            }
        }
    }
}
