using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    public static class DbTableExtensions
    {
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

        public static DbSelectStatement BuildUpdateStatement(this IDbTable target, IDbTable source,
            IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
        {
            target.VerifyNotNull(nameof(target));
            source.VerifyNotNull(nameof(source));
            Verify(columnMappings, nameof(columnMappings), source.Model, target.Model);
            Verify(join, nameof(join), source.Model, target.Model);

            return source.QueryStatement.BuildUpdateStatement(target.Model, columnMappings, join);
        }

        public static DbSelectStatement BuildUpdateStatement<TSource, TTarget>(this DbTable<TTarget> target, DbSet<TSource> source,
            IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new()
        {
            target.VerifyNotNull(nameof(target));
            source.VerifyNotNull(nameof(source));
            Verify(columnMappings, nameof(columnMappings), source.Model, target.Model);
            Verify(join, nameof(join), source.Model, target.Model);

            return target.BuildUpdateStatement(source, columnMappings, join);
        }

        public static DbSelectStatement BuildUpdateStatement<TSource, TTarget>(this DbTable<TTarget> target, DbSet<TSource> source,
            Action<ColumnMapper, TSource, TTarget> columnMapper, IReadOnlyList<ColumnMapping> join)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new()
        {
            target.VerifyNotNull(nameof(target));
            source.VerifyNotNull(nameof(source));
            columnMapper.VerifyNotNull(nameof(columnMapper));
            Verify(join, nameof(join), source.Model, target.Model);

            return target.BuildUpdateStatement(source, columnMapper, join);
        }

        public static DbSelectStatement BuildDeleteStatement<TSource, TTarget>(this DbTable<TTarget> target, DbSet<TSource> source, IReadOnlyList<ColumnMapping> join)
            where TSource : class, IEntity, new()
            where TTarget : class, IEntity, new()
        {
            target.VerifyNotNull(nameof(target));
            source.VerifyNotNull(nameof(source));
            Verify(join, nameof(join), source.Model, target.Model);

            return target.BuildDeleteStatement(source, join);
        }

        public static Task<int> UpdateAsync(this IDbTable dbTable, DbSelectStatement statement, CancellationToken ct)
        {
            return dbTable.DbSession.UpdateAsync(statement, ct);
        }
    }
}
