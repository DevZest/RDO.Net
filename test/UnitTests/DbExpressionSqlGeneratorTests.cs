using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Data.SqlTypes;

namespace DevZest.Data.MySql
{
    [TestClass]
    public class DbExpressionSqlGeneratorTests
    {
        [TestMethod]
        public void DbExpressionSqlGenerator_DbConstantExpression()
        {
            {   //Binary
                var expr = CreateDbConstantExpression<_Binary, Binary>(null);
                VerifyDbExpression(MySqlVersion.LowestSupported, expr, "NULL");
                expr = CreateDbConstantExpression<_Binary, Binary>(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
                VerifyDbExpression(MySqlVersion.LowestSupported, expr, "0x0102030405060708090A0B0C0D0E0F10");
            }

            {   // Bit
                var expr = CreateDbConstantExpression<_Boolean, Boolean?>(null);
                VerifyDbExpression(MySqlVersion.LowestSupported, expr, "NULL");
                expr = CreateDbConstantExpression<_Boolean, Boolean?>(true);
                VerifyDbExpression(MySqlVersion.LowestSupported, expr, "1");
                expr = CreateDbConstantExpression<_Boolean, Boolean?>(false);
                VerifyDbExpression(MySqlVersion.LowestSupported, expr, "0");
            }

            {   // Char
                var expr = CreateDbConstantExpression<_Char, Char?>(null);
                VerifyDbExpression(MySqlVersion.LowestSupported, expr, "NULL");
                expr = CreateDbConstantExpression<_Char, Char?>('A');
                VerifyDbExpression(MySqlVersion.LowestSupported, expr, "'A'");
                expr = CreateDbConstantExpression<_Char, Char?>('A', x => x.AsMySqlChar(true));
                VerifyDbExpression(MySqlVersion.LowestSupported, expr, "N'A'");
            }

            {   // DateTime
                var expr = CreateDbConstantExpression<_DateTime, DateTime?>(null);
                VerifyDbExpression(MySqlVersion.LowestSupported, expr, "NULL");
                var dateTime = new DateTime(2015, 5, 14, 17, 14, 20, 888);
                expr = CreateDbConstantExpression<_DateTime, DateTime?>(dateTime);
                VerifyDbExpression(MySqlVersion.LowestSupported, expr, "(TIMESTAMP '2015-05-14 17:14:20')");
                expr = CreateDbConstantExpression<_DateTime, DateTime?>(dateTime, x => x.AsMySqlDate());
                VerifyDbExpression(MySqlVersion.LowestSupported, expr, "(DATE '2015-05-14')");
                expr = CreateDbConstantExpression<_DateTime, DateTime?>(dateTime, x => x.AsMySqlTime());
                VerifyDbExpression(MySqlVersion.LowestSupported, expr, "(TIME '17:14:20')");
                expr = CreateDbConstantExpression<_DateTime, DateTime?>(dateTime, x => x.AsMySqlDateTime(3));
                VerifyDbExpression(MySqlVersion.LowestSupported, expr, "(TIMESTAMP '2015-05-14 17:14:20.888')");
            }

            //{   // DateTimeOffset
            //    var expr = CreateDbConstantExpression<_DateTimeOffset, DateTimeOffset?>(null);
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "NULL");
            //    var dateTime = new DateTime(2015, 5, 14, 17, 14, 20, 888);
            //    expr = CreateDbConstantExpression<_DateTimeOffset, DateTimeOffset?>(new DateTimeOffset(dateTime));
            //    VerifyDbExpression(SqlVersion.Sql11, expr, string.Format("'{0}'", dateTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz")));
            //}

            //{   // Decimal
            //    var expr = CreateDbConstantExpression<_Decimal, Decimal?>(null);
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "NULL");
            //    expr = CreateDbConstantExpression<_Decimal, Decimal?>(3.1415926M);
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "3.1415926");
            //    expr = CreateDbConstantExpression<_Decimal, Decimal?>(3.1415926M, x => x.AsSqlMoney());
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "3.1415926");
            //    expr = CreateDbConstantExpression<_Decimal, Decimal?>(3.1415926M, x => x.AsSqlSmallMoney());
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "3.1415926");
            //}

            //{   // Double
            //    var expr = CreateDbConstantExpression<_Double, Double?>(null);
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "NULL");
            //    expr = CreateDbConstantExpression<_Double, Double?>(123457.2);
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "123457.2");
            //}

            //{   // Guid
            //    var expr = CreateDbConstantExpression<_Guid, Guid?>(null);
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "NULL");
            //    var guid = Guid.NewGuid();
            //    expr = CreateDbConstantExpression<_Guid, Guid?>(guid);
            //    VerifyDbExpression(SqlVersion.Sql11, expr, string.Format("'{0}'", guid.ToString()));
            //}

            //{   // Int16
            //    var expr = CreateDbConstantExpression<_Int16, Int16?>(null);
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "NULL");
            //    expr = CreateDbConstantExpression<_Int16, Int16?>(112);
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "112");
            //}

            //{   // Int32
            //    var expr = CreateDbConstantExpression<_Int32, Int32?>(null);
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "NULL");
            //    expr = CreateDbConstantExpression<_Int32, Int32?>(345);
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "345");
            //}

            //{   // Int64
            //    var expr = CreateDbConstantExpression<_Int64, Int64?>(null);
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "NULL");
            //    expr = CreateDbConstantExpression<_Int64, Int64?>(456);
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "456");
            //}

            //{   // Single
            //    var expr = CreateDbConstantExpression<_Single, Single?>(null);
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "NULL");
            //    expr = CreateDbConstantExpression<_Single, Single?>(12.5f);
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "12.5");
            //}

            //{   // string
            //    var expr = CreateDbConstantExpression<_String, String>(null);
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "NULL");
            //    expr = CreateDbConstantExpression<_String, String>("ABCD'EFG");
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "N'ABCD''EFG'");
            //    expr = CreateDbConstantExpression<_String, String>("ABCD'EFG", x => x.AsSqlVarChar(100));
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "'ABCD''EFG'");
            //}

            //{   // TimeSpan
            //    var expr = CreateDbConstantExpression<_TimeSpan, TimeSpan?>(null);
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "NULL");
            //    expr = CreateDbConstantExpression<_TimeSpan, TimeSpan?>(TimeSpan.FromHours(5));
            //    VerifyDbExpression(SqlVersion.Sql11, expr, "'05:00:00.0000000'");
            //}
        }

        private static DbConstantExpression CreateDbConstantExpression<TColumn, TData>(TData value, Action<TColumn> columnInitializer = null)
            where TColumn : Column<TData>, new()
        {
            var column = new ConstantExpression<TData>(value).MakeColumn<TColumn>();
            if (columnInitializer != null)
                columnInitializer(column);
            return (DbConstantExpression)column.DbExpression;
        }

        private static void VerifyDbExpression(MySqlVersion mySqlVersion, DbExpression expr, string expectedSql)
        {
            VerifyDbExpression(mySqlVersion, expr, expectedSql, out _);
        }

        private class ModelAliasManagerMock : IModelAliasManager
        {
            public string this[Model model]
            {
                get { return model.GetType().Name; }
            }
        }

        private static void VerifyDbExpression(MySqlVersion mySqlVersion, DbExpression expr, string expectedSql, out ExpressionGenerator generator)
        {
            generator = new ExpressionGenerator()
            {
                MySqlVersion = mySqlVersion,
                SqlBuilder = new IndentedStringBuilder(),
                ModelAliasManager = new ModelAliasManagerMock()
            };
            expr.Accept(generator);
            Assert.AreEqual(expectedSql, generator.SqlBuilder.ToString());
        }

        //[TestMethod]
        //public void DbExpressionSqlGenerator_DbColumnExpression()
        //{
        //    var model = new TestModel();
        //    var expr = CreateDbColumnExpression<_Int32>(model, "Column1");
        //    VerifyDbExpression(SqlVersion.Sql11, expr, "[TestModel].[Column1]");
        //    expr = CreateDbColumnExpression<_Int32>(model, "[Column2]");
        //    VerifyDbExpression(SqlVersion.Sql11, expr, "[TestModel].[Column2]");
        //}

        private class TestModel : Model
        {
        }

        //private static T CreateColumn<T>(TestModel parentModel, string columnName)
        //    where T : Column, new()
        //{
        //    var column = new T();
        //    column.ConstructModelMember(parentModel, typeof(TestModel), columnName);
        //    column.DbColumnName = columnName;
        //    return column;
        //}

        //private static DbColumnExpression CreateDbColumnExpression<T>(TestModel parentModel, string columnName)
        //    where T : Column, new()
        //{
        //    return new DbColumnExpression(CreateColumn<T>(parentModel, columnName));
        //}

