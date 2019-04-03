using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Helpers
{
    internal static class DbExpressionExtensions
    {
        public static void Verify(this DbBinaryExpression expression,
            BinaryExpressionKind expectedKind,
            Column expectedLeft,
            Column expectedRight)
        {
            Assert.AreEqual(expectedKind, expression.Kind);
            Assert.AreEqual(expectedLeft.DbExpression, expression.Left);
            Assert.AreEqual(expectedRight.DbExpression, expression.Right);
        }

        public static void Verify(this DbCaseExpression expression, Column on, params Column[] whenElse)
        {
            Assert.AreEqual(on == null ? null : on.DbExpression, expression.On);
            Assert.AreEqual(1, whenElse.Length % 2);
            var whenCount = whenElse.Length >> 1;
            Assert.AreEqual(whenCount, expression.When.Count);
            Assert.AreEqual(whenCount, expression.Then.Count);
            for (int i = 0; i < whenCount; i++)
            {
                Assert.AreEqual(whenElse[i * 2].DbExpression, expression.When[i]);
                Assert.AreEqual(whenElse[i * 2 + 1].DbExpression, expression.Then[i]);
            }

            Assert.AreEqual(whenElse[whenElse.Length - 1].DbExpression, expression.Else);
        }

        public static void Verify(this DbCastExpression expression, Column operand, Type sourceDataType, Type targetDataType)
        {
            Assert.AreEqual(operand.DbExpression, expression.Operand);
            Assert.AreEqual(sourceDataType, expression.SourceDataType);
            Assert.AreEqual(targetDataType, expression.DataType);
        }

        public static void Verify(this DbFunctionExpression expression, FunctionKey functionKey)
        {
            Assert.AreEqual(functionKey, expression.FunctionKey);
            Assert.AreEqual(0, expression.ParamList.Count);
        }

        public static void Verify(this DbFunctionExpression expression, FunctionKey functionKey, params Column[] paramList)
        {
            Assert.AreEqual(functionKey, expression.FunctionKey);
            Assert.AreEqual(paramList.Length, expression.ParamList.Count);
            for (int i = 0; i < paramList.Length; i++)
                Assert.AreEqual(paramList[i].DbExpression, expression.ParamList[i]);
        }

        public static void Verify(this DbUnaryExpression expression, DbUnaryExpressionKind expectedKind, Column expectedOperand)
        {
            Assert.AreEqual(expectedKind, expression.Kind);
            Assert.AreEqual(expectedOperand.DbExpression, expression.Operand);
        }
    }
}
