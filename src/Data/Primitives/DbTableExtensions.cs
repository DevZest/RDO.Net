using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Provides extension methods for database table object.
    /// </summary>
    public static class DbTableExtensions
    {
        /// <summary>
        /// Builds INSERT query statement.
        /// </summary>
        /// <typeparam name="TSource">Entity type of source DbSet.</typeparam>
        /// <typeparam name="TTarget">Entity type of target DbTable.</typeparam>
        /// <param name="target">The target DbTable.</param>
        /// <param name="source">The source DbSet.</param>
        /// <param name="columnMapper">Provides column mappings between source DbSet and target DbTable.</param>
        /// <returns>The query statement.</returns>
        public static DbSelectStatement BuildInsertStatement<TSource, TTarget>(this DbTable<TTarget> target, DbSet<TSource> source,
            Action<ColumnMapper, TSource, TTarget> columnMapper)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new()
        {
            target.VerifyNotNull(nameof(target));
            source.VerifyNotNull(nameof(source));
            var columnMappings = target.Verify(columnMapper, nameof(columnMapper), source._);

            return target.BuildInsertStatement(source, columnMappings);
        }

        /// <summary>
        /// Builds INSERT query statement.
        /// </summary>
        /// <typeparam name="TSource">Entity type of source DbSet.</typeparam>
        /// <typeparam name="TTarget">Entity type of target DbTable.</typeparam>
        /// <param name="target">The target DbTable.</param>
        /// <param name="source">The source DbSet.</param>
        /// <param name="columnMappings">Column mappings between source DbSet and target DbTable.</param>
        /// <returns>The query statement.</returns>
        public static DbSelectStatement BuildInsertStatement<TSource, TTarget>(this DbTable<TTarget> target, DbSet<TSource> source,
            IReadOnlyList<ColumnMapping> columnMappings)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new()
        {
            target.VerifyNotNull(nameof(target));
            source.VerifyNotNull(nameof(source));
            Verify(columnMappings, nameof(columnMappings), source.Model, target.Model);

            return target.BuildInsertStatement(source, columnMappings);
        }

        private static void Verify(IReadOnlyList<ColumnMapping> columnMappings, string paramName, Model source, Model target)
        {
            columnMappings.VerifyNotEmpty(paramName);
            for (int i = 0; i < columnMappings.Count; i++)
            {
                var columnMapping = columnMappings[i];
                VerifySource(columnMapping, paramName, i, source);
                VerifyTarget(columnMapping, paramName, i, target);
            }
        }

        private static void VerifySource(ColumnMapping mapping, string paramName, int index, Model source)
        {
            var sourceColumn = mapping.Source;
            if (sourceColumn == null)
                throw new ArgumentNullException(string.Format("{0}[{1}].{2}", paramName, index, nameof(ColumnMapping.Source)));
            var sourceModels = sourceColumn.ScalarSourceModels;
            foreach (var model in sourceModels)
            {
                if (model != source)
                    throw new ArgumentException(DiagnosticMessages.ColumnMapper_InvalidSourceParentModelSet(model), paramName);
            }
        }

        private static void VerifyTarget(ColumnMapping mapping, string paramName, int index, Model target)
        {
            var targetColumn = mapping.Target;
            if (targetColumn == null)
                throw new ArgumentNullException(string.Format("{0}[{1}].{2}", paramName, index, nameof(ColumnMapping.Target)));
            if (targetColumn.ParentModel != target)
                throw new ArgumentException(DiagnosticMessages.ColumnMapper_InvalidTarget(targetColumn), string.Format("{0}[{1}].{2}", paramName, index, nameof(ColumnMapping.Target)));
        }

        /// <summary>
        /// Builds UPDATE query statement.
        /// </summary>
        /// <param name="target">The target DbTable.</param>
        /// <param name="source">The source DbTable.</param>
        /// <param name="columnMappings">Column mappings between source and target DbTables.</param>
        /// <param name="keyMappings">Key mappings between source and target DbTables.</param>
        /// <returns>The query statement</returns>
        public static DbSelectStatement BuildUpdateStatement(this IDbTable target, IDbTable source,
            IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> keyMappings)
        {
            target.VerifyNotNull(nameof(target));
            source.VerifyNotNull(nameof(source));
            Verify(columnMappings, nameof(columnMappings), source.Model, target.Model);
            Verify(keyMappings, nameof(keyMappings), source.Model, target.Model);

            return source.QueryStatement.BuildUpdateStatement(target.Model, columnMappings, keyMappings);
        }

        /// <summary>
        /// Builds UPDATE query statement.
        /// </summary>
        /// <param name="target">The target DbTable.</param>
        /// <param name="source">The source DbTable.</param>
        /// <param name="columnMappings">Column mappings between source and target DbTables.</param>
        /// <param name="keyMappings">Key mappings between source and target DbTables.</param>
        /// <returns>The query statement</returns>
        public static DbSelectStatement BuildUpdateStatement<TSource, TTarget>(this DbTable<TTarget> target, DbSet<TSource> source,
            IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> keyMappings)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new()
        {
            target.VerifyNotNull(nameof(target));
            source.VerifyNotNull(nameof(source));
            Verify(columnMappings, nameof(columnMappings), source.Model, target.Model);
            Verify(keyMappings, nameof(keyMappings), source.Model, target.Model);

            return target.BuildUpdateStatement(source, columnMappings, keyMappings);
        }

        /// <summary>
        /// Builds UPDATE query statement.
        /// </summary>
        /// <param name="target">The target DbTable.</param>
        /// <param name="source">The source DbTable.</param>
        /// <param name="columnMapper">Provides column mappings between source and target DbTables.</param>
        /// <param name="keyMappings">Key mappings between source and target DbTables.</param>
        /// <returns>The query statement</returns>
        public static DbSelectStatement BuildUpdateStatement<TSource, TTarget>(this DbTable<TTarget> target, DbSet<TSource> source,
            Action<ColumnMapper, TSource, TTarget> columnMapper, IReadOnlyList<ColumnMapping> keyMappings)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new()
        {
            target.VerifyNotNull(nameof(target));
            source.VerifyNotNull(nameof(source));
            columnMapper.VerifyNotNull(nameof(columnMapper));
            Verify(keyMappings, nameof(keyMappings), source.Model, target.Model);

            return target.BuildUpdateStatement(source, columnMapper, keyMappings);
        }

        /// <summary>
        /// Builds DELETE query statement.
        /// </summary>
        /// <typeparam name="TSource">Entity type of source DbSet.</typeparam>
        /// <typeparam name="TTarget">Entity type of target DbTable</typeparam>
        /// <param name="target">The target DbTable.</param>
        /// <param name="source">The source DbSet.</param>
        /// <param name="keyMappings">Key mappings between source and target DbTables.</param>
        /// <returns>The query statement</returns>
        public static DbSelectStatement BuildDeleteStatement<TSource, TTarget>(this DbTable<TTarget> target, DbSet<TSource> source, IReadOnlyList<ColumnMapping> keyMappings)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new()
        {
            target.VerifyNotNull(nameof(target));
            source.VerifyNotNull(nameof(source));
            Verify(keyMappings, nameof(keyMappings), source.Model, target.Model);

            return target.BuildDeleteStatement(source, keyMappings);
        }

        /// <summary>
        /// Executes UPDATE query statement.
        /// </summary>
        /// <param name="dbTable">The target DbTable.</param>
        /// <param name="statement">The query statement.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>Number of rows affected.</returns>
        public static Task<int> UpdateAsync(this IDbTable dbTable, DbSelectStatement statement, CancellationToken ct)
        {
            return dbTable.DbSession.UpdateAsync(statement, ct);
        }
    }
}