//        [TestMethod]
//        public void DbExpressionSqlGenerator_DbBinaryExpression()
//        {
//            var model = new TestModel();
//            var column1 = CreateColumn<_Int32>(model, "Column1");
//            var column2 = CreateColumn<_Int32>(model, "Column2");
//            VerifyDbExpression(SqlVersion.Sql11, (column1 + column2).DbExpression, "([TestModel].[Column1] + [TestModel].[Column2])");
//            VerifyDbExpression(SqlVersion.Sql11, (column1 - column2).DbExpression, "([TestModel].[Column1] - [TestModel].[Column2])");
//            VerifyDbExpression(SqlVersion.Sql11, (column1 * column2).DbExpression, "([TestModel].[Column1] * [TestModel].[Column2])");
//            VerifyDbExpression(SqlVersion.Sql11, (column1 / column2).DbExpression, "([TestModel].[Column1] / [TestModel].[Column2])");
//            VerifyDbExpression(SqlVersion.Sql11, (column1 % column2).DbExpression, "([TestModel].[Column1] % [TestModel].[Column2])");
//            VerifyDbExpression(SqlVersion.Sql11, (column1 == column2).DbExpression, "([TestModel].[Column1] = [TestModel].[Column2])");
//            VerifyDbExpression(SqlVersion.Sql11, (column1 != column2).DbExpression, "([TestModel].[Column1] <> [TestModel].[Column2])");
//            VerifyDbExpression(SqlVersion.Sql11, (column1 > column2).DbExpression, "([TestModel].[Column1] > [TestModel].[Column2])");
//            VerifyDbExpression(SqlVersion.Sql11, (column1 >= column2).DbExpression, "([TestModel].[Column1] >= [TestModel].[Column2])");
//            VerifyDbExpression(SqlVersion.Sql11, (column1 < column2).DbExpression, "([TestModel].[Column1] < [TestModel].[Column2])");
//            VerifyDbExpression(SqlVersion.Sql11, (column1 <= column2).DbExpression, "([TestModel].[Column1] <= [TestModel].[Column2])");
//            VerifyDbExpression(SqlVersion.Sql11, (column1 & column2).DbExpression, "([TestModel].[Column1] & [TestModel].[Column2])");
//            VerifyDbExpression(SqlVersion.Sql11, (column1 | column2).DbExpression, "([TestModel].[Column1] | [TestModel].[Column2])");
//            VerifyDbExpression(SqlVersion.Sql11, (column1 ^ column2).DbExpression, "([TestModel].[Column1] ^ [TestModel].[Column2])");

