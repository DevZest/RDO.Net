//using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using DevZest.Data.Primitives;


//namespace DevZest.Data.Helpers
//{
//    internal static class ColumnExtensions
//    {
//        public static void Verify(this Column column, Model model, Type declaringType, string name)
//        {
//            column.Verify(model, declaringType, name, declaringType, name);
//        }

//        public static void Verify(this Column column, Model model, Type declaringType, string name, Type originalDeclaringType, string originalName)
//        {
//            ModelMember modelProperty = column;
//            modelProperty.Verify(model, declaringType, name);
//            Assert.AreEqual(originalDeclaringType, column.OriginalDeclaringType);
//            Assert.AreEqual(originalName, column.OriginalName);
//        }

//        public static void VerifyParam<T>(this Column<T> column, T expectedValue)
//        {
//            var dbExpr = (DbParamExpression)column.DbExpression;
//            Assert.AreEqual(expectedValue, dbExpr.Value);
//            column.VerifyEval(expectedValue);
//        }

//        public static void VerifyConst<T>(this Column<T> column, T expectedValue)
//        {
//            var dbExpr = (DbConstantExpression)column.DbExpression;
//            Assert.AreEqual(expectedValue, dbExpr.Value);
//            column.VerifyEval(expectedValue);
//        }

//        public static void VerifyEval<T>(this Column<T> column, T expectedValue)
//        {
//            Assert.AreEqual(expectedValue, column.Eval());
//        }

//        public static void VerifyNullable(this Column column, bool expectedValue)
//        {
//            Assert.AreEqual(expectedValue, column.IsNullable);
//        }

//        public static void VerifyDefault<T>(this Column<T> column, T expectedValue)
//        {
//            Assert.AreEqual(expectedValue, column.GetDefault().Value);
//        }

//        public static T Eval<T>(this Column<T> column)
//        {
//            return column[null];
//        }
//    }
//}
