using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using DevZest.Data.SqlServer;
using DevZest.Samples.AdventureWorksLT;
using Moq;
using DevZest.Data.SqlServer.Addons;

namespace DevZest.Data.SqlServer
{
    [TestClass]
    public class ColumnExtensionsTests
    {
        [TestMethod]
        public void Column_default_SqlType()
        {
            VerifyDefaultSqlType<_Binary>(SqlVersion.Sql11, SqlDbType.VarBinary, ColumnExtensions.MAX_VARBINARY_SIZE, string.Format("VARBINARY({0})", ColumnExtensions.MAX_VARBINARY_SIZE));
            VerifyDefaultSqlType<_Boolean>(SqlVersion.Sql11, SqlDbType.Bit, "BIT");
            VerifyDefaultSqlType<_Byte>(SqlVersion.Sql11, SqlDbType.TinyInt, "TINYINT");
            VerifyDefaultSqlType<_Char>(SqlVersion.Sql11, SqlDbType.Char, 1, "CHAR(1)");
            VerifyDefaultSqlType<_DateTime>(SqlVersion.Sql11, SqlDbType.DateTime2, "DATETIME2(7)", 7);
            VerifyDefaultSqlType<_DateTimeOffset>(SqlVersion.Sql11, SqlDbType.DateTimeOffset, "DATETIMEOFFSET");
            VerifyDefaultSqlType<_Decimal>(SqlVersion.Sql11, SqlDbType.Decimal, string.Format("DECIMAL({0}, {1})", ColumnExtensions.DEFAULT_DECIMAL_PRECISION, ColumnExtensions.DEFAULT_DECIMAL_SCALE), ColumnExtensions.DEFAULT_DECIMAL_PRECISION, ColumnExtensions.DEFAULT_DECIMAL_SCALE);
            VerifyDefaultSqlType<_Double>(SqlVersion.Sql11, SqlDbType.Float, "FLOAT(53)", 53);
            VerifyDefaultSqlType<_Guid>(SqlVersion.Sql11, SqlDbType.UniqueIdentifier, "UNIQUEIDENTIFIER");
            VerifyDefaultSqlType<_Int16>(SqlVersion.Sql11, SqlDbType.SmallInt, "SMALLINT");
            VerifyDefaultSqlType<_Int32>(SqlVersion.Sql11, SqlDbType.Int, "INT");
            VerifyDefaultSqlType<_Int64>(SqlVersion.Sql11, SqlDbType.BigInt, "BIGINT");
            VerifyDefaultSqlType<_Single>(SqlVersion.Sql11, SqlDbType.Float, "FLOAT(24)", 24);
            VerifyDefaultSqlType<_String>(SqlVersion.Sql11, SqlDbType.NVarChar, ColumnExtensions.MAX_NVARCHAR_SIZE, string.Format("NVARCHAR({0})", ColumnExtensions.MAX_NVARCHAR_SIZE));
            VerifyDefaultSqlType<_TimeSpan>(SqlVersion.Sql11, SqlDbType.Time, "TIME");
            VerifyDefaultSqlType<_ByteEnum<SalesOrderStatus>>(SqlVersion.Sql11, SqlDbType.TinyInt, "TINYINT");
            VerifyDefaultSqlType<_CharEnum<SalesOrderStatus>>(SqlVersion.Sql11, SqlDbType.Char, 1, "CHAR(1)");
            VerifyDefaultSqlType<_Int16Enum<SalesOrderStatus>>(SqlVersion.Sql11, SqlDbType.SmallInt, "SMALLINT");
            VerifyDefaultSqlType<_Int32Enum<SalesOrderStatus>>(SqlVersion.Sql11, SqlDbType.Int, "INT");
            VerifyDefaultSqlType<_Int64Enum<SalesOrderStatus>>(SqlVersion.Sql11, SqlDbType.BigInt, "BIGINT");
        }

        private static void VerifyDefaultSqlType<T>(SqlVersion sqlVersion, SqlDbType sqlDbType, string sqlString)
            where T : Column, new()
        {
            VerifySqlType(sqlVersion, new T(), sqlDbType, sqlString);
        }

