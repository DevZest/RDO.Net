using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevZest.Samples.AdventureWorksLT;
using MySql.Data.MySqlClient;

namespace DevZest.Data.MySql
{
    [TestClass]
    public class ColumnExtensionsTests
    {
        [TestMethod]
        public void Column_default_MySqlType()
        {
            VerifyDefaultMySqlType<_Binary>(MySqlVersion.LowestSupported, MySqlDbType.VarBinary, ColumnExtensions.MAX_VARBINARY_SIZE, string.Format("VARBINARY({0})", ColumnExtensions.MAX_VARBINARY_SIZE));
            VerifyDefaultMySqlType<_Boolean>(MySqlVersion.LowestSupported, MySqlDbType.Bit, "BIT");
            VerifyDefaultMySqlType<_Byte>(MySqlVersion.LowestSupported, MySqlDbType.Byte, "TINYINT");
            VerifyDefaultMySqlType<_Char>(MySqlVersion.LowestSupported, MySqlDbType.String, 1, "CHAR(1)");
            VerifyDefaultMySqlType<_DateTime>(MySqlVersion.LowestSupported, MySqlDbType.DateTime, "DATETIME");
            VerifyDefaultMySqlType<_Decimal>(MySqlVersion.LowestSupported, MySqlDbType.Decimal, string.Format("DECIMAL({0}, {1})", ColumnExtensions.DEFAULT_DECIMAL_PRECISION, ColumnExtensions.DEFAULT_DECIMAL_SCALE), ColumnExtensions.DEFAULT_DECIMAL_PRECISION, ColumnExtensions.DEFAULT_DECIMAL_SCALE);
            VerifyDefaultMySqlType<_Double>(MySqlVersion.LowestSupported, MySqlDbType.Double, "DOUBLE");
            VerifyDefaultMySqlType<_Guid>(MySqlVersion.LowestSupported, MySqlDbType.Guid, "CHAR(36)");
            VerifyDefaultMySqlType<_Int16>(MySqlVersion.LowestSupported, MySqlDbType.Int16, "SMALLINT");
            VerifyDefaultMySqlType<_Int32>(MySqlVersion.LowestSupported, MySqlDbType.Int32, "INT");
            VerifyDefaultMySqlType<_Int64>(MySqlVersion.LowestSupported, MySqlDbType.Int64, "BIGINT");
            VerifyDefaultMySqlType<_Single>(MySqlVersion.LowestSupported, MySqlDbType.Float, "FLOAT");
            VerifyDefaultMySqlType<_String>(MySqlVersion.LowestSupported, MySqlDbType.VarString, ColumnExtensions.DEFAULT_VARCHAR_SIZE, string.Format("VARCHAR({0})", ColumnExtensions.DEFAULT_VARCHAR_SIZE));
            VerifyDefaultMySqlType<_ByteEnum<SalesOrderStatus>>(MySqlVersion.LowestSupported, MySqlDbType.Byte, "TINYINT");
            VerifyDefaultMySqlType<_CharEnum<SalesOrderStatus>>(MySqlVersion.LowestSupported, MySqlDbType.String, 1, "CHAR(1)");
            VerifyDefaultMySqlType<_Int16Enum<SalesOrderStatus>>(MySqlVersion.LowestSupported, MySqlDbType.Int16, "SMALLINT");
            VerifyDefaultMySqlType<_Int32Enum<SalesOrderStatus>>(MySqlVersion.LowestSupported, MySqlDbType.Int32, "INT");
            VerifyDefaultMySqlType<_Int64Enum<SalesOrderStatus>>(MySqlVersion.LowestSupported, MySqlDbType.Int64, "BIGINT");
        }

        private static void VerifyDefaultMySqlType<T>(MySqlVersion mySqlVersion, MySqlDbType mySqlDbType, string sqlString)
            where T : Column, new()
        {
            VerifyMySqlType(mySqlVersion, new T(), mySqlDbType, sqlString);
        }

        private static void VerifyDefaultMySqlType<T>(MySqlVersion mySqlVersion, MySqlDbType mySqlDbType, int size, string sqlString)
            where T : Column, new()
        {
            VerifyMySqlType(mySqlVersion, new T(), mySqlDbType, size, sqlString);
        }

        private static void VerifyDefaultMySqlType<T>(MySqlVersion mySqlVersion, MySqlDbType mySqlDbType, string sqlString, byte precision)
            where T : Column, new()
        {
            VerifyMySqlType(mySqlVersion, new T(), mySqlDbType, sqlString, precision);
        }

        private static void VerifyDefaultMySqlType<T>(MySqlVersion mySqlVersion, MySqlDbType mySqlDbType, string sqlString, byte precision, byte scale)
            where T : Column, new()
        {
            VerifyMySqlType(mySqlVersion, new T(), mySqlDbType, sqlString, precision, scale);
        }

        private static void VerifyMySqlType(MySqlVersion mySqlVersion, Column column, MySqlDbType mySqlDbType, string sqlString)
        {
            var expectedParamInfo = new MySqlParameterInfo(mySqlDbType, default(int?), default(byte?), default(byte?));
            VerifyMySqlType(mySqlVersion, column, expectedParamInfo, sqlString);
        }

