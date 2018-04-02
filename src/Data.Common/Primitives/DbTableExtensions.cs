using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public static class DbTableExtensions
    {
        public static DbSelectStatement BuildInsertStatement<TSource, TTarget>(this DbTable<TTarget> target, DbSet<TSource> source,
            Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder = null, bool autoJoin = false)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            Check.NotNull(target, nameof(target));
            Check.NotNull(source, nameof(source));

            return target.BuildInsertStatement(source, columnMappingsBuilder, autoJoin);
        }

        public static DbSelectStatement BuildUpdateStatement<TSource, TTarget>(this DbTable<TTarget> target, DbSet<TSource> source,
            Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder = null)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            Check.NotNull(target, nameof(target));
            Check.NotNull(source, nameof(source));

            return target.BuildUpdateStatement(source, columnMappingsBuilder);
        }

        public static DbSelectStatement BuildDeleteStatement<TSource, TTarget>(this DbTable<TTarget> targetTable, DbSet<TSource> source, IReadOnlyList<ColumnMapping> columnMappings)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            Check.NotNull(targetTable, nameof(targetTable));

            return targetTable.BuildDeleteStatement(source, columnMappings);
        }
    }
}