        private static void VerifyDefaultSqlType<T>(SqlVersion sqlVersion, SqlDbType sqlDbType, int size, string sqlString)
            where T : Column, new()
        {
            VerifySqlType(sqlVersion, new T(), sqlDbType, size, sqlString);
        }

        private static void VerifyDefaultSqlType<T>(SqlVersion sqlVersion, SqlDbType sqlDbType, string sqlString, byte precision)
            where T : Column, new()
        {
            VerifySqlType(sqlVersion, new T(), sqlDbType, sqlString, precision);
        }

        private static void VerifyDefaultSqlType<T>(SqlVersion sqlVersion, SqlDbType sqlDbType, string sqlString, byte precision, byte scale)
            where T : Column, new()
        {
            VerifySqlType(sqlVersion, new T(), sqlDbType, sqlString, precision, scale);
        }

        private static void VerifySqlType(SqlVersion sqlVersion, Column column, SqlDbType sqlDbType, string sqlString)
        {
            var expectedParamInfo = new SqlParameterInfo(sqlDbType, default(int?), default(byte?), default(byte?), default(string));
            VerifySqlType(sqlVersion, column, expectedParamInfo, sqlString);
        }

        private static void VerifySqlType(SqlVersion sqlVersion, Column column, SqlDbType sqlDbType, int size, string sqlString)
        {
            var expectedParamInfo = new SqlParameterInfo(sqlDbType, size, default(byte?), default(byte?), default(string));
            VerifySqlType(sqlVersion, column, expectedParamInfo, sqlString);
        }

        private static void VerifySqlType(SqlVersion sqlVersion, Column column, SqlDbType sqlDbType, string sqlString, byte precision)
        {
            var expectedParamInfo = new SqlParameterInfo(sqlDbType, default(int?), precision, default(byte?), default(string));
            VerifySqlType(sqlVersion, column, expectedParamInfo, sqlString);
        }

        private static void VerifySqlType(SqlVersion sqlVersion, Column column, SqlDbType sqlDbType, string sqlString, byte precision, byte scale)
        {
            var expectedParamInfo = new SqlParameterInfo(sqlDbType, default(int?), precision, scale, default(string));
            VerifySqlType(sqlVersion, column, expectedParamInfo, sqlString);
        }

        private static void VerifySqlType(SqlVersion sqlVersion, Column column, SqlParameterInfo expectedParamInfo, string expectedSqlString)
        {
            var sqlType = column.GetSqlType();
            VerifySqlParamInfo(sqlVersion, sqlType, expectedParamInfo);
            Assert.AreEqual(expectedSqlString, sqlType.GetDataTypeSql(sqlVersion));
        }

        private static void VerifySqlParamInfo(SqlVersion sqlVersion, SqlType sqlType, SqlParameterInfo expected)
        {
            var actual = sqlType.GetSqlParameterInfo(sqlVersion);
            Assert.AreEqual(expected.SqlDbType, actual.SqlDbType);
            Assert.AreEqual(expected.Size, actual.Size);
            Assert.AreEqual(expected.Precision, actual.Precision);
            Assert.AreEqual(expected.Scale, actual.Scale);
            Assert.AreEqual(expected.UdtTypeName, actual.UdtTypeName);
        }

