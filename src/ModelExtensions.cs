using DevZest.Data.Addons;
using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.MySql
{
    internal static class ModelExtensions
    {
        internal static void GenerateCreateTableSql(this Model model, IndentedStringBuilder sqlBuilder, MySqlVersion mySqlVersion, bool isTempTable)
        {
            model.GenerateCreateTableSql(model.GetDbTableName(), model.GetDbTableDescription(), sqlBuilder, mySqlVersion, isTempTable);
        }

        internal static void GenerateCreateTableSql(this Model model, string tableName, string description, IndentedStringBuilder sqlBuilder, MySqlVersion mySqlVersion, bool isTempTable)
        {
            tableName = tableName.ToQuotedIdentifier();

            if (isTempTable)    // Temp table may already exist when connection is reused with connection pooling, drop it if exists.
            {
                sqlBuilder.AppendLine("SET @@sql_notes = 0;");  // workaround: DROP TABLE IF EXISTS causes warning if table does not exist (https://bugs.mysql.com/bug.php?id=2839)
                sqlBuilder.Append("DROP TEMPORARY TABLE IF EXISTS ").Append(tableName).AppendLine(";").AppendLine();
                sqlBuilder.AppendLine("SET @@sql_notes = 1;");
            }

            sqlBuilder.Append("CREATE ");
            if (isTempTable)
                sqlBuilder.Append(" TEMPORARY ");
            sqlBuilder.Append("TABLE ").Append(tableName).AppendLine(" (");
            sqlBuilder.Indent++;

            var columns = model.GetColumns();
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                column.GenerateColumnDefinitionSql(sqlBuilder, mySqlVersion, tableName, isTempTable, i < columns.Count - 1 || model.HasConstraintOrIndex(isTempTable));
            }

            int countConstraints = model.GenerateConstraints(sqlBuilder, mySqlVersion, tableName, isTempTable);
            int countIndexes = model.GenerateIndexes(sqlBuilder, mySqlVersion, tableName, isTempTable, countConstraints > 0);
            if (countConstraints + countIndexes > 0)
                sqlBuilder.AppendLine();

            sqlBuilder.Indent--;
            sqlBuilder.Append(")");

            if (!isTempTable)
                sqlBuilder.GenerateComment(description);

            sqlBuilder.AppendLine(";");
        }

        private static bool HasConstraintOrIndex(this Model model, bool isTempTable)
        {
            IReadOnlyList<DbTableConstraint> constraints = model.GetAddons<DbTableConstraint>();
            bool result;
            if (isTempTable)
                result = constraints.Where(x => x.IsValidOnTempTable).Any();
            else
                result = constraints.Where(x => x.IsValidOnTable).Any();

            if (result)
                return true;

            IReadOnlyList<DbIndex> indexes = model.GetAddons<DbIndex>();
            if (isTempTable)
                return indexes.Where(x => x.IsValidOnTempTable).Any();
            else
                return indexes.Where(x => x.IsValidOnTable).Any();
        }

        internal static IndentedStringBuilder GenerateComment(this IndentedStringBuilder sqlBuilder, string comment)
        {
            if (!string.IsNullOrEmpty(comment))
                sqlBuilder.Append(" COMMENT ").Append(comment.ToLiteral());

            return sqlBuilder;
        }

        private static void GenerateColumnDefinitionSql(this Column column, IndentedStringBuilder sqlBuilder, MySqlVersion mySqlVersion, string tableName, bool isTempTable, bool addComma)
        {
            var columnName = column.DbColumnName.ToQuotedIdentifier();
            sqlBuilder.Append(columnName).Append(' ');
            column.GetMySqlType().GenerateColumnDefinitionSql(sqlBuilder, tableName, isTempTable, mySqlVersion);
            if (addComma)
                sqlBuilder.Append(",");
            sqlBuilder.AppendLine();
        }

        private static int GenerateConstraints(this Model model, IndentedStringBuilder sqlBuilder, MySqlVersion mySqlVersion, string tableName, bool isTempTable)
        {
            IReadOnlyList<DbTableConstraint> constraints = model.GetAddons<DbTableConstraint>();
            if (isTempTable)
                constraints = constraints.Where(x => x.IsValidOnTempTable).ToList();
            else
                constraints = constraints.Where(x => x.IsValidOnTable).ToList();
            if (constraints.Count == 0)
                return 0;

            sqlBuilder.AppendLine();
            for (int i = 0; i < constraints.Count; i++)
            {
                if (i > 0)
                    sqlBuilder.AppendLine(",");
                var constraint = constraints[i];
                sqlBuilder.GenerateConstraintName(constraint.Name, tableName, isTempTable);
                if (constraint is DbPrimaryKey)
                    GeneratePrimaryKeyConstraint(sqlBuilder, (DbPrimaryKey)constraint);
                else if (constraint is DbUniqueConstraint)
                    GenerateUniqueConstraint(sqlBuilder, (DbUniqueConstraint)constraint);
                else if (constraint is DbCheckConstraint)
                    GenerateCheckConstraint(sqlBuilder, mySqlVersion, (DbCheckConstraint)constraint);
                else if (constraint is DbForeignKeyConstraint)
                    GenerateForeignKeyConstraint(sqlBuilder, (DbForeignKeyConstraint)constraint);
                else
                    throw new NotSupportedException(DiagnosticMessages.ConstraintTypeNotSupported(constraint.GetType()));
            }
            return constraints.Count;
        }

        private static void GenerateConstraintName(this IndentedStringBuilder sqlBuilder, string name, string tableName, bool isTempTable)
        {
            if (string.IsNullOrEmpty(name))
                return;

            sqlBuilder.Append("CONSTRAINT ");
            if (isTempTable)
                name += "_" + Guid.NewGuid().ToString();
            sqlBuilder.Append(name.FormatName(tableName));
            sqlBuilder.Append(' ');
        }

        private static void GeneratePrimaryKeyConstraint(IndentedStringBuilder sqlBuilder, DbPrimaryKey constraint)
        {
            Debug.Assert(constraint != null);

            sqlBuilder.Append("PRIMARY KEY (");
            GenerateColumnSortList(sqlBuilder, constraint.PrimaryKey);
            sqlBuilder.Append(")");
        }

        private static void GenerateUniqueConstraint(IndentedStringBuilder sqlBuilder, DbUniqueConstraint constraint)
        {
            Debug.Assert(constraint != null);

            sqlBuilder.Append("UNIQUE (");
            GenerateColumnSortList(sqlBuilder, constraint.Columns);
            sqlBuilder.Append(")");
        }

        private static void GenerateForeignKeyConstraint(IndentedStringBuilder sqlBuilder, DbForeignKeyConstraint constraint)
        {
            Debug.Assert(constraint != null);

            sqlBuilder.Append("FOREIGN KEY");
            constraint.ForeignKey.GenerateColumnList(sqlBuilder);
            sqlBuilder.Indent++;
            sqlBuilder.Append("REFERENCES ");
            sqlBuilder.Append(constraint.ReferencedTableName.ToQuotedIdentifier());
            constraint.ReferencedKey.GenerateColumnList(sqlBuilder);
            constraint.DeleteRule.GenerateForeignKeyRule(sqlBuilder, "DELETE");
            sqlBuilder.AppendLine();
            constraint.UpdateRule.GenerateForeignKeyRule(sqlBuilder, "UPDATE");
            sqlBuilder.Indent--;
        }

        private static void GenerateColumnList(this CandidateKey key, IndentedStringBuilder sqlBuilder)
        {
            sqlBuilder.Append(" (");
            for (int i = 0; i < key.Count; i++)
            {
                sqlBuilder.Append(key[i].Column.DbColumnName.ToQuotedIdentifier());
                if (i != key.Count - 1)
                    sqlBuilder.Append(", ");
            }
            sqlBuilder.AppendLine(")");
        }

        private static void GenerateForeignKeyRule(this Rule rule, IndentedStringBuilder sqlBuilder, string changeType)
        {
            Debug.Assert(changeType == "DELETE" || changeType == "UPDATE");
            sqlBuilder.Append("ON ");
            sqlBuilder.Append(changeType);
            if (rule == Rule.None)
                sqlBuilder.Append(" NO ACTION");
            else if (rule == Rule.Cascade)
                sqlBuilder.Append(" CASCADE");
            else if (rule == Rule.SetNull)
                sqlBuilder.Append(" SET NULL");
            else
                sqlBuilder.Append(" SET DEFAULT");
        }

        private static void GenerateCheckConstraint(IndentedStringBuilder sqlBuilder, MySqlVersion mySqlVersion, DbCheckConstraint constraint)
        {
            Debug.Assert(constraint != null);

            sqlBuilder.Append("CHECK ");
            constraint.LogicalExpression.GenerateSql(sqlBuilder, mySqlVersion);
        }

        private static int GenerateIndexes(this Model model, IndentedStringBuilder sqlBuilder, MySqlVersion mySqlVersion, string tableName, bool isTempTable, bool hasConstraint)
        {
            IReadOnlyList<DbIndex> indexes = model.GetAddons<DbIndex>();
            if (isTempTable)
                indexes = indexes.Where(x => x.IsValidOnTempTable).ToList();
            else
                indexes = indexes.Where(x => x.IsValidOnTable).ToList();
            if (indexes.Count == 0)
                return 0;

            for (int i = 0; i < indexes.Count; i++)
            {
                if (hasConstraint || i > 0)
                    sqlBuilder.AppendLine(",");
                else
                    sqlBuilder.AppendLine();
                var index = indexes[i];
                sqlBuilder.Append("INDEX ");
                sqlBuilder.Append(index.Name.FormatName(tableName));
                sqlBuilder.Append(' ');

                if (index.IsUnique)
                    sqlBuilder.Append("UNIQUE ");
                sqlBuilder.Append('(');
                GenerateColumnSortList(sqlBuilder, index.Columns);
                sqlBuilder.Append(')');
            }
            return indexes.Count;
        }

        private static void GenerateColumnSortList(IndentedStringBuilder sqlBuilder, IReadOnlyList<ColumnSort> columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i].Column;
                var sort = columns[i].Direction;
                sqlBuilder.Append(column.DbColumnName.ToQuotedIdentifier());
                if (sort == SortDirection.Ascending)
                    sqlBuilder.Append(" ASC");
                else if (sort == SortDirection.Descending)
                    sqlBuilder.Append(" DESC");
                if (i != columns.Count - 1)
                    sqlBuilder.Append(", ");
            }
        }
    }
}
