using DevZest.Data.Utilities;
using System;

namespace DevZest.Data.Primitives
{
    public static class DbTableExtensions
    {
        public static DbSelectStatement BuildInsertStatement<T, TSource>(this DbTable<T> targetTable, DbSet<TSource> sourceData, Action<ColumnMappingsBuilder, T, TSource> columnMappingsBuilder, bool autoJoin)
            where T : Model, new()
            where TSource : Model, new()
        {
            Check.NotNull(targetTable, nameof(targetTable));
            Check.NotNull(sourceData, nameof(sourceData));

            return targetTable.BuildInsertStatement(sourceData, columnMappingsBuilder, autoJoin);
        }
    }
}
