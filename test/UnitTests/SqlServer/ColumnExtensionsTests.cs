using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using DevZest.Data.SqlServer;
using DevZest.Samples.AdventureWorksLT;
using Moq;

namespace DevZest.Data.SqlServer
{
    [TestClass]
    public class ColumnExtensionsTests
    {
        [TestMethod]
        public void Column_default_column_mappers()
        {
            VerifyDefaultColumnMapper<_Binary>(SqlVersion.Sql11, SqlDbType.VarBinary, ColumnExtensions.MAX_VARBINARY_SIZE, string.Format("VARBINARY({0})", ColumnExtensions.MAX_VARBINARY_SIZE));
            VerifyDefaultColumnMapper<_Boolean>(SqlVersion.Sql11, SqlDbType.Bit, "BIT");
            VerifyDefaultColumnMapper<_Byte>(SqlVersion.Sql11, SqlDbType.TinyInt, "TINYINT");
            VerifyDefaultColumnMapper<_Char>(SqlVersion.Sql11, SqlDbType.Char, 1, "CHAR(1)");
            VerifyDefaultColumnMapper<_DateTime>(SqlVersion.Sql11, SqlDbType.DateTime2, "DATETIME2(7)", 7);
            VerifyDefaultColumnMapper<_DateTimeOffset>(SqlVersion.Sql11, SqlDbType.DateTimeOffset, "DATETIMEOFFSET");
            VerifyDefaultColumnMapper<_Decimal>(SqlVersion.Sql11, SqlDbType.Decimal, string.Format("DECIMAL({0}, {1})", ColumnExtensions.DEFAULT_DECIMAL_PRECISION, ColumnExtensions.DEFAULT_DECIMAL_SCALE), ColumnExtensions.DEFAULT_DECIMAL_PRECISION, ColumnExtensions.DEFAULT_DECIMAL_SCALE);
            VerifyDefaultColumnMapper<_Double>(SqlVersion.Sql11, SqlDbType.Float, "FLOAT(53)", 53);
            VerifyDefaultColumnMapper<_Guid>(SqlVersion.Sql11, SqlDbType.UniqueIdentifier, "UNIQUEIDENTIFIER");
            VerifyDefaultColumnMapper<_Int16>(SqlVersion.Sql11, SqlDbType.SmallInt, "SMALLINT");
            VerifyDefaultColumnMapper<_Int32>(SqlVersion.Sql11, SqlDbType.Int, "INT");
            VerifyDefaultColumnMapper<_Int64>(SqlVersion.Sql11, SqlDbType.BigInt, "BIGINT");
            VerifyDefaultColumnMapper<_Single>(SqlVersion.Sql11, SqlDbType.Float, "FLOAT(24)", 24);
            VerifyDefaultColumnMapper<_String>(SqlVersion.Sql11, SqlDbType.NVarChar, ColumnExtensions.MAX_NVARCHAR_SIZE, string.Format("NVARCHAR({0})", ColumnExtensions.MAX_NVARCHAR_SIZE));
            VerifyDefaultColumnMapper<_TimeSpan>(SqlVersion.Sql11, SqlDbType.Time, "TIME");
            VerifyDefaultColumnMapper<_ByteEnum<SalesOrderStatus>>(SqlVersion.Sql11, SqlDbType.TinyInt, "TINYINT");
            VerifyDefaultColumnMapper<_CharEnum<SalesOrderStatus>>(SqlVersion.Sql11, SqlDbType.Char, 1, "CHAR(1)");
            VerifyDefaultColumnMapper<_Int16Enum<SalesOrderStatus>>(SqlVersion.Sql11, SqlDbType.SmallInt, "SMALLINT");
            VerifyDefaultColumnMapper<_Int32Enum<SalesOrderStatus>>(SqlVersion.Sql11, SqlDbType.Int, "INT");
            VerifyDefaultColumnMapper<_Int64Enum<SalesOrderStatus>>(SqlVersion.Sql11, SqlDbType.BigInt, "BIGINT");
        }

        private static void VerifyDefaultColumnMapper<T>(SqlVersion sqlVersion, SqlDbType sqlDbType, string sqlString)
            where T : Column, new()
        {
            VerifyColumnMapper(sqlVersion, new T(), sqlDbType, sqlString);
        }

