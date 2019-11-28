using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.DbInit
{
    [TestClass]
    public class InputAttributeTests
    {
        private class TestObject
        {
            [Input(IsPassword = true, Order = 2)]
            public string Password { get; set; }

            [Input(Title = "User Name", EnvironmentVariableName = "DbInit_UserName", Order = 1)]
            public string UserName { get; set; }
        }

        [TestMethod]
        public void InputAttributes()
        {
            var obj = new TestObject();
            var attributes = InputAttribute.Resolve(obj.GetType());
            Assert.AreEqual(2, attributes.Length);
            Assert.AreEqual("User Name", attributes[0].Title);
            Assert.AreEqual("DbInit_UserName", attributes[0].EnvironmentVariableName);
            Assert.AreEqual(false, attributes[0].IsPassword);
            Assert.AreEqual(nameof(TestObject.Password), attributes[1].Title);
            Assert.AreEqual(nameof(TestObject.Password), attributes[1].EnvironmentVariableName);
            Assert.AreEqual(true, attributes[1].IsPassword);

            attributes[0].SetValue(obj, "TestUserName");
            attributes[1].SetValue(obj, "TestPassword");
            Assert.AreEqual("TestUserName", obj.UserName);
            Assert.AreEqual("TestPassword", obj.Password);
        }
    }
}
