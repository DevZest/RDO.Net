using DevZest.Data.Primitives;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace DevZest.Data.MySql
{
    internal sealed class SqlGenerator : DbFromClauseVisitor
    {
        internal static SqlGenerator Select(MySqlSession mySqlSession, DbQueryStatement query)
        {
            return mySqlSession._sqlGeneratorCache.GetValue(query, (DbQueryStatement x) =>
            {
                var result = new SqlGenerator(mySqlSession.MySqlVersion);
                x.Accept(result);
                return result;
            });
        }

        internal static SqlGenerator Insert(MySqlSession mySqlSession, DbSelectStatement statement)
        {
            var result = new SqlGenerator(mySqlSession.MySqlVersion);
            var model = statement.Model;
            var sqlBuilder = result.SqlBuilder;

            BuildInsertIntoClause(sqlBuilder, statement.Model, statement.Select);

            if (statement.Select == null)
                statement.From.Accept(result);
            else if (statement.Select.Count > 0)
                statement.Accept(result);

            return result;
        }

        private static void BuildInsertIntoClause(IndentedStringBuilder sqlBuilder, Model model, IReadOnlyList<ColumnMapping> select)
        {
            var insertList = GetInsertList(model, select);

            sqlBuilder.Append("INSERT INTO ");
            sqlBuilder.Append(model.GetDbTableClause().Name.ToQuotedIdentifier());

            if (insertList.Count == 0)
            {
                sqlBuilder.AppendLine(" () VALUES ();");
                return;
            }
            else
                sqlBuilder.AppendLine();

            sqlBuilder.Append('(');
            for (int i = 0; i < insertList.Count; i++)
            {
                sqlBuilder.Append(insertList[i].DbColumnName.ToQuotedIdentifier());
                if (i != insertList.Count - 1)
                    sqlBuilder.Append(", ");
            }
            sqlBuilder.AppendLine(")");
        }

        private static IList<Column> GetInsertList(Model model, IReadOnlyList<ColumnMapping> select)
        {
            if (select != null)
                return select.Select(x => x.Target).ToList();
            else
                return model.GetInsertableColumns().ToList();
        }

        internal static SqlGenerator InsertScalar(MySqlSession mySqlSession, DbSelectStatement statement)
        {
            var result = new SqlGenerator(mySqlSession.MySqlVersion);
            var model = statement.Model;
            var sqlBuilder = result.SqlBuilder;

            BuildInsertIntoClause(sqlBuilder, model, statement.Select);
            statement.Accept(result);

            return result;
        }

        internal static SqlGenerator Update(MySqlSession mySqlSession, DbSelectStatement statement)
        {
            var result = new SqlGenerator(mySqlSession.MySqlVersion);
            var sqlBuilder = result.SqlBuilder;
            var model = statement.Model;
            var selectList = statement.Select;
            var from = statement.From == null ? model.GetDbTableClause() : statement.From;
            var where = statement.Where;

            result.VisitingQueryStatement();

            result.ModelAliasManager = MySql.ModelAliasManager.Create(from);

            sqlBuilder.Append("UPDATE");
            result.GenerateFromStatement(from);
            sqlBuilder.AppendLine().AppendLine("SET");
            sqlBuilder.Indent++;
            var targetTable = result.ModelAliasManager[model].ToQuotedIdentifier();
            for (int i = 0; i < selectList.Count; i++)
            {
                if (i > 0)
                    sqlBuilder.AppendLine();
                var select = selectList[i];
                sqlBuilder.Append(targetTable).Append('.').Append(select.Target.DbColumnName.ToQuotedIdentifier());
                sqlBuilder.Append(" = ");
                select.SourceExpression.Accept(result._expressionGenerator);
                if (i != selectList.Count - 1)
                    sqlBuilder.Append(',');
            }
            sqlBuilder.Indent--;

            if (where != null)
                result.GenerateWhereClause(where);

            result.VisitedQueryStatement(statement);
            return result;
        }

        internal static SqlGenerator Delete(MySqlSession mySqlSession, DbSelectStatement statement)
        {
            var result = new SqlGenerator(mySqlSession.MySqlVersion);
            var sqlBuilder = result.SqlBuilder;
            var model = statement.Model;
            var selectList = statement.Select;
            var from = statement.From == null ? model.GetDbTableClause() : statement.From;
            var where = statement.Where;

            result.ModelAliasManager = MySql.ModelAliasManager.Create(from);

            sqlBuilder.Append("DELETE ");
            sqlBuilder.AppendLine(result.ModelAliasManager[model].ToQuotedIdentifier());

            result.VisitingQueryStatement();
            result.GenerateFromClause(from);
            if (where != null)
                result.GenerateWhereClause(where);
            result.VisitedQueryStatement(statement);

            return result;
        }

        private SqlGenerator(MySqlVersion mySqlVersion)
        {
            MySqlVersion = mySqlVersion;
            SqlBuilder = new IndentedStringBuilder();
            _expressionGenerator = new ExpressionGenerator()
            {
                MySqlVersion = MySqlVersion,
                SqlBuilder = SqlBuilder,
            };
        }

        private readonly MySqlVersion MySqlVersion;

        private readonly IndentedStringBuilder SqlBuilder = new IndentedStringBuilder();

        public override void Visit(DbTableClause table)
        {
            if (TryGenerateJsonTable(table))
                return;

            var model = table.Model;
            var tableName = table.Name.ToQuotedIdentifier();
            var alias = ModelAliasManager[model].ToQuotedIdentifier();
            SqlBuilder.Append(tableName);
            if (alias != tableName)
                SqlBuilder.Append(' ').Append(alias);
        }

        private bool TryGenerateJsonTable(DbTableClause table)
        {
            var model = table.Model;
            var jsonParam = model.GetSourceJsonParam();
            if (ReferenceEquals(jsonParam, null))
                return false;

            SqlBuilder.Append("JSON_TABLE(");
            jsonParam.DbExpression.Accept(_expressionGenerator);
            SqlBuilder.AppendLine(", '$[*]' COLUMNS (");
            SqlBuilder.Indent++;
            var columns = model.GetColumns();
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var columnName = column.DbColumnName.ToQuotedIdentifier();
                var mySqlType = column.GetMySqlType();
                SqlBuilder.Append(columnName).Append(' ').Append(mySqlType.GetDataTypeSql(MySqlVersion));
                if (!mySqlType.IsJsonOrdinalityType)
                    SqlBuilder.Append(" PATH ").Append(("$." + column.Name).ToLiteral());
                if (i < columns.Count - 1)
                    SqlBuilder.Append(',').AppendLine();
            }
            var alias = ModelAliasManager[model].ToQuotedIdentifier();
            SqlBuilder.Append(")) AS ").Append(alias);
            SqlBuilder.Indent--;
            return true;
        }

        public override void Visit(DbJoinClause join)
        {
            SqlBuilder.Append("(");
            join.Left.Accept(this);
            SqlBuilder.AppendLine();

            SqlBuilder.AppendLine(join.Kind.ToSql());

            join.Right.Accept(this);

            GenerateJoinOnClause(join.On);

            SqlBuilder.Append(")");
        }

        private void GenerateJoinOnClause(IReadOnlyList<ColumnMapping> joinOn)
        {
            if (joinOn == null)
                return;

            SqlBuilder.AppendLine();
            SqlBuilder.Append("ON ");
            for (int i = 0; i < joinOn.Count; i++)
            {
                if (i > 0)
                    SqlBuilder.Append(" AND ");
                var columnMapping = joinOn[i];
                var source = columnMapping.SourceExpression;
                var target = columnMapping.Target;
                columnMapping.SourceExpression.Accept(_expressionGenerator);
                SqlBuilder.Append(" = ");
                columnMapping.TargetExpression.Accept(_expressionGenerator);
            }
        }

        private IModelAliasManager ModelAliasManager
        {
            get { return _expressionGenerator.ModelAliasManager; }
            set { _expressionGenerator.ModelAliasManager = value; }
        }

        private readonly ExpressionGenerator _expressionGenerator;

        private int _queryStatementCount = 0;

        private bool IsTopLevelQuery
        {
            get { return _queryStatementCount == 1; }
        }

        public override void Visit(DbSelectStatement selectStatement)
        {
            VisitingQueryStatement();

            if (selectStatement.From == null)
                GenerateSelectClause(selectStatement.Select, appendLine: false);
            else
            {
                var savedModelAliasManager = ModelAliasManager;
                ModelAliasManager = MySql.ModelAliasManager.Create(selectStatement.From);

                GenerateSelectClause(selectStatement.Select, appendLine: true);
                GenerateFromClause(selectStatement.From);
                GenerateWhereClause(selectStatement.Where);
                GenerateGroupByClause(selectStatement.GroupBy);
                GenerateHavingClause(selectStatement.Having);
                if (IsTopLevelQuery || selectStatement.Offset != -1 || selectStatement.Fetch != -1)
                    GenerateOrderByClause(selectStatement.OrderBy, selectStatement.Offset, selectStatement.Fetch);

                ModelAliasManager = savedModelAliasManager;
            }
            VisitedQueryStatement(selectStatement);
        }

        public override void Visit(DbUnionStatement unionStatement)
        {
            VisitingQueryStatement();
            _countUnionStatement++;

            unionStatement.Query1.Accept(this);
            SqlBuilder.AppendLine();
            SqlBuilder.AppendLine(unionStatement.Kind == DbUnionKind.Union ? "UNION" : "UNION ALL");
            unionStatement.Query2.Accept(this);

            _countUnionStatement--;
            VisitedQueryStatement(unionStatement);
        }

        private void VisitingQueryStatement()
        {
            _queryStatementCount++;

            if (!IsTopLevelQuery)
                SqlBuilder.Append("(");
        }

        private int _countUnionStatement;
        private bool IsUnionStatement
        {
            get { return _countUnionStatement > 0; }
        }

        private void VisitedQueryStatement(DbQueryStatement query)
        {
            if (IsTopLevelQuery)
                SqlBuilder.AppendLine(";");
            else
            {
                if (IsUnionStatement)
                    SqlBuilder.Append(")");
                else
                {
                    var alias = ModelAliasManager[query.Model].ToQuotedIdentifier();
                    SqlBuilder.Append(") ").Append(alias);
                }
            }

            _queryStatementCount--;
        }

        private void GenerateSelectClause(IReadOnlyList<ColumnMapping> select, bool appendLine)
        {
            SqlBuilder.Append("SELECT");
            GenerateExpressionList(select.Count, i => select[i].SourceExpression, i => select[i].Target, appendLine);
        }

        private void GenerateExpressionList(int count, Func<int, DbExpression> getSelectExpression, Func<int, Column> getTargetColumn, bool appendLine)
        {
            Debug.Assert(getSelectExpression != null);

            SqlBuilder.Indent++;

            if (count == 1)
                SqlBuilder.Append(" ");
            else
                SqlBuilder.AppendLine();

            for (int i = 0; i < count; i++)
            {
                var selectExpression = getSelectExpression(i);
                selectExpression.Accept(_expressionGenerator);
                if (getTargetColumn != null)
                {
                    var targetColumn = getTargetColumn(i);
                    if (targetColumn != null)
                        SqlBuilder.Append(" AS ").Append(targetColumn.DbColumnName.ToQuotedIdentifier());
                }
                bool isLast = (i == count - 1);
                if (!isLast)
                    SqlBuilder.AppendLine(",");
                else if (appendLine)
                    SqlBuilder.AppendLine();
            }

            SqlBuilder.Indent--;
        }

        private void GenerateFromClause(DbFromClause from)
        {
            SqlBuilder.Append("FROM");
            GenerateFromStatement(from);
        }

        private void GenerateFromStatement(DbFromClause from)
        {
            bool isDbTable = from.GetType() == typeof(DbTableClause);

            if (isDbTable)
                SqlBuilder.Append(" ");
            else
                SqlBuilder.AppendLine().Indent++;

            from.Accept(this);

            if (!isDbTable)
                SqlBuilder.Indent--;
        }

        private void GenerateWhereClause(DbExpression where)
        {
            if (where == null)
                return;

            SqlBuilder.AppendLine().Append("WHERE ");
            where.Accept(_expressionGenerator);
        }

        private void GenerateGroupByClause(IReadOnlyList<DbExpression> groupBy)
        {
            if (groupBy == null || groupBy.Count == 0)
                return;

            SqlBuilder.AppendLine().Append("GROUP BY");
            GenerateExpressionList(groupBy.Count, i => groupBy[i], null, false);
        }

        private void GenerateHavingClause(DbExpression having)
        {
            if (having == null)
                return;

            SqlBuilder.AppendLine().Append("HAVING ");
            having.Accept(_expressionGenerator);
        }

        private void GenerateOrderByClause(IReadOnlyList<DbExpressionSort> orderByList, int offset, int rowCount)
        {
            if (orderByList == null)
                return;

            SqlBuilder.AppendLine().Append("ORDER BY ");
            for (int i = 0; i < orderByList.Count; i++)
            {
                var orderBy = orderByList[i];
                orderBy.Expression.Accept(_expressionGenerator);
                var direction = orderBy.Direction;
                if (direction == SortDirection.Descending)
                    SqlBuilder.Append(" DESC");
                else if (direction == SortDirection.Ascending)
                    SqlBuilder.Append(" ASC");
                bool isLast = i == orderByList.Count - 1;
                if (!isLast)
                    SqlBuilder.Append(", ");
            }

            if (offset != -1 && rowCount != -1)
            {
                SqlBuilder.AppendLine();
                SqlBuilder.Append("LIMIT ");
                if (offset != -1)
                {
                    SqlBuilder.Append(offset.ToString(NumberFormatInfo.InvariantInfo));
                    SqlBuilder.Append(", ");
                }
                if (rowCount != -1)
                    SqlBuilder.Append(rowCount.ToString(NumberFormatInfo.InvariantInfo));
                else
                    SqlBuilder.Append("18446744073709551615");  // max value of big int.
            }
        }

        public int CommandParametersCount
        {
            get { return _expressionGenerator.ParametersCount; }
        }

        public IEnumerable<MySqlParameter> ComandParameters
        {
            get
            {
                for (int i = 0; i < _expressionGenerator.ParametersCount; i++)
                    yield return _expressionGenerator.CreateMySqlParameter(i);
            }
        }

        public string CommandText
        {
            get { return SqlBuilder.ToString(); }
        }

        public MySqlCommand CreateCommand(MySqlConnection mySqlConnection)
        {
            return CommandText.CreateSqlCommand(mySqlConnection, ComandParameters);
        }
    }
}
