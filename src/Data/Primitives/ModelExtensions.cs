using DevZest.Data.Addons;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public static class ModelExtensions
    {
        public static ColumnCollection GetColumns(this Model model)
        {
            return model.Columns;
        }

        public static IReadOnlyList<Column> GetLocalColumns(this Model model)
        {
            return model.LocalColumns;
        }

        public static IReadOnlyList<ColumnList> GetColumnLists(this Model model)
        {
            return model.ColumnLists;
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
            model.VerifyNotNull(nameof(model));
            column.Initialize(model, model.GetType(), name, ColumnKind.SystemCustom, initializer);
        }

        public static DbTableClause GetDbTableClause(this Model model)
        {
            model.VerifyNotNull(nameof(model));
            var dbTable = model.DataSource as IDbTable;
            return dbTable == null ? null : new DbTableClause(model, dbTable.Name);
        }

        public static _Int32 GetTableRowIdColumn(this Model model)
        {
            model.VerifyNotNull(nameof(model));
            return model.Columns[new ColumnId(model.GetType(), Model.SYS_ROW_ID_COL_NAME)] as _Int32;
        }

        public static DbTable<T> CreateDbTable<T>(this T model, DbSession dbSession, string name)
            where T : Model, new()
        {
            model.VerifyNotNull(nameof(model));
            dbSession.VerifyNotNull(nameof(dbSession));
            name.VerifyNotNull(nameof(name));
            var result = model.DataSource as DbTable<T>;
            return result ?? DbTable<T>.Create(model, dbSession, name);
        }

        public static IEnumerable<Column> GetInsertableColumns(this Model model)
        {
            model.VerifyNotNull(nameof(model));

            IDbTable dbTable = model.DataSource as IDbTable;
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
            model.VerifyNotNull(nameof(model));
            if (model.DataSource == null || model.DataSource.Kind != DataSourceKind.DbTempTable || model.ParentModel == null)
                return model.GetInsertableColumns();
            return model.GetUpdatableColumns(model.ParentRelationship);
        }

        private static IEnumerable<Column> GetUpdatableColumns(this Model model, IReadOnlyList<ColumnMapping> parentRelationship)
        {
            foreach (var column in model.GetInsertableColumns())
            {
                if (!parentRelationship.ContainsSource(column))
                    yield return column;
            }
        }

        public static ModelCollection GetChildModels(this Model model)
        {
            model.VerifyNotNull(nameof(model));
            return model.ChildModels;
        }

        public static int GetOrdinal(this Model model)
        {
            model.VerifyNotNull(nameof(model));
            return model.Ordinal;
        }

        public static DataSource GetDataSource(this Model model)
        {
            model.VerifyNotNull(nameof(model));
            return model.DataSource;
        }

        public static DataSet GetDataSet(this Model model)
        {
            model.VerifyNotNull(nameof(model));
            return model.DataSet;
        }

        public static int GetDepth(this Model model)
        {
            model.VerifyNotNull(nameof(model));
            return model.Depth;
        }

        internal static void Initialize<T>(this T _, Action<T> initializer)
            where T : class, IModelReference, new()
        {
            Debug.Assert(_ != null);
            if (initializer != null)
            {
                _.Model.Initializer = r => initializer((T)r);
                initializer(_);
            }
        }

        internal static T MakeCopy<T>(this T prototype, bool setDataSource)
            where T : class, IModelReference, new()
        {
            T result = new T();
            result.Model.InitializeClone(prototype.Model, setDataSource);
            return result;
        }

        internal static IModelReference MakeCopy(this IModelReference prototype, bool setDataSource)
        {
            var result = (IModelReference)Activator.CreateInstance(prototype.GetType());
            result.Model.InitializeClone(prototype.Model, setDataSource);
            return result;
        }

        public static string GetDbTableName(this Model model)
        {
            return (model.DataSource as IDbTable)?.Name;
        }

        public static string GetDbTableDescription(this Model model)
        {
            return (model.DataSource as IDbTable)?.Description;
        }
    }
}
