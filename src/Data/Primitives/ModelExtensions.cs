using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public static class ModelExtensions
    {
        public static string GetDbAlias(this Model model)
        {
            return model.DbAlias;
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

        public static DbTable<T> CreateDbTable<T>(this T modelRef, DbSession dbSession, string name)
            where T : class, IModelReference, new()
        {
            modelRef.VerifyNotNull(nameof(modelRef));
            dbSession.VerifyNotNull(nameof(dbSession));
            name.VerifyNotNull(nameof(name));
            var result = modelRef.Model.DataSource as DbTable<T>;
            return result ?? DbTable<T>.Create(modelRef, dbSession, name);
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
                if (column == identityColumn && !model.IsIdentitySuspended)
                    continue;

                if (isTable && column.IsDbComputed)
                    continue;

                yield return column;
            }
        }

        public static IEnumerable<Column> GetUpdatableColumns(this Model model)
        {
            model.VerifyNotNull(nameof(model));
            return model.GetUpdatableColumns(model.ParentRelationship);
        }

        private static IEnumerable<Column> GetUpdatableColumns(this Model model, IReadOnlyList<ColumnMapping> parentRelationship)
        {
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

                if (parentRelationship != null && parentRelationship.ContainsSource(column))
                    continue;

                yield return column;
            }
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

        public static bool IsIdentitySuspended(this Model model)
        {
            return model.IsIdentitySuspended;
        }

        public static void SuspendIdentity(this Model model)
        {
            model.SuspendIdentity();
        }

        public static void ResumeIdentity(this Model model)
        {
            model.ResumeIdentity();
        }

        public static IReadOnlyList<ColumnMapping> GetParentRelationship(this Model model)
        {
            return model.ParentRelationship;
        }

        public static DataSource GetDataSource(this Model model)
        {
            return model.DataSource;
        }
    }
}
