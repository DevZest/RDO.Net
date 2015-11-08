using DevZest.Data.Utilities;
using System;

namespace DevZest.Data.Primitives
{
    public static class DbTableExtensions
    {
        public static DbSelectStatement BuildInsertStatement<TSource, TTarget>(this DbTable<TTarget> targetTable, DbSet<TSource> sourceData, Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder, bool autoJoin)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            Check.NotNull(sourceData, nameof(sourceData));
            Check.NotNull(targetTable, nameof(targetTable));

            return targetTable.BuildInsertStatement(sourceData, columnMappingsBuilder, autoJoin);
        }
    }
}