        private static void VerifyDefaultColumnMapper<T>(SqlVersion sqlVersion, SqlDbType sqlDbType, int size, string sqlString)
            where T : Column, new()
        {
            VerifyColumnMapper(sqlVersion, new T(), sqlDbType, size, sqlString);
        }

        private static void VerifyDefaultColumnMapper<T>(SqlVersion sqlVersion, SqlDbType sqlDbType, string sqlString, byte precision)
            where T : Column, new()
        {
            VerifyColumnMapper(sqlVersion, new T(), sqlDbType, sqlString, precision);
        }

        private static void VerifyDefaultColumnMapper<T>(SqlVersion sqlVersion, SqlDbType sqlDbType, string sqlString, byte precision, byte scale)
            where T : Column, new()
        {
            VerifyColumnMapper(sqlVersion, new T(), sqlDbType, sqlString, precision, scale);
        }

        private static void VerifyColumnMapper(SqlVersion sqlVersion, Column column, SqlDbType sqlDbType, string sqlString)
        {
            var expectedParamInfo = new SqlParameterInfo(sqlDbType, default(int?), default(byte?), default(byte?), default(string));
            VerifyColumnMapper(sqlVersion, column, expectedParamInfo, sqlString);
        }

        private static void VerifyColumnMapper(SqlVersion sqlVersion, Column column, SqlDbType sqlDbType, int size, string sqlString)
        {
            var expectedParamInfo = new SqlParameterInfo(sqlDbType, size, default(byte?), default(byte?), default(string));
            VerifyColumnMapper(sqlVersion, column, expectedParamInfo, sqlString);
        }

        private static void VerifyColumnMapper(SqlVersion sqlVersion, Column column, SqlDbType sqlDbType, string sqlString, byte precision)
        {
            var expectedParamInfo = new SqlParameterInfo(sqlDbType, default(int?), precision, default(byte?), default(string));
            VerifyColumnMapper(sqlVersion, column, expectedParamInfo, sqlString);
        }

        private static void VerifyColumnMapper(SqlVersion sqlVersion, Column column, SqlDbType sqlDbType, string sqlString, byte precision, byte scale)
        {
            var expectedParamInfo = new SqlParameterInfo(sqlDbType, default(int?), precision, scale, default(string));
            VerifyColumnMapper(sqlVersion, column, expectedParamInfo, sqlString);
        }

        private static void VerifyColumnMapper(SqlVersion sqlVersion, Column column, SqlParameterInfo expectedParamInfo, string expectedSqlString)
        {
            var columnMapper = column.GetSqlType();
            VerifySqlParamInfo(sqlVersion, columnMapper, expectedParamInfo);
            Assert.AreEqual(expectedSqlString, columnMapper.GetDataTypeSql(sqlVersion));
        }

        private static void VerifySqlParamInfo(SqlVersion sqlVersion, SqlType columnMapper, SqlParameterInfo expected)
        {
            var actual = columnMapper.GetSqlParameterInfo(sqlVersion);
            Assert.AreEqual(expected.SqlDbType, actual.SqlDbType);
            Assert.AreEqual(expected.Size, actual.Size);
            Assert.AreEqual(expected.Precision, actual.Precision);
            Assert.AreEqual(expected.Scale, actual.Scale);
            Assert.AreEqual(expected.UdtTypeName, actual.UdtTypeName);
        }

