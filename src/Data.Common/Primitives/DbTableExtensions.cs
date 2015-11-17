using DevZest.Data.Utilities;
using System;

namespace DevZest.Data.Primitives
{
    public static class DbTableExtensions
    {
        public static DbSelectStatement BuildInsertStatement<TSource, TTarget>(this DbTable<TTarget> targetTable, DbSet<TSource> sourceData,
            Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder, bool autoJoin)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            Check.NotNull(targetTable, nameof(targetTable));
            Check.NotNull(sourceData, nameof(sourceData));

            return targetTable.BuildInsertStatement(sourceData, columnMappingsBuilder, autoJoin);
        }

        public static DbSelectStatement BuildUpdateStatement<TSource, TTarget>(this DbTable<TTarget> targetTable, DbSet<TSource> sourceData,
            Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            Check.NotNull(targetTable, nameof(targetTable));
            Check.NotNull(sourceData, nameof(sourceData));

            return targetTable.BuildUpdateStatement(sourceData, columnMappingsBuilder);
        }
    }
}