//            model = new TestModel();
//            var boolColumn1 = CreateColumn<_Boolean>(model, "Column1");
//            var boolColumn2 = CreateColumn<_Boolean>(model, "Column2");
//            VerifyDbExpression(SqlVersion.Sql11, (boolColumn1 & boolColumn2).DbExpression, "([TestModel].[Column1] AND [TestModel].[Column2])");
//            VerifyDbExpression(SqlVersion.Sql11, (boolColumn1 | boolColumn2).DbExpression, "([TestModel].[Column1] OR [TestModel].[Column2])");
//        }

//        [TestMethod]
//        public void DbExpressionSqlGenerator_DbCaseExpression()
//        {
//            var model = new TestModel();
//            var column1 = CreateColumn<_Int32>(model, "Column1");
//            _Int32 c1 = _Int32.Const(1);
//            _Int32 c0 = _Int32.Const(0);

//            {
//                var expr = Case.On(column1)
//                    .When(c1).Then(_Boolean.True)
//                    .When(c0).Then(_Boolean.False)
//                    .Else(_Boolean.Null);
//                var expectedSql =
//@"CASE [TestModel].[Column1]
//    WHEN 1 THEN 1
//    WHEN 0 THEN 0
//    ELSE NULL
//END";
//                VerifyDbExpression(SqlVersion.Sql11, expr.DbExpression, expectedSql);
//            }