        [TestMethod]
        public void Column_intercepted_column_mappers()
        {
            {
                var binary = new _Binary().AsSqlBinary(500);
                VerifyColumnMapper(SqlVersion.Sql11, binary, SqlDbType.Binary, 500, "BINARY(500)");
            }

            {
                var binaryMax = new _Binary().AsSqlBinaryMax();
                VerifyColumnMapper(SqlVersion.Sql11, binaryMax, SqlDbType.Binary, -1, "BINARY(MAX)");
            }

            {
                var varBinary = new _Binary().AsSqlVarBinary(225);
                VerifyColumnMapper(SqlVersion.Sql11, varBinary, SqlDbType.VarBinary, 225, "VARBINARY(225)");
            }

            {
                var varBinaryMax = new _Binary().AsSqlVarBinaryMax();
                VerifyColumnMapper(SqlVersion.Sql11, varBinaryMax, SqlDbType.VarBinary, -1, "VARBINARY(MAX)");
            }

            {
                var timestamp = new _Binary().AsSqlTimestamp();
                VerifyColumnMapper(SqlVersion.Sql11, timestamp, SqlDbType.Timestamp, "TIMESTAMP");
            }

            {
                var decimalColumn = new _Decimal().AsSqlDecimal(28, 8);
                VerifyColumnMapper(SqlVersion.Sql11, decimalColumn, SqlDbType.Decimal, "DECIMAL(28, 8)", 28, 8);
            }

            {
                var smallMoney = new _Decimal().AsSqlSmallMoney();
                VerifyColumnMapper(SqlVersion.Sql11, smallMoney, SqlDbType.SmallMoney, "SMALLMONEY");
            }

            {
                var money = new _Decimal().AsSqlMoney();
                VerifyColumnMapper(SqlVersion.Sql11, money, SqlDbType.Money, "MONEY");
            }

            {
                var date = new _DateTime().AsSqlDate();
                VerifyColumnMapper(SqlVersion.Sql11, date, SqlDbType.Date, "DATE");
            }

            {
                var time = new _DateTime().AsSqlTime();
                VerifyColumnMapper(SqlVersion.Sql11, time, SqlDbType.Time, "TIME");
            }

            {
                var dateTime = new _DateTime().AsSqlDateTime();
                VerifyColumnMapper(SqlVersion.Sql11, dateTime, SqlDbType.DateTime, "DATETIME");
            }

            {
                var smallDateTime = new _DateTime().AsSqlSmallDateTime();
                VerifyColumnMapper(SqlVersion.Sql11, smallDateTime, SqlDbType.SmallDateTime, "SMALLDATETIME");
            }

            {
                var dateTime2 = new _DateTime().AsSqlDateTime2(5);
                VerifyColumnMapper(SqlVersion.Sql11, dateTime2, SqlDbType.DateTime2, "DATETIME2(5)", 5);
            }

            {
                var charColumn = new _String().AsSqlChar(478);
                VerifyColumnMapper(SqlVersion.Sql11, charColumn, SqlDbType.Char, 478, "CHAR(478)");
            }

            {
                var charMax = new _String().AsSqlCharMax();
                VerifyColumnMapper(SqlVersion.Sql11, charMax, SqlDbType.Char, -1, "CHAR(MAX)");
            }

            {
                var nchar = new _String().AsSqlNChar(333);
                VerifyColumnMapper(SqlVersion.Sql11, nchar, SqlDbType.NChar, 333, "NCHAR(333)");
            }

            {
                var ncharMax = new _String().AsSqlNCharMax();
                VerifyColumnMapper(SqlVersion.Sql11, ncharMax, SqlDbType.NChar, -1, "NCHAR(MAX)");
            }

            {
                var varchar = new _String().AsSqlVarChar(512);
                VerifyColumnMapper(SqlVersion.Sql11, varchar, SqlDbType.VarChar, 512, "VARCHAR(512)");
            }

            {
                var varcharMax = new _String().AsSqlVarCharMax();
                VerifyColumnMapper(SqlVersion.Sql11, varcharMax, SqlDbType.VarChar, -1, "VARCHAR(MAX)");
            }

            {
                var nvarchar = new _String().AsSqlNVarChar(1024);
                VerifyColumnMapper(SqlVersion.Sql11, nvarchar, SqlDbType.NVarChar, 1024, "NVARCHAR(1024)");
            }

            {
                var nvarcharMax = new _String().AsSqlNVarCharMax();
                VerifyColumnMapper(SqlVersion.Sql11, nvarcharMax, SqlDbType.NVarChar, -1, "NVARCHAR(MAX)");
            }

            {
                var singleChar = new _Char().IsUnicode(true);
                VerifyColumnMapper(SqlVersion.Sql11, singleChar, SqlDbType.NChar, 1, "NCHAR(1)");
                singleChar = new _Char().IsUnicode(false);
                VerifyColumnMapper(SqlVersion.Sql11, singleChar, SqlDbType.Char, 1, "CHAR(1)");
            }
        }

        [TestMethod]
        public void Column_Clone_intercepted_column_mappers()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var column = db.SalesOrderHeaders._.AccountNumber.Clone(new Mock<Model>().Object);
                VerifyColumnMapper(SqlVersion.Sql11, column, SqlDbType.NVarChar, 15, "NVARCHAR(15)");
            }
        }
    }
}
