using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace DevZest.Data.SqlServer
{
    internal static class ModelExtensions
    {
        internal static void GenerateCreateTableSql(this Model model, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string tableName, bool isTempTable)
        {
            tableName = tableName.ToQuotedIdentifier();

            sqlBuilder.Append("CREATE TABLE ");
            sqlBuilder.Append(tableName);
            sqlBuilder.AppendLine(" (");
            sqlBuilder.Indent++;

            var columns = model.GetColumns();
            for (int i = 0; i < columns.Count; i++)
                columns[i].GenerateColumnDefinitionSql(sqlBuilder, sqlVersion, isTempTable, i == columns.Count - 1);

            model.GenerateConstraints(sqlBuilder, sqlVersion, tableName, isTempTable);

            sqlBuilder.Indent--;
            sqlBuilder.AppendLine(");");
        }

        private static void GenerateColumnDefinitionSql(this Column column, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, bool isTempTable, bool isLastColumn)
        {
            var columnName = column.DbColumnName.ToQuotedIdentifier();
            sqlBuilder.Append(columnName).Append(' ');
            column.GetMapper().GenerateColumnDefinitionSql(sqlBuilder, isTempTable, sqlVersion);
            if (!isLastColumn)
                sqlBuilder.Append(",");
            sqlBuilder.AppendLine();
        }

        private static void GenerateConstraints(this Model model, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string tableName, bool isTempTable)
        {
            IReadOnlyList<DbTableConstraint> constraints = model.GetResources<DbTableConstraint>();
            if (isTempTable)
                constraints = constraints.Where(x => !(x is ForeignKeyConstraint)).ToList();
            if (constraints.Count == 0)
                return;

            sqlBuilder.AppendLine();
            for (int i = 0; i < constraints.Count; i++)
            {
                var constraint = constraints[i];
                if (!isTempTable && !string.IsNullOrEmpty(constraint.Name))
                {
                    sqlBuilder.Append("CONSTRAINT ");
                    sqlBuilder.Append(constraint.Name.ToQuotedIdentifier());
                    sqlBuilder.Append(' ');
                }
                if (constraint is PrimaryKeyConstraint)
                    GeneratePrimaryKeyConstraint(sqlBuilder, (PrimaryKeyConstraint)constraint);
                else if (constraint is UniqueConstraint)
                    GenerateUniqueConstraint(sqlBuilder, (UniqueConstraint)constraint);
                else if (constraint is CheckConstraint)
                    GenerateCheckConstraint(sqlBuilder, sqlVersion, (CheckConstraint)constraint);
                else if (constraint is ForeignKeyConstraint)
                    GenerateForeignKeyConstraint(sqlBuilder, (ForeignKeyConstraint)constraint);
                else
                    throw new NotSupportedException(Strings.ConstraintTypeNotSupported(constraint.GetType().FullName));

                bool isLastConstraint = (i == constraints.Count - 1);
                if (!isLastConstraint)
                    sqlBuilder.Append(",");
                sqlBuilder.AppendLine();
            }
        }

        private static void GeneratePrimaryKeyConstraint(IndentedStringBuilder sqlBuilder, PrimaryKeyConstraint constraint)
        {
            Debug.Assert(constraint != null);

            sqlBuilder.Append("PRIMARY KEY ");
            if (constraint.IsClustered)
                sqlBuilder.Append("CLUSTERED (");
            else
                sqlBuilder.Append("NONCLUSTERED (");
            var primaryKey = constraint.PrimaryKey;
            for (int i = 0; i < primaryKey.Count; i++)
            {
                var column = primaryKey[i].Column;
                var sort = primaryKey[i].Direction;
                sqlBuilder.Append(column.DbColumnName.ToQuotedIdentifier());
                if (sort == SortDirection.Ascending)
                    sqlBuilder.Append(" ASC");
                else if (sort == SortDirection.Descending)
                    sqlBuilder.Append(" DESC");
                if (i != primaryKey.Count - 1)
                    sqlBuilder.Append(", ");
            }
            sqlBuilder.Append(")");
        }

        private static void GenerateUniqueConstraint(IndentedStringBuilder sqlBuilder, UniqueConstraint constraint)
        {
            Debug.Assert(constraint != null);

            sqlBuilder.Append("UNIQUE ");
            if (constraint.IsClustered)
                sqlBuilder.Append("CLUSTERED (");
            else
                sqlBuilder.Append("NONCLUSTERED (");
            var columns = constraint.Columns;
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
            sqlBuilder.Append(")");
        }

        private static void GenerateForeignKeyConstraint(IndentedStringBuilder sqlBuilder, ForeignKeyConstraint constraint)
        {
            Debug.Assert(constraint != null);

            sqlBuilder.Append("FOREIGN KEY");
            constraint.ForeignKey.GenerateColumnList(sqlBuilder);
            sqlBuilder.Indent++;
            sqlBuilder.Append("REFERENCES ");
            sqlBuilder.Append(constraint.ReferencedTableName.ToQuotedIdentifier());
            constraint.ReferencedKey.GenerateColumnList(sqlBuilder);
            constraint.OnDelete.GenerateForeignKeyAction(sqlBuilder, "DELETE");
            sqlBuilder.AppendLine();
            constraint.OnUpdate.GenerateForeignKeyAction(sqlBuilder, "UPDATE");
            sqlBuilder.Indent--;
        }

        private static void GenerateColumnList(this PrimaryKey key, IndentedStringBuilder sqlBuilder)
        {
            sqlBuilder.Append(" (");
            for (int i = 0; i < key.Count(); i++)
            {
                sqlBuilder.Append(key[i].Column.DbColumnName.ToQuotedIdentifier());
                if (i != key.Count() - 1)
                    sqlBuilder.Append(", ");
            }
            sqlBuilder.AppendLine(")");
        }

        private static void GenerateForeignKeyAction(this ForeignKeyAction action, IndentedStringBuilder sqlBuilder, string changeType)
        {
            Debug.Assert(changeType == "DELETE" || changeType == "UPDATE");
            sqlBuilder.Append("ON ");
            sqlBuilder.Append(changeType);
            if (action == ForeignKeyAction.NoAction)
                sqlBuilder.Append(" NO ACTION");
            else if (action == ForeignKeyAction.Cascade)
                sqlBuilder.Append(" CASCADE");
            else if (action == ForeignKeyAction.SetNull)
                sqlBuilder.Append(" SET NULL");
            else
                sqlBuilder.Append(" SET DEFAULT");
        }

        private static void GenerateCheckConstraint(IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, CheckConstraint constraint)
        {
            Debug.Assert(constraint != null);
            
            sqlBuilder.Append("CHECK ");
            constraint.LogicalExpression.GenerateSql(sqlBuilder, sqlVersion);
        }
    }
}
