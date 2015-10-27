using DevZest.Data.Resources;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public static class ModelExtensions
    {
        public static IList<DataRow> GetDataSet(this Model model, DataRow parentDataRow)
        {
            var parentModel = model.ParentModel;
            var parentDataRowModel = parentDataRow == null ? null : parentDataRow.Model;
            if (parentModel != parentDataRowModel)
                throw new ArgumentException(Strings.InvalidChildModel, nameof(parentDataRow));

            return parentDataRowModel == null ? model.DataSet : parentDataRow.GetChildren(model);
        }

        public static ColumnCollection GetColumns(this Model model)
        {
            return model.Columns;
        }

        public static string GetDbAlias(this Model model)
        {
            return model.DbAlias;
        }

        public static Identity GetIdentity(this Model model, bool isTempTable)
        {
            return model.GetIdentity(isTempTable);
        }

        public static void AddSystemColumn(this Model model, Column column, string name, Action<Column> initializer = null)
        {
            Check.NotNull(model, nameof(model));
            column.Initialize(model, model.GetType(), name, ColumnKind.SystemCustom, initializer);
        }

        public static DbTableClause GetDbTableClause(this Model model)
        {
            Check.NotNull(model, nameof(model));
            var dbTable = model.DataSource as IDbTable;
            return dbTable == null ? null : new DbTableClause(model, dbTable.Name);
        }

        public static _Int32 GetTableRowIdColumn(this Model model)
        {
            Check.NotNull(model, nameof(model));
            return model.Columns[new ColumnKey(model.GetType(), Model.SYS_ROW_ID_COL_NAME)] as _Int32;
        }

        public static DbTable<T> CreateDbTable<T>(this T model, DbSession dbSession, string name)
            where T : Model, new()
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(dbSession, nameof(dbSession));
            Check.NotNull(name, nameof(name));
            var result = model.DataSource as DbTable<T>;
            return result ?? DbTable<T>.Create(model, dbSession, name);
        }

        public static IEnumerable<Column> GetUpdatableColumns(this Model model)
        {
            IDbTable dbTable = (IDbTable)model.DataSource;
            bool isTempTable = dbTable.Kind == DataSourceKind.DbTempTable;
            var identity = model.GetIdentity(dbTable.Kind == DataSourceKind.DbTempTable);
            Column identityColumn = identity == null ? null : identity.Column;

            foreach (var column in model.Columns)
            {
                if (column == identityColumn)
                    continue;

                if (!isTempTable && column.GetComputation() != null)
                    continue;

                yield return column;
            }
        }

        internal static IEnumerable<Column> GetSelectColumns(this Model model)
        {
            foreach (var column in model.Columns)
            {
                if (column.Kind != ColumnKind.SystemRowId)
                    yield return column;
            }
        }
    }
}