        [TestMethod]
        public void Column_intercepted_SqlType()
        {
            {
                var binary = new _Binary().AsSqlBinary(500);
                VerifySqlType(SqlVersion.Sql11, binary, SqlDbType.Binary, 500, "BINARY(500)");
            }

            {
                var binaryMax = new _Binary().AsSqlBinaryMax();
                VerifySqlType(SqlVersion.Sql11, binaryMax, SqlDbType.Binary, -1, "BINARY(MAX)");
            }

            {
                var varBinary = new _Binary().AsSqlVarBinary(225);
                VerifySqlType(SqlVersion.Sql11, varBinary, SqlDbType.VarBinary, 225, "VARBINARY(225)");
            }

            {
                var varBinaryMax = new _Binary().AsSqlVarBinaryMax();
                VerifySqlType(SqlVersion.Sql11, varBinaryMax, SqlDbType.VarBinary, -1, "VARBINARY(MAX)");
            }

            {
                var timestamp = new _Binary().AsSqlTimestamp();
                VerifySqlType(SqlVersion.Sql11, timestamp, SqlDbType.Timestamp, "TIMESTAMP");
            }

            {
                var decimalColumn = new _Decimal().AsSqlDecimal(28, 8);
                VerifySqlType(SqlVersion.Sql11, decimalColumn, SqlDbType.Decimal, "DECIMAL(28, 8)", 28, 8);
            }

            {
                var smallMoney = new _Decimal().AsSqlSmallMoney();
                VerifySqlType(SqlVersion.Sql11, smallMoney, SqlDbType.SmallMoney, "SMALLMONEY");
            }

            {
                var money = new _Decimal().AsSqlMoney();
                VerifySqlType(SqlVersion.Sql11, money, SqlDbType.Money, "MONEY");
            }

            {
                var date = new _DateTime().AsSqlDate();
                VerifySqlType(SqlVersion.Sql11, date, SqlDbType.Date, "DATE");
            }

            {
                var time = new _DateTime().AsSqlTime();
                VerifySqlType(SqlVersion.Sql11, time, SqlDbType.Time, "TIME");
            }

            {
                var dateTime = new _DateTime().AsSqlDateTime();
                VerifySqlType(SqlVersion.Sql11, dateTime, SqlDbType.DateTime, "DATETIME");
            }

            {
                var smallDateTime = new _DateTime().AsSqlSmallDateTime();
                VerifySqlType(SqlVersion.Sql11, smallDateTime, SqlDbType.SmallDateTime, "SMALLDATETIME");
            }

            {
                var dateTime2 = new _DateTime().AsSqlDateTime2(5);
                VerifySqlType(SqlVersion.Sql11, dateTime2, SqlDbType.DateTime2, "DATETIME2(5)", 5);
            }

            {
                var charColumn = new _String().AsSqlChar(478);
                VerifySqlType(SqlVersion.Sql11, charColumn, SqlDbType.Char, 478, "CHAR(478)");
            }

            {
                var charMax = new _String().AsSqlCharMax();
                VerifySqlType(SqlVersion.Sql11, charMax, SqlDbType.Char, -1, "CHAR(MAX)");
            }

            {
                var nchar = new _String().AsSqlNChar(333);
                VerifySqlType(SqlVersion.Sql11, nchar, SqlDbType.NChar, 333, "NCHAR(333)");
            }

            {
                var ncharMax = new _String().AsSqlNCharMax();
                VerifySqlType(SqlVersion.Sql11, ncharMax, SqlDbType.NChar, -1, "NCHAR(MAX)");
            }

            {
                var varchar = new _String().AsSqlVarChar(512);
                VerifySqlType(SqlVersion.Sql11, varchar, SqlDbType.VarChar, 512, "VARCHAR(512)");
            }

            {
                var varcharMax = new _String().AsSqlVarCharMax();
                VerifySqlType(SqlVersion.Sql11, varcharMax, SqlDbType.VarChar, -1, "VARCHAR(MAX)");
            }

            {
                var nvarchar = new _String().AsSqlNVarChar(1024);
                VerifySqlType(SqlVersion.Sql11, nvarchar, SqlDbType.NVarChar, 1024, "NVARCHAR(1024)");
            }

            {
                var nvarcharMax = new _String().AsSqlNVarCharMax();
                VerifySqlType(SqlVersion.Sql11, nvarcharMax, SqlDbType.NVarChar, -1, "NVARCHAR(MAX)");
            }

            {
                var singleChar = new _Char().AsSqlChar(true);
                VerifySqlType(SqlVersion.Sql11, singleChar, SqlDbType.NChar, 1, "NCHAR(1)");
                singleChar = new _Char().AsSqlChar(false);
                VerifySqlType(SqlVersion.Sql11, singleChar, SqlDbType.Char, 1, "CHAR(1)");
            }
        }

        [TestMethod]
        public void Column_Clone_intercepted_SqlTypes()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var column = db.SalesOrderHeader._.AccountNumber.Clone(new Mock<Model>().Object);
                VerifySqlType(SqlVersion.Sql11, column, SqlDbType.NVarChar, 15, "NVARCHAR(15)");
            }
        }
    }
}
