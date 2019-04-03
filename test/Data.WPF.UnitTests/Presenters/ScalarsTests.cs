using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Presenters
{
    [TestClass]
    public class ScalarsTests
    {
        [TestMethod]
        public void Scalars_New()
        {
            {
                var scalar1 = ScalarContainerMock.New().CreateNew<int>();
                Assert.AreEqual(scalar1, Scalars.New(scalar1));
            }

            {
                var container = ScalarContainerMock.New();
                var scalar1 = container.CreateNew<int>();
                var scalar2 = container.CreateNew<int>();
                var scalars = Scalars.New(scalar1, scalar2);
                Assert.AreEqual(2, scalars.Count);
                Assert.IsTrue(scalars.Contains(scalar1));
                Assert.IsTrue(scalars.Contains(scalar2));
            }
        }

        [TestMethod]
        public void Scalars_Union()
        {
            {
                Assert.AreEqual(Scalars.Empty, Scalars.Empty.Union(Scalars.Empty));
            }

            {
                var container = ScalarContainerMock.New();
                var scalar1 = container.CreateNew<int>();
                Assert.AreEqual(scalar1, Scalars.Empty.Union(scalar1));
                Assert.AreEqual(scalar1, scalar1.Union(Scalars.Empty));
            }

            {
                var container = ScalarContainerMock.New();
                var scalar1 = container.CreateNew<int>();
                Assert.AreEqual(scalar1, scalar1.Union(scalar1));
            }

            {
                var container = ScalarContainerMock.New();
                var scalar1 = container.CreateNew<int>();
                var scalar2 = container.CreateNew<int>();
                var scalars = scalar1.Union(scalar2);
                Assert.AreEqual(2, scalars.Count);
                Assert.IsTrue(scalars.Contains(scalar1));
                Assert.IsTrue(scalars.Contains(scalar2));
            }
        }

        [TestMethod]
        public void Scalars_IsSubsetOf()
        {
            Assert.IsTrue(Scalars.Empty.IsSubsetOf(Scalars.Empty));

            var container = ScalarContainerMock.New();
            var scalar1 = container.CreateNew<int>();
            var scalar2 = container.CreateNew<int>();
            var scalar1And2 = Scalars.New(scalar1, scalar2);

            Assert.IsTrue(scalar1.IsSubsetOf(scalar1And2));
            Assert.IsTrue(scalar2.IsSubsetOf(scalar1And2));
            Assert.IsTrue(scalar1And2.IsSubsetOf(scalar1And2));
        }

        [TestMethod]
        public void Scalars_IsProperSubsetOf()
        {
            Assert.IsTrue(Scalars.Empty.IsSubsetOf(Scalars.Empty));

            var container = ScalarContainerMock.New();
            var scalar1 = container.CreateNew<int>();
            var scalar2 = container.CreateNew<int>();
            var scalar1And2 = Scalars.New(scalar1, scalar2);

            Assert.IsTrue(scalar1.IsProperSubsetOf(scalar1And2));
            Assert.IsTrue(scalar2.IsProperSubsetOf(scalar1And2));
            Assert.IsFalse(scalar1And2.IsProperSubsetOf(scalar1And2));
        }

        [TestMethod]
        public void Scalars_IsSupersetOf()
        {
            Assert.IsTrue(Scalars.Empty.IsSubsetOf(Scalars.Empty));

            var container = ScalarContainerMock.New();
            var scalar1 = container.CreateNew<int>();
            var scalar2 = container.CreateNew<int>();
            var scalar1And2 = Scalars.New(scalar1, scalar2);

            Assert.IsTrue(scalar1And2.IsSupersetOf(scalar1));
            Assert.IsTrue(scalar1And2.IsSupersetOf(scalar2));
            Assert.IsTrue(scalar1And2.IsSupersetOf(scalar1And2));
        }

        [TestMethod]
        public void Scalars_IsProperSupersetOf()
        {
            Assert.IsTrue(Scalars.Empty.IsSubsetOf(Scalars.Empty));

            var container = ScalarContainerMock.New();
            var scalar1 = container.CreateNew<int>();
            var scalar2 = container.CreateNew<int>();
            var scalar1And2 = Scalars.New(scalar1, scalar2);

            Assert.IsTrue(scalar1And2.IsProperSupersetOf(scalar1));
            Assert.IsTrue(scalar1And2.IsProperSupersetOf(scalar2));
            Assert.IsFalse(scalar1And2.IsProperSupersetOf(scalar1And2));
        }

        [TestMethod]
        public void Scalars_Equals()
        {
            var container = ScalarContainerMock.New();
            var scalar1 = container.CreateNew<int>();
            Assert.IsTrue(!scalar1.Equals(null));
            Assert.IsTrue(!scalar1.Equals(new object()));

            var scalar2 = container.CreateNew<int>();

            var scalars1 = Scalars.Empty.Add(scalar1).Add(scalar2);
            var scalars2 = Scalars.Empty.Add(scalar2).Add(scalar1);

            Assert.IsTrue(scalars1.Equals(scalars2));
        }
    }
}
