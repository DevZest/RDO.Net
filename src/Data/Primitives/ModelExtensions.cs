using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Provides extension methods of <see cref="Model"/> object.
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        /// Gets the database alias.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The database alias</returns>
        public static string GetDbAlias(this Model model)
        {
            return model.DbAlias;
        }

        /// <summary>
        /// Adds a system column into the model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="column">The column.</param>
        /// <param name="name">The name of column.</param>
        /// <param name="initializer">The column initializer.</param>
        public static void AddSystemColumn(this Model model, Column column, string name, Action<Column> initializer = null)
        {
            model.VerifyNotNull(nameof(model));
            column.Initialize(model, model.GetType(), name, ColumnKind.SystemCustom, initializer);
        }
        
        /// <summary>
        /// Gets database table clause.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The database table clause.</returns>
        public static DbTableClause GetDbTableClause(this Model model)
        {
            model.VerifyNotNull(nameof(model));
            var dbTable = model.DataSource as IDbTable;
            return dbTable == null ? null : new DbTableClause(model, dbTable.Name);
        }

        /// <summary>
        /// Gets the system row id column.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The column which is the system row id.</returns>
        public static _Int32 GetTableRowIdColumn(this Model model)
        {
            model.VerifyNotNull(nameof(model));
            return model.Columns[new ColumnId(model.GetType(), Model.SYS_ROW_ID_COL_NAME)] as _Int32;
        }
        
        /// <summary>
        /// Creates DbTable object.
        /// </summary>
        /// <typeparam name="T">Type of model.</typeparam>
        /// <param name="model">The model reference.</param>
        /// <param name="dbSession">The database session.</param>
        /// <param name="name">The name of the table.</param>
        /// <returns>The created DbTable object.</returns>
        public static DbTable<T> CreateDbTable<T>(this T model, DbSession dbSession, string name)
            where T : Model, new()
        {
            model.VerifyNotNull(nameof(model));
            dbSession.VerifyNotNull(nameof(dbSession));
            name.VerifyNotNull(nameof(name));
            var result = model.DataSource as DbTable<T>;
            return result ?? DbTable<T>.Create(model, dbSession, name);
        }

        /// <summary>
        /// Gets insertable columns.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The insertable columns.</returns>
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

        /// <summary>
        /// Gets updatable columns.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The updatable columns.</returns>
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
            where T : Model, new()
        {
            Debug.Assert(_ != null);
            if (initializer != null)
            {
                _.Initializer = r => initializer((T)r);
                initializer(_);
            }
        }

        internal static T MakeCopy<T>(this T prototype, bool setDataSource)
            where T : Model, new()
        {
            T result = new T();
            result.InitializeClone(prototype, setDataSource);
            return result;
        }

        internal static Model MakeCopy(this Model prototype, bool setDataSource)
        {
            var result = (Model)Activator.CreateInstance(prototype.GetType());
            result.InitializeClone(prototype, setDataSource);
            return result;
        }

        /// <summary>
        /// Gets the name of database table.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The name of database name.</returns>
        public static string GetDbTableName(this Model model)
        {
            return (model.DataSource as IDbTable)?.Name;
        }

        /// <summary>
        /// Gets the description of database table.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The description of database table.</returns>
        public static string GetDbTableDescription(this Model model)
        {
            return (model.DataSource as IDbTable)?.Description;
        }

        /// <summary>
        /// Determines whether identity column is suspended.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><see langword="true"/> if identity column is suspended, otherwise <see langword="false"/>.</returns>
        public static bool IsIdentitySuspended(this Model model)
        {
            return model.IsIdentitySuspended;
        }

        /// <summary>
        /// Suspend identity column of the model.
        /// </summary>
        /// <param name="model">The model.</param>
        public static void SuspendIdentity(this Model model)
        {
            model.SuspendIdentity();
        }

        /// <summary>
        /// Resumes identity column of the model.
        /// </summary>
        /// <param name="model">The model.</param>
        public static void ResumeIdentity(this Model model)
        {
            model.ResumeIdentity();
        }

        /// <summary>
        /// Gets the parent relationship.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The parent relationship.</returns>
        public static IReadOnlyList<ColumnMapping> GetParentRelationship(this Model model)
        {
            return model.ParentRelationship;
        }

        /// <summary>
        /// Gets the owner DataSource.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The owner DataSource</returns>
        public static DataSource GetDataSource(this Model model)
        {
            return model.DataSource;
        }

        /// <summary>
        /// Gets the all columns owned by this <see cref="Model"/>, including recursive child projection(s).
        /// </summary>
        /// <returns>All columns owned by this <see cref="Model"/>.</returns>
        public static IEnumerable<Column> GetAllColumns(this Model model)
        {
            return model.AllColumns;
        }

        /// <summary>
        /// Gets the validators for specified <see cref="Model"/>.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The vlidators.</returns>
        public static List<IValidator> GetValidators(this Model model)
        {
            return model.Validators;
        }
    }
}