//            {
//                var expr = Case.When(column1 == c1).Then(_Boolean.True)
//                    .When(column1 == c0).Then(_Boolean.False)
//                    .Else(_Boolean.Null);
//                var expectedSql =
//@"CASE
//    WHEN ([TestModel].[Column1] = 1) THEN 1
//    WHEN ([TestModel].[Column1] = 0) THEN 0
//    ELSE NULL
//END";
//                VerifyDbExpression(SqlVersion.Sql11, expr.DbExpression, expectedSql);
//            }
//        }

//        [TestMethod]
//        public void DbExpressionSqlGenerator_DbCastExpression()
//        {
//            var model = new TestModel();
//            var int32Column = CreateColumn<_Int32>(model, "Column1");
//            VerifyDbExpression(SqlVersion.Sql11, ((_Int64)int32Column).DbExpression, "CAST([TestModel].[Column1] AS BIGINT)");
//        }

//        [TestMethod]
//        public void DbExpressionSqlGenerator_DbParamExpression()
//        {
//            var param = _Int32.Param(5);
//            ExpressionGenerator generator;
//            VerifyDbExpression(SqlVersion.Sql11, param.DbExpression, "@p1", out generator);
//            Assert.AreEqual(1, generator.ParametersCount);
//            var sqlParameter = generator.CreateSqlParameter(0);
//            Assert.AreEqual("@p1", sqlParameter.ParameterName);
//            Assert.AreEqual(SqlDbType.Int, sqlParameter.SqlDbType);
//            Assert.AreEqual(ParameterDirection.Input, sqlParameter.Direction);
//            Assert.AreEqual(5, sqlParameter.Value);
//        }

//        [TestMethod]
//        public void DbExpressionSqlGenerator_DbUnaryExpression()
//        {
//            var model = new TestModel();
//            var int32Column = CreateColumn<_Int32>(model, "Column1");
//            VerifyDbExpression(SqlVersion.Sql11, (-int32Column).DbExpression, "(-[TestModel].[Column1])");
//            VerifyDbExpression(SqlVersion.Sql11, (~int32Column).DbExpression, "(~[TestModel].[Column1])");

//            model = new TestModel();
//            var boolColumn = CreateColumn<_Boolean>(model, "Column1");
//            VerifyDbExpression(SqlVersion.Sql11, (!boolColumn).DbExpression, "(NOT [TestModel].[Column1])");
//        }

//        [TestMethod]
//        public void DbExpressionSqlGenerator_DbFunctionExpression()
//        {
//            VerifyDbExpression(SqlVersion.Sql11, Data.Functions.GetDate().DbExpression, "GETDATE()");
//            VerifyDbExpression(SqlVersion.Sql11, Data.Functions.GetUtcDate().DbExpression, "GETUTCDATE()");

//            var model = new TestModel();
//            var intColumn = CreateColumn<_Int32>(model, "Column1");

//            VerifyDbExpression(SqlVersion.Sql11, intColumn.IsNull().DbExpression, "([TestModel].[Column1] IS NULL)");
//            VerifyDbExpression(SqlVersion.Sql11, intColumn.IsNotNull().DbExpression, "([TestModel].[Column1] IS NOT NULL)");
//            VerifyDbExpression(SqlVersion.Sql11, intColumn.Average().DbExpression, "AVG([TestModel].[Column1])");
//            VerifyDbExpression(SqlVersion.Sql11, intColumn.Count().DbExpression, "COUNT([TestModel].[Column1])");
//            VerifyDbExpression(SqlVersion.Sql11, intColumn.First().DbExpression, "FIRST([TestModel].[Column1])");
//            VerifyDbExpression(SqlVersion.Sql11, intColumn.Last().DbExpression, "LAST([TestModel].[Column1])");
//            VerifyDbExpression(SqlVersion.Sql11, intColumn.Max().DbExpression, "MAX([TestModel].[Column1])");
//            VerifyDbExpression(SqlVersion.Sql11, intColumn.Min().DbExpression, "MIN([TestModel].[Column1])");
//            VerifyDbExpression(SqlVersion.Sql11, intColumn.Sum().DbExpression, "SUM([TestModel].[Column1])");

//            var stringColumn = CreateColumn<_String>(model, "Column2");
//            VerifyDbExpression(SqlVersion.Sql11, stringColumn.Contains(_String.Const("abc")).DbExpression, "(CHARINDEX(N'abc', [TestModel].[Column2]) > 0)");
//        }
    }
}
