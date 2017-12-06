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
            bool anyDescription = false;
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                column.GenerateColumnDefinitionSql(sqlBuilder, sqlVersion, tableName, isTempTable, i == columns.Count - 1);
                if (!string.IsNullOrEmpty(column.Description))
                    anyDescription = true;
            }

            int countConstraints = model.GenerateConstraints(sqlBuilder, sqlVersion, tableName, isTempTable);
            int countIndexes = model.GenerateIndexes(sqlBuilder, sqlVersion, tableName, isTempTable, countConstraints > 0);
            if (countConstraints + countIndexes > 0)
                sqlBuilder.AppendLine();

            sqlBuilder.Indent--;
            sqlBuilder.AppendLine(");");

            if (!isTempTable && anyDescription)
                columns.GenerateColumnsDescriptionSql(sqlBuilder, sqlVersion, tableName);
        }

        private static void GenerateColumnDefinitionSql(this Column column, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string tableName, bool isTempTable, bool isLastColumn)
        {
            var columnName = column.DbColumnName.ToQuotedIdentifier();
            sqlBuilder.Append(columnName).Append(' ');
            column.GetMapper().GenerateColumnDefinitionSql(sqlBuilder, tableName, isTempTable, sqlVersion);
            if (!isLastColumn)
                sqlBuilder.Append(",");
            sqlBuilder.AppendLine();
        }

        private static int GenerateConstraints(this Model model, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string tableName, bool isTempTable)
        {
            IReadOnlyList<DbTableConstraint> constraints = model.GetExtensions<DbTableConstraint>();
            if (isTempTable)
                constraints = constraints.Where(x => x.IsMemberOfTempTable).ToList();
            else
                constraints = constraints.Where(x => x.IsMemberOfTable).ToList();
            if (constraints.Count == 0)
                return 0;

            sqlBuilder.AppendLine();
            for (int i = 0; i < constraints.Count; i++)
            {
                if (i > 0)
                    sqlBuilder.AppendLine(",");
                var constraint = constraints[i];
                if (!isTempTable && !string.IsNullOrEmpty(constraint.Name))
                {
                    sqlBuilder.Append("CONSTRAINT ");
                    sqlBuilder.Append(constraint.Name.FormatName(tableName));
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
            }
            return constraints.Count;
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
            GenerateColumnSortList(sqlBuilder, constraint.Columns);
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

        private static int GenerateIndexes(this Model model, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string tableName, bool isTempTable, bool hasConstraint)
        {
            IReadOnlyList<DbIndex> indexes = model.GetExtensions<DbIndex>();
            if (isTempTable)
                indexes = indexes.Where(x => x.IsMemberOfTempTable).ToList();
            else
                indexes = indexes.Where(x => x.IsMemberOfTable).ToList();
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
                if (index.IsClustered)
                    sqlBuilder.Append("CLUSTERED (");
                else
                    sqlBuilder.Append("NONCLUSTERED (");
                GenerateColumnSortList(sqlBuilder, index.Columns);
                sqlBuilder.Append(")");
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

        private static void GenerateColumnsDescriptionSql(this IReadOnlyList<Column> columns, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string tableName)
        {
            var parsedIdentifiers = tableName.ParseIdentifier();
            if (parsedIdentifiers.Count > 2)
                return;
            var table = parsedIdentifiers[parsedIdentifiers.Count - 1];
            var schema = parsedIdentifiers.Count > 1 ? parsedIdentifiers[parsedIdentifiers.Count - 2] : null;
            for (int i = 0; i < columns.Count; i++)
                columns[i].GenerateColumnDescriptionSql(sqlBuilder, sqlVersion, schema, table);
        }

        private static void GenerateColumnDescriptionSql(this Column column, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string schema, string table)
        {
            var description = column.Description;
            if (!string.IsNullOrEmpty(description))
                sqlBuilder.GenerateColumnExtendedPropertySql(sqlVersion, schema, table, column.DbColumnName, "MS_Description", description);
        }

        private static void GenerateColumnExtendedPropertySql(this IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string schema, string table, string columnName, string name, string value)
        {
            Debug.Assert(!string.IsNullOrEmpty(value));
            if (string.IsNullOrEmpty(schema))
                schema = "dbo";
            Debug.Assert(!string.IsNullOrEmpty(table));
            Debug.Assert(!string.IsNullOrEmpty(columnName));

            sqlBuilder.Append(@"EXEC sp_addextendedproperty @name = ").Append(name.ToTSqlLiteral(true)).Append(", ");
            sqlBuilder.Append(@"@value = ").Append(value.ToTSqlLiteral(true)).Append(", ");
            sqlBuilder.Append(@"@level0type = N'Schema', @level0name = ").Append(schema.ToTSqlLiteral(true)).Append(", ");
            sqlBuilder.Append(@"@level1type = N'Table', @level1name = ").Append(table.ToTSqlLiteral(true)).Append(", ");
            sqlBuilder.Append(@"@level2type = N'Column', @level2name = ").Append(columnName.ToTSqlLiteral(true)).AppendLine(";");
        }
    }
}