        private static void VerifyMySqlType(MySqlVersion mySqlVersion, Column column, MySqlDbType mySqlDbType, int size, string sqlString)
        {
            var expectedParamInfo = new MySqlParameterInfo(mySqlDbType, size, default(byte?), default(byte?));
            VerifyMySqlType(mySqlVersion, column, expectedParamInfo, sqlString);
        }

        private static void VerifyMySqlType(MySqlVersion mySqlVersion, Column column, MySqlDbType mySqlDbType, string sqlString, byte precision)
        {
            var expectedParamInfo = new MySqlParameterInfo(mySqlDbType, default(int?), precision, default(byte?));
            VerifyMySqlType(mySqlVersion, column, expectedParamInfo, sqlString);
        }

        private static void VerifyMySqlType(MySqlVersion mySqlVersion, Column column, MySqlDbType mySqlDbType, string sqlString, byte precision, byte scale)
        {
            var expectedParamInfo = new MySqlParameterInfo(mySqlDbType, default(int?), precision, scale);
            VerifyMySqlType(mySqlVersion, column, expectedParamInfo, sqlString);
        }

        private static void VerifyMySqlType(MySqlVersion mySqlVersion, Column column, MySqlParameterInfo expectedParamInfo, string expectedSqlString)
        {
            var mySqlType = column.GetMySqlType();
            VerifyMySqlParamInfo(mySqlVersion, mySqlType, expectedParamInfo);
            Assert.AreEqual(expectedSqlString, mySqlType.GetDataTypeSql(mySqlVersion));
        }

        private static void VerifyMySqlParamInfo(MySqlVersion mySqlVersion, MySqlType mySqlType, MySqlParameterInfo expected)
        {
            var actual = mySqlType.GetSqlParameterInfo(mySqlVersion);
            Assert.AreEqual(expected.MySqlDbType, actual.MySqlDbType);
            Assert.AreEqual(expected.Size, actual.Size);
            Assert.AreEqual(expected.Precision, actual.Precision);
            Assert.AreEqual(expected.Scale, actual.Scale);
        }

        [TestMethod]
        public void Column_intercepted_MySqlType()
        {
            {
                var binary = new _Binary().AsMySqlBinary(255);
                VerifyMySqlType(MySqlVersion.LowestSupported, binary, MySqlDbType.Binary, 255, "BINARY(255)");
            }

            {
                var varBinary = new _Binary().AsMySqlVarBinary(4000);
                VerifyMySqlType(MySqlVersion.LowestSupported, varBinary, MySqlDbType.VarBinary, 4000, "VARBINARY(4000)");
            }

            {
                var timestamp = new _DateTime().AsMySqlTimestamp();
                VerifyMySqlType(MySqlVersion.LowestSupported, timestamp, MySqlDbType.Timestamp, "TIMESTAMP");
            }

            {
                var decimalColumn = new _Decimal().AsMySqlDecimal(28, 8);
                VerifyMySqlType(MySqlVersion.LowestSupported, decimalColumn, MySqlDbType.Decimal, "DECIMAL(28, 8)", 28, 8);
            }

            {
                var money = new _Decimal().AsMySqlMoney();
                VerifyMySqlType(MySqlVersion.LowestSupported, money, MySqlDbType.Decimal, "DECIMAL(19, 4)", 19, 4);
            }

            {
                var date = new _DateTime().AsMySqlDate();
                VerifyMySqlType(MySqlVersion.LowestSupported, date, MySqlDbType.Date, "DATE");
            }

            {
                var time = new _DateTime().AsMySqlTime();
                VerifyMySqlType(MySqlVersion.LowestSupported, time, MySqlDbType.Time, "TIME");
            }

            {
                var dateTime = new _DateTime().AsMySqlDateTime();
                VerifyMySqlType(MySqlVersion.LowestSupported, dateTime, MySqlDbType.DateTime, "DATETIME");
            }

            {
                var charColumn = new _String().AsMySqlChar(255, "utf8mb4");
                VerifyMySqlType(MySqlVersion.LowestSupported, charColumn, MySqlDbType.String, 255, "CHAR(255) CHARACTER SET utf8mb4");
            }

            {
                var varchar = new _String().AsMySqlVarChar(512, "utf8mb4");
                VerifyMySqlType(MySqlVersion.LowestSupported, varchar, MySqlDbType.VarString, 512, "VARCHAR(512) CHARACTER SET utf8mb4");
            }

            {
                var singleChar = new _Char().AsMySqlChar();
                VerifyMySqlType(MySqlVersion.LowestSupported, singleChar, MySqlDbType.String, 1, "CHAR(1)");
                singleChar = new _Char().AsMySqlChar("utf8mb4");
                VerifyMySqlType(MySqlVersion.LowestSupported, singleChar, MySqlDbType.String, 1, "CHAR(1) CHARACTER SET utf8mb4");
            }
        }
    }
}
