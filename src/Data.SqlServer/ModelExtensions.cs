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
            {
                var column = columns[i];
                column.GenerateColumnDefinitionSql(sqlBuilder, sqlVersion, tableName, isTempTable, i == columns.Count - 1);
            }

            int countConstraints = model.GenerateConstraints(sqlBuilder, sqlVersion, tableName, isTempTable);
            int countIndexes = model.GenerateIndexes(sqlBuilder, sqlVersion, tableName, isTempTable, countConstraints > 0);
            if (countConstraints + countIndexes > 0)
                sqlBuilder.AppendLine();

            sqlBuilder.Indent--;
            sqlBuilder.AppendLine(");");

            if (!isTempTable)
                model.GenerateDescriptionSql(sqlBuilder, sqlVersion, tableName);
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
                if (constraint is DbPrimaryKey)
                    GeneratePrimaryKeyConstraint(sqlBuilder, (DbPrimaryKey)constraint);
                else if (constraint is DbUnique)
                    GenerateUniqueConstraint(sqlBuilder, (DbUnique)constraint);
                else if (constraint is DbCheck)
                    GenerateCheckConstraint(sqlBuilder, sqlVersion, (DbCheck)constraint);
                else if (constraint is DbForeignKey)
                    GenerateForeignKeyConstraint(sqlBuilder, (DbForeignKey)constraint);
                else
                    throw new NotSupportedException(Strings.ConstraintTypeNotSupported(constraint.GetType().FullName));
            }
            return constraints.Count;
        }

        private static void GeneratePrimaryKeyConstraint(IndentedStringBuilder sqlBuilder, DbPrimaryKey constraint)
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

        private static void GenerateUniqueConstraint(IndentedStringBuilder sqlBuilder, DbUnique constraint)
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

        private static void GenerateForeignKeyConstraint(IndentedStringBuilder sqlBuilder, DbForeignKey constraint)
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

        private static void GenerateCheckConstraint(IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, DbCheck constraint)
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

        private static void GenerateDescriptionSql(this Model model, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string tableName)
        {
            var parsedIdentifiers = tableName.ParseIdentifier();
            if (parsedIdentifiers.Count > 2)
                return;
            var table = parsedIdentifiers[parsedIdentifiers.Count - 1];
            var schema = parsedIdentifiers.Count > 1 ? parsedIdentifiers[parsedIdentifiers.Count - 2] : null;
            if (string.IsNullOrEmpty(schema))
                schema = "dbo";

            var columns = model.GetColumns();
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                column.GenerateColumnDescriptionSql(sqlBuilder, sqlVersion, schema, table);
            }

            var constraints = model.GetExtensions<DbTableConstraint>().Where(x => x.IsMemberOfTable).ToList();
            for (int i = 0; i < constraints.Count; i++)
            {
                var constraint = constraints[i];
                constraint.GenerateConstraintDescriptionSql(sqlBuilder, sqlVersion, schema, table);
            }

            var indexes = model.GetExtensions<DbIndex>().Where(x => x.IsMemberOfTable).ToList();
            for (int i = 0; i < indexes.Count; i++)
            {
                var index = indexes[i];
                index.GenerateIndexDescriptionSql(sqlBuilder, sqlVersion, schema, table);
            }
        }

        private static void GenerateColumnDescriptionSql(this Column column, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string schema, string table)
        {
            var description = column.DbColumnDescription;
            if (string.IsNullOrEmpty(description))
                return;
            sqlBuilder.GenerateDescriptionSql(sqlVersion, schema, table, "COLUMN", column.DbColumnName, description);
        }

        private static void GenerateConstraintDescriptionSql(this DbTableConstraint constraint, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string schema, string table)
        {
            var description = constraint.Description;
            if (string.IsNullOrEmpty(constraint.Name) || string.IsNullOrEmpty(description))
                return;
            sqlBuilder.GenerateDescriptionSql(sqlVersion, schema, table, "CONSTRAINT", constraint.Name.FormatName(table, false), description);
        }

        private static void GenerateIndexDescriptionSql(this DbIndex index, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string schema, string table)
        {
            var description = index.Description;
            if (string.IsNullOrEmpty(index.Name) || string.IsNullOrEmpty(description))
                return;
            sqlBuilder.GenerateDescriptionSql(sqlVersion, schema, table, "INDEX", index.Name.FormatName(table, false), description);
        }

        private static void GenerateDescriptionSql(this IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string schema, string table, string level2Type, string level2Name, string value)
        {
            sqlBuilder.GenerateExtendedPropertySql(sqlVersion, schema, table, level2Type, level2Name, "MS_Description", value);
        }

        private static void GenerateExtendedPropertySql(this IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string schema, string table, string level2Type, string level2Name, string name, string value)
        {
            Debug.Assert(!string.IsNullOrEmpty(value));
            if (string.IsNullOrEmpty(schema))
                schema = "dbo";
            Debug.Assert(!string.IsNullOrEmpty(table));
            Debug.Assert(!string.IsNullOrEmpty(level2Type));
            Debug.Assert(!string.IsNullOrEmpty(level2Name));

            sqlBuilder.Append(@"EXEC sp_addextendedproperty @name = ").Append(name.ToTSqlLiteral(true)).Append(", ");
            sqlBuilder.Append(@"@value = ").Append(value.ToTSqlLiteral(true)).Append(", ");
            sqlBuilder.Append(@"@level0type = N'Schema', @level0name = ").Append(schema.ToTSqlLiteral(true)).Append(", ");
            sqlBuilder.Append(@"@level1type = N'Table', @level1name = ").Append(table.ToTSqlLiteral(true)).Append(", ");
            sqlBuilder.Append(@"@level2type = ").Append(level2Type.ToTSqlLiteral(true)).Append(@", @level2name = ").Append(level2Name.ToTSqlLiteral(true)).AppendLine(";");
        }
    }
}
