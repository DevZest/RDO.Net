using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Tools
{
    [TestClass]
    public class IconsTests
    {
        [TestInitialize]
        public void InitTests()
        {
            // http://jake.ginnivan.net/pack-uri-in-unit-tests/
            System.Windows.Application.ResourceAssembly = typeof(Icons).Assembly;
        }

        [TestMethod]
        public void Icons_loaded_correctly()
        {
            Assert.IsNotNull(Icons.Model);
            Assert.IsNotNull(Icons.PrimaryKey);
            Assert.IsNotNull(Icons.Key);
            Assert.IsNotNull(Icons.Ref);
            Assert.IsNotNull(Icons.Column);
            Assert.IsNotNull(Icons.LocalColumn);
            Assert.IsNotNull(Icons.ColumnList);
            Assert.IsNotNull(Icons.Computation);
            Assert.IsNotNull(Icons.ChildModel);
            Assert.IsNotNull(Icons.ForeignKey);
            Assert.IsNotNull(Icons.CheckConstraint);
            Assert.IsNotNull(Icons.UniqueConstraint);
            Assert.IsNotNull(Icons.CustomValidator);
            Assert.IsNotNull(Icons.Index);
            Assert.IsNotNull(Icons.Projection);
            Assert.IsNotNull(Icons.Folder);
            Assert.IsNotNull(Icons.FolderOpen);
            Assert.IsNotNull(Icons.Db);
            Assert.IsNotNull(Icons.Table);
            Assert.IsNotNull(Icons.Relationship);
        }
    }
}
