using DevZest.Data.Addons;
using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.SqlServer
{
    internal static class ModelExtensions
    {
        internal static void GenerateCreateTableSql(this Model model, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, bool isTempTable)
        {
            model.GenerateCreateTableSql(model.GetDbTableName(), model.GetDbTableDescription(), sqlBuilder, sqlVersion, isTempTable);
        }

        internal static void GenerateCreateTableSql(this Model model, string tableName, string description, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, bool isTempTable)
        {
            tableName = tableName.ToQuotedIdentifier();

            sqlBuilder.Append("CREATE TABLE ");
            sqlBuilder.Append(tableName);
            sqlBuilder.AppendLine(" (");
            sqlBuilder.IndentLevel++;

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

            sqlBuilder.IndentLevel--;
            sqlBuilder.AppendLine(");");

            if (!isTempTable)
                model.GenerateDescriptionSql(sqlBuilder, sqlVersion, tableName, description);
        }

        private static void GenerateColumnDefinitionSql(this Column column, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string tableName, bool isTempTable, bool isLastColumn)
        {
            var columnName = column.DbColumnName.ToQuotedIdentifier();
            sqlBuilder.Append(columnName).Append(' ');
            column.GetSqlType().GenerateColumnDefinitionSql(sqlBuilder, tableName, isTempTable, sqlVersion);
            if (!isLastColumn)
                sqlBuilder.Append(",");
            sqlBuilder.AppendLine();
        }

        private static int GenerateConstraints(this Model model, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string tableName, bool isTempTable)
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
                    GenerateCheckConstraint(sqlBuilder, sqlVersion, (DbCheckConstraint)constraint);
                else if (constraint is DbForeignKeyConstraint)
                    GenerateForeignKeyConstraint(sqlBuilder, (DbForeignKeyConstraint)constraint);
                else
                    throw new NotSupportedException(DiagnosticMessages.ConstraintTypeNotSupported(constraint.GetType().FullName));
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

            sqlBuilder.Append("PRIMARY KEY ");
            GenerateColumnSortList(sqlBuilder, constraint.IsClustered, constraint.PrimaryKey);
            sqlBuilder.Append(")");
        }

        private static void GenerateUniqueConstraint(IndentedStringBuilder sqlBuilder, DbUniqueConstraint constraint)
        {
            Debug.Assert(constraint != null);

            sqlBuilder.Append("UNIQUE ");
            GenerateColumnSortList(sqlBuilder, constraint.IsClustered, constraint.Columns);
            sqlBuilder.Append(")");
        }

        private static void GenerateForeignKeyConstraint(IndentedStringBuilder sqlBuilder, DbForeignKeyConstraint constraint)
        {
            Debug.Assert(constraint != null);

            sqlBuilder.Append("FOREIGN KEY");
            constraint.ForeignKey.GenerateColumnList(sqlBuilder);
            sqlBuilder.IndentLevel++;
            sqlBuilder.Append("REFERENCES ");
            sqlBuilder.Append(constraint.ReferencedTableName.ToQuotedIdentifier());
            constraint.ReferencedKey.GenerateColumnList(sqlBuilder);
            constraint.DeleteRule.GenerateForeignKeyRule(sqlBuilder, "DELETE");
            sqlBuilder.AppendLine();
            constraint.UpdateRule.GenerateForeignKeyRule(sqlBuilder, "UPDATE");
            sqlBuilder.IndentLevel--;
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

        private static void GenerateForeignKeyRule(this ForeignKeyRule rule, IndentedStringBuilder sqlBuilder, string changeType)
        {
            Debug.Assert(changeType == "DELETE" || changeType == "UPDATE");
            sqlBuilder.Append("ON ");
            sqlBuilder.Append(changeType);
            if (rule == ForeignKeyRule.None)
                sqlBuilder.Append(" NO ACTION");
            else if (rule == ForeignKeyRule.Cascade)
                sqlBuilder.Append(" CASCADE");
            else if (rule == ForeignKeyRule.SetNull)
                sqlBuilder.Append(" SET NULL");
            else
                sqlBuilder.Append(" SET DEFAULT");
        }

        private static void GenerateCheckConstraint(IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, DbCheckConstraint constraint)
        {
            Debug.Assert(constraint != null);
            
            sqlBuilder.Append("CHECK ");
            constraint.LogicalExpression.GenerateSql(sqlBuilder, sqlVersion);
        }

        private static int GenerateIndexes(this Model model, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string tableName, bool isTempTable, bool hasConstraint)
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
                GenerateColumnSortList(sqlBuilder, index.IsClustered, index.Columns);
                sqlBuilder.Append(")");
            }
            return indexes.Count;
        }

        private static void GenerateColumnSortList(IndentedStringBuilder sqlBuilder, bool isClustered, IReadOnlyList<ColumnSort> columns)
        {
            if (isClustered)
                sqlBuilder.Append("CLUSTERED (");
            else
                sqlBuilder.Append("NONCLUSTERED (");

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

        private static void GenerateDescriptionSql(this Model model, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string tableName, string tableDescription)
        {
            var parsedIdentifiers = tableName.ParseIdentifier();
            if (parsedIdentifiers.Count > 2)
                return;
            var table = parsedIdentifiers[parsedIdentifiers.Count - 1];
            var schema = parsedIdentifiers.Count > 1 ? parsedIdentifiers[parsedIdentifiers.Count - 2] : null;
            if (string.IsNullOrEmpty(schema))
                schema = "dbo";

            sqlBuilder.GenerateTableDescriptionSql(sqlVersion, schema, table, tableDescription);

            var columns = model.GetColumns();
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                column.GenerateColumnDescriptionSql(sqlBuilder, sqlVersion, schema, table);
                var columnDefault = column.GetDefault();
                if (columnDefault != null && !column.IsDbComputed)
                    columnDefault.GenerateColumnDefaultDescriptionSql(sqlBuilder, sqlVersion, schema, table);

            }

            var constraints = model.GetAddons<DbTableConstraint>().Where(x => x.IsValidOnTable).ToList();
            for (int i = 0; i < constraints.Count; i++)
            {
                var constraint = constraints[i];
                constraint.GenerateConstraintDescriptionSql(sqlBuilder, sqlVersion, schema, table);
            }

            var indexes = model.GetAddons<DbIndex>().Where(x => x.IsValidOnTable).ToList();
            for (int i = 0; i < indexes.Count; i++)
            {
                var index = indexes[i];
                index.GenerateIndexDescriptionSql(sqlBuilder, sqlVersion, schema, table);
            }
        }

        private static void GenerateTableDescriptionSql(this IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string schema, string table, string description)
        {
            if (!string.IsNullOrEmpty(description))
                sqlBuilder.GenerateDescriptionSql(sqlVersion, schema, table, null, null, description);
        }

        private static void GenerateColumnDescriptionSql(this Column column, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string schema, string table)
        {
            var description = column.DbColumnDescription;
            if (string.IsNullOrEmpty(description))
                return;
            sqlBuilder.GenerateDescriptionSql(sqlVersion, schema, table, "COLUMN", column.DbColumnName, description);
        }

        private static void GenerateColumnDefaultDescriptionSql(this ColumnDefault columnDefault, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string schema, string table)
        {
            if (string.IsNullOrEmpty(columnDefault.Name))
                return;

            var description = columnDefault.Description;
            if (string.IsNullOrEmpty(description))
                return;

            sqlBuilder.GenerateDescriptionSql(sqlVersion, schema, table, "CONSTRAINT", columnDefault.Name.FormatName(table, false), description);
        }

        private static void GenerateConstraintDescriptionSql(this DbTableConstraint constraint, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string schema, string table)
        {
            if (string.IsNullOrEmpty(constraint.Name))
                return;

            var description = constraint.Description;
            if (string.IsNullOrEmpty(description))
                return;

            sqlBuilder.GenerateDescriptionSql(sqlVersion, schema, table, "CONSTRAINT", constraint.Name.FormatName(table, false), description);
        }

        private static void GenerateIndexDescriptionSql(this DbIndex index, IndentedStringBuilder sqlBuilder, SqlVersion sqlVersion, string schema, string table)
        {
            if (string.IsNullOrEmpty(index.Name))
                return;

            var description = index.Description;
            if (string.IsNullOrEmpty(description))
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

            sqlBuilder.Append(@"EXEC sp_addextendedproperty @name = ").Append(name.ToTSqlLiteral(true)).Append(", ");
            sqlBuilder.Append(@"@value = ").Append(value.ToTSqlLiteral(true)).Append(", ");
            sqlBuilder.Append(@"@level0type = N'Schema', @level0name = ").Append(schema.ToTSqlLiteral(true)).Append(", ");
            sqlBuilder.Append(@"@level1type = N'Table', @level1name = ").Append(table.ToTSqlLiteral(true)).Append(", ");
            sqlBuilder.Append(@"@level2type = ").Append(level2Type.ToTSqlLiteral(true)).Append(@", @level2name = ").Append(level2Name.ToTSqlLiteral(true)).AppendLine(";");
        }
    }
}
