using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace DevZest.Data.SqlServer
{
    internal class ExpressionGenerator : DbExpressionVisitor
    {
        private const string NULL = "NULL";

        public IndentedStringBuilder SqlBuilder { get; internal set; }

        public IModelAliasManager ModelAliasManager { get; internal set; }

        public SqlVersion SqlVersion { get; internal set; }

        private static readonly Dictionary<BinaryExpressionKind, string> BinaryExpressionMappers = new Dictionary<BinaryExpressionKind, string>()
        {
            { BinaryExpressionKind.Add, " + " },
            { BinaryExpressionKind.And, " AND " },
            { BinaryExpressionKind.BitwiseAnd, " & " },
            { BinaryExpressionKind.BitwiseOr, " | " },
            { BinaryExpressionKind.BitwiseXor, " ^ " },
            { BinaryExpressionKind.Divide, " / " },
            { BinaryExpressionKind.Equal, " = " },
            { BinaryExpressionKind.GreaterThan, " > " },
            { BinaryExpressionKind.GreaterThanOrEqual, " >= " },
            { BinaryExpressionKind.LessThan, " < " },
            { BinaryExpressionKind.LessThanOrEqual, " <= " },
            { BinaryExpressionKind.Modulo, " % " },
            { BinaryExpressionKind.Multiply, " * " },
            { BinaryExpressionKind.NotEqual, " <> " },
            { BinaryExpressionKind.Or, " OR " },
            { BinaryExpressionKind.Substract, " - " },
        };
        public override void Visit(DbBinaryExpression e)
        {
            SqlBuilder.Append("(");
            e.Left.Accept(this);
            SqlBuilder.Append(BinaryExpressionMappers[e.Kind]);
            e.Right.Accept(this);
            SqlBuilder.Append(")");
        }

        public override void Visit(DbCaseExpression e)
        {
            SqlBuilder.Append("CASE");
            if (e.On != null)
            {
                SqlBuilder.Append(' ');
                e.On.Accept(this);
            }
            SqlBuilder.AppendLine();

            for (var i = 0; i < e.When.Count; ++i)
            {
                SqlBuilder.Indent++;
                SqlBuilder.Append("WHEN ");
                e.When[i].Accept(this);
                SqlBuilder.Append(" THEN ");
                e.Then[i].Accept(this);
                SqlBuilder.AppendLine();
                SqlBuilder.Indent--;
            }
            
            SqlBuilder.Indent++;
            SqlBuilder.Append("ELSE ");
            e.Else.Accept(this);
            SqlBuilder.AppendLine();
            SqlBuilder.Indent--;

            SqlBuilder.Append("END");
        }

        public override void Visit(DbCastExpression e)
        {
            SqlBuilder.Append("CAST(");
            e.Operand.Accept(this);
            SqlBuilder.Append(" AS ");
            SqlBuilder.Append(e.TargetColumn.GetMapper().GetDataTypeSql(this.SqlVersion));
            SqlBuilder.Append(")");
        }

        public override void Visit(DbColumnExpression e)
        {
            var columnName = e.DbColumnName.ToQuotedIdentifier();
            if (ModelAliasManager != null)
            {
                var modelAlias = ModelAliasManager[e.Column.GetParentModel()].ToQuotedIdentifier();
                SqlBuilder.Append(modelAlias).Append('.').Append(columnName);
            }
            else
                SqlBuilder.Append(columnName);
        }

        public override void Visit(DbConstantExpression e)
        {
            GenerateConst(SqlBuilder, SqlVersion, e.Column, e.Value);
        }

        internal static void GenerateConst(IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, Column column, object value)
        {
            if (value == null)
            {
                sqlBuilder.Append(NULL);
                return;
            }

            sqlBuilder.Append(column.GetMapper().GetTSqlLiteral(value, sqlVersion));
        }

        private static Dictionary<FunctionKey, Action<ExpressionGenerator, DbFunctionExpression>> s_functionHandlers = new Dictionary<FunctionKey, Action<ExpressionGenerator, DbFunctionExpression>>()
        {
            { Data.FunctionKeys.IsNull, (g, e) => g.VisitFunction_IsNull(e) },
            { Data.FunctionKeys.IsNotNull, (g, e) => g.VisitFunction_IsNotNull(e) },
            { Data.FunctionKeys.IfNull, (g, e) => g.VisitFunction_IfNull(e) },
            { Data.FunctionKeys.GetDate, (g, e) => g.VisitFunction_GetDate(e) },
            { Data.FunctionKeys.GetUtcDate, (g, e) => g.VisitFunction_GetUtcDate(e) },
            { Data.FunctionKeys.NewGuid, (g, e) => g.VisitFunction_NewGuid(e) },
            { Data.FunctionKeys.Average, (g, e) => g.VisitFunction_Average(e) },
            { Data.FunctionKeys.Count, (g, e) => g.VisitFunction_Count(e) },
            { Data.FunctionKeys.CountRows, (g, e) => g.VisitFunction_CountRows(e) },
            { Data.FunctionKeys.First, (g, e) => g.VisitFunction_First(e) },
            { Data.FunctionKeys.Last, (g, e) => g.VisitFunction_Last(e) },
            { Data.FunctionKeys.Max, (g, e) => g.VisitFunction_Max(e) },
            { Data.FunctionKeys.Min, (g, e) => g.VisitFunction_Min(e) },
            { Data.FunctionKeys.Sum, (g, e) => g.VisitFunction_Sum(e) },
            { Data.FunctionKeys.Contains, (g, e) => g.VisitFunction_Contains(e) },
            { FunctionKeys.XmlValue, (g, e) => g.VisitFunction_XmlValue(e) }
        };

        public override void Visit(DbFunctionExpression e)
        {
            Action<ExpressionGenerator, DbFunctionExpression> handler;
            try
            {
                handler = s_functionHandlers[e.FunctionKey];
            }
            catch (KeyNotFoundException)
            {
                throw new NotSupportedException(Strings.FunctionNotSupported(e.FunctionKey));
            }
            handler(this, e);
        }

        private void VisitFunction_IsNull(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(" IS NULL)");
        }

        private void VisitFunction_IsNotNull(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(" IS NOT NULL)");
        }

        private void VisitFunction_IfNull(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 2);
            SqlBuilder.Append("ISNULL(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(", ");
            e.ParamList[1].Accept(this);
            SqlBuilder.Append(')');
        }

        private void VisitFunction_GetDate(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 0);
            SqlBuilder.Append("GETDATE()");
        }

        private void VisitFunction_GetUtcDate(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 0);
            SqlBuilder.Append("GETUTCDATE()");
        }

        private void VisitFunction_NewGuid(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 0);
            SqlBuilder.Append("NEWID()");
        }

        private void VisitFunction_Average(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("AVG(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(")");
        }

        private void VisitFunction_Count(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("COUNT(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(")");
        }

        private void VisitFunction_CountRows(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("COUNT(*)");
        }


        private void VisitFunction_First(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("FIRST(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(")");
        }

        private void VisitFunction_Last(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("LAST(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(")");
        }

        private void VisitFunction_Max(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("MAX(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(")");
        }

        private void VisitFunction_Min(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("MIN(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(")");
        }

        private void VisitFunction_Sum(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("SUM(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(")");
        }

        private void VisitFunction_Contains(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 2);
            SqlBuilder.Append("(CHARINDEX(");
            e.ParamList[1].Accept(this);
            SqlBuilder.Append(", ");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(") > 0)");
        }

        private void VisitFunction_XmlValue(DbFunctionExpression e)
        {
            var paramList = e.ParamList;
            Debug.Assert(paramList.Count == 3);

            paramList[0].Accept(this);
            SqlBuilder.Append(".value(");
            paramList[1].Accept(this);
            SqlBuilder.Append(", ");
            var targetColumn = ((DbColumnExpression)paramList[2]).Column;
            SqlBuilder.AppendSingleQuoted(targetColumn.GetMapper().GetDataTypeSql(SqlVersion));
            SqlBuilder.Append(")");
        }

        private readonly List<DbParamExpression> DbParamExpressions = new List<DbParamExpression>();

        public int ParametersCount
        {
            get { return DbParamExpressions.Count; }
        }

        public DbParamExpression GetDbParamExpression(int index)
        {
            return DbParamExpressions[index];
        }

        internal SqlParameter CreateSqlParameter(int index)
        {
            var dbParamExpression = DbParamExpressions[index];
            var column = dbParamExpression.SourceColumn ?? dbParamExpression.Column;
            var columnMapper = column.GetMapper();
            return columnMapper.CreateSqlParameter(GetParamName(index), ParameterDirection.Input, dbParamExpression.Value, SqlVersion);
        }

        public override void Visit(DbParamExpression e)
        {
            int index = DbParamExpressions.IndexOf(e);
            if (index == -1)
            {
                index = DbParamExpressions.Count;
                DbParamExpressions.Add(e);
            }
            string paramName = GetParamName(index);
            SqlBuilder.Append(paramName);
        }

        internal static string GetParamName(int index)
        {
            return "@p" + (index + 1).ToString(CultureInfo.InvariantCulture);
        }

        private static readonly Dictionary<DbUnaryExpressionKind, string> UnaryExpressionMappers = new Dictionary<DbUnaryExpressionKind, string>()
        {
            { DbUnaryExpressionKind.Negate, "-" },
            { DbUnaryExpressionKind.Not, "NOT " },
            { DbUnaryExpressionKind.OnesComplement, "~" }
        };
        public override void Visit(DbUnaryExpression e)
        {
            SqlBuilder.Append("(");
            SqlBuilder.Append(UnaryExpressionMappers[e.Kind]);
            e.Operand.Accept(this);
            SqlBuilder.Append(")");
        }
    }
}
