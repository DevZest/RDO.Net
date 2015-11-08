
using DevZest.Data.SqlServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DevZest.Data.Helpers
{
    internal static class DbTableExtensions
    {
        internal static SqlCommand GetInsertCommand<TSource, TTarget>(this DbTable<TTarget> dbTable, DbSet<TSource> dbSet, Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder = null, bool autoJoin = false)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var statement = dbTable.BuildInsertStatement(dbSet, columnMappingsBuilder, autoJoin);
            return ((SqlSession)dbTable.DbSession).GetInsertCommand(statement);
        }

        internal static SqlCommand GetInsertScalarCommand<TSource, TTarget>(this DbTable<TTarget> dbTable, DataSet<TSource> dataSet, int rowOrdinal, Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder = null, bool autoJoin = false)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var statement = dbTable.BuildInsertScalarStatement(dataSet, rowOrdinal, columnMappingsBuilder, autoJoin);
            return ((SqlSession)dbTable.DbSession).GetInsertCommand(statement);
        }

        internal static SqlCommand GetUpdateCommand<T>(this DbTable<T> dbTable, Action<ColumnMappingsBuilder, T> columnMappingsBuilder, Func<T, _Boolean> getWhere = null)
            where T : Model, new()
        {
            var statement = dbTable.BuildUpdateStatement(columnMappingsBuilder, getWhere);
            return ((SqlSession)dbTable.DbSession).GetUpdateCommand(statement);
        }

        internal static SqlCommand GetUpdateCommand<TSource, TTarget>(this DbTable<TTarget> dbTable, DbSet<TSource> dbSet, Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder = null)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var statement = dbTable.BuildUpdateStatement(dbSet, columnMappingsBuilder);
            return ((SqlSession)dbTable.DbSession).GetUpdateCommand(statement);
        }

        internal static SqlCommand GetUpdateScalarCommand<TSource, TTarget>(this DbTable<TTarget> dbTable, DataSet<TSource> dataSet, int ordinal, Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder = null)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var statement = dbTable.BuildUpdateScalarStatement(dataSet, ordinal, columnMappingsBuilder);
            return ((SqlSession)dbTable.DbSession).GetUpdateCommand(statement);
        }

        internal static SqlCommand GetDeleteCommand<T>(this DbTable<T> dbTable, Func<T, _Boolean> getWhere = null)
            where T : Model, new()
        {
            var statement = dbTable.BuildDeleteStatement(getWhere);
            return ((SqlSession)dbTable.DbSession).GetDeleteCommand(statement);
        }

        internal static SqlCommand GetDeleteCommand<TSource, TTarget>(this DbTable<TTarget> dbTable, DbSet<TSource> dbSet, Action<ColumnMappingsBuilder, TSource, TTarget> keyMappingsBuilder = null)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var statement = dbTable.BuildDeleteStatement(dbSet, keyMappingsBuilder);
            return ((SqlSession)dbTable.DbSession).GetDeleteCommand(statement);
        }

        internal static SqlCommand GetDeleteScalarCommand<TSource, TTarget>(this DbTable<TTarget> dbTable, DataSet<TSource> dataSet, int ordinal, Action<ColumnMappingsBuilder, TSource, TTarget> keyMappingsBuilder = null)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var statement = dbTable.BuildDeleteScalarStatement(dataSet, ordinal, keyMappingsBuilder);
            return ((SqlSession)dbTable.DbSession).GetDeleteCommand(statement);
        }
    }
}
