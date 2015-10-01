using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DevZest.Data.Helpers;

namespace DevZest.Data
{
    [TestClass]
    public class ColumnTests
    {
        [TestMethod]
        public void Column_Nullable()
        {
            var column = new _Int32();
            column.VerifyNullable(true);

            column.Nullable(false);
            column.VerifyNullable(false);
            
            column.Nullable(true);
            column.VerifyNullable(true);
        }

        [TestMethod]
        public void Column_Default_const()
        {
            var column = new _Int32();
            column.DefaultValue(5);
            column.VerifyDefault(5);
        }

        [TestMethod]
        public void Column_Default_function()
        {
            var dateTime = new _DateTime();
            dateTime.Default(Functions.GetDate());
            var defaultValue = dateTime.GetDefault().DefaultValue.Eval();
            var span = DateTime.Now - defaultValue;
            Assert.IsTrue(span.Value.Seconds < 1);
        }
    }
}
