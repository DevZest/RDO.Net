using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevZest.Data.Primitives
{
    public static class ModelExtensions
    {
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

        public static IEnumerable<Column> GetInsertableColumns(this Model model)
        {
            Check.NotNull(model, nameof(model));

            IDbTable dbTable = (IDbTable)model.DataSource;
            bool isTempTable = dbTable == null ? false : dbTable.Kind == DataSourceKind.DbTempTable;
            bool isTable = dbTable == null ? false : dbTable.Kind == DataSourceKind.DbTable;
            var identity = dbTable == null ? null : model.GetIdentity(isTempTable);
            Column identityColumn = identity == null ? null : identity.Column;

            foreach (var column in model.Columns)
            {
                if (column == identityColumn)
                    continue;

                if (isTable && column.IsDbComputed)
                    continue;

                yield return column;
            }
        }

        public static IEnumerable<Column> GetUpdatableColumns(this Model model)
        {
            Check.NotNull(model, nameof(model));
            if (model.DataSource == null || model.DataSource.Kind != DataSourceKind.DbTempTable || model.ParentModel == null)
                return model.GetInsertableColumns();
            return model.GetUpdatableColumns(model.ParentRelationship);
        }

        private static IEnumerable<Column> GetUpdatableColumns(this Model model, IList<ColumnMapping> parentRelationship)
        {
            foreach (var column in model.GetInsertableColumns())
            {
                if (!parentRelationship.ContainsSource(column))
                    yield return column;
            }
        }

        public static IList<ColumnMapping> GetColumnMappings<TSource, TTarget>(this TTarget targetModel, TSource sourceModel, Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder, bool isInsertable)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            Check.NotNull(targetModel, nameof(targetModel));
            Check.NotNull(sourceModel, nameof(sourceModel));

            if (columnMappingsBuilder == null)
                return targetModel.GetColumnMappings(sourceModel, isInsertable);

            var result = new ColumnMappingsBuilder(sourceModel, targetModel).Build(builder => columnMappingsBuilder(builder, sourceModel, targetModel));
            var columns = isInsertable ? targetModel.GetInsertableColumns() : targetModel.GetUpdatableColumns();
            var columnKeys = new HashSet<ColumnKey>(columns.Select(x => x.Key));
            foreach (var resultItem in result)
            {
                if (!columnKeys.Contains(resultItem.Target.Key))
                    throw new InvalidOperationException(Strings.ColumnMappingsBuilder_InvalidTarget(resultItem.Target));
            }

            return result;
        }

        private static List<ColumnMapping> GetColumnMappings(this Model targetModel, Model sourceModel, bool isInsertable)
        {
            Check.NotNull(targetModel, nameof(targetModel));
            Check.NotNull(sourceModel, nameof(sourceModel));

            var result = new List<ColumnMapping>();
            var sourceColumns = sourceModel.Columns;
            var columns = isInsertable ? targetModel.GetInsertableColumns() : targetModel.GetUpdatableColumns();
            foreach (var column in columns)
            {
                if (column.IsSystem)
                    continue;
                var sourceColumn = sourceColumns[column.Key];
                if (sourceColumn != null)
                    result.Add(new ColumnMapping(sourceColumn, column));
            }

            return result;
        }

        public static void EnsureChildModelsInitialized(this Model model)
        {
            Check.NotNull(model, nameof(model));
            model.EnsureChildModelsInitialized();
        }

        public static ModelCollection GetChildModels(this Model model)
        {
            Check.NotNull(model, nameof(model));
            return model.ChildModels;
        }

        public static int GetOrdinal(this Model model)
        {
            Check.NotNull(model, nameof(model));
            return model.Ordinal;
        }

        public static DataSource GetDataSource(this Model model)
        {
            Check.NotNull(model, nameof(model));
            return model.DataSource;
        }

        public static DataSet GetDataSet(this Model model)
        {
            Check.NotNull(model, nameof(model));
            return model.DataSet;
        }

        public static int GetHierarchicalLevel(this Model model)
        {
            Check.NotNull(model, nameof(model));
            return model.HierarchicalLevel;
        }
    }
}
