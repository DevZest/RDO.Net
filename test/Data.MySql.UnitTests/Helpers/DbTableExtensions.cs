using DevZest.Data.Primitives;
using MySql.Data.MySqlClient;
using System;
using System.Diagnostics;

namespace DevZest.Data.MySql.Helpers
{
    internal static class DbTableExtensions
    {
        internal static DbTable<TChild> MockCreateChild<T, TChild>(this DbTable<T> dbTable, Func<T, TChild> getChildModel)
            where T : Model, new()
            where TChild : Model, new()
        {
            return dbTable.MockCreateChild(null, getChildModel);
        }

        internal static DbTable<TChild> MockCreateChild<T, TChild>(this DbTable<T> dbTable, Action<TChild> initializer, Func<T, TChild> getChildModel)
            where T : Model, new()
            where TChild : Model, new()
        {
            var model = dbTable.VerifyCreateChild(initializer, getChildModel);

            var dbSession = dbTable.DbSession;
            var name = dbSession.AssignTempTableName(model);
            var result = DbTable<TChild>.CreateTemp(model, dbSession, name);
            return result;
        }

        private static MySqlSession MySqlSession<T>(this DbTable<T> dbTable)
            where T : Model, new()
        {
            return (MySqlSession)dbTable.DbSession;
        }

        private static MySqlCommand GetInsertCommand<T>(this DbTable<T> dbTable, DbSelectStatement statement)
            where T : Model, new()
        {
            return dbTable.MySqlSession().InternalGetInsertCommand(statement);
        }

        public static MySqlCommand MockInsert<T>(this DbTable<T> dbTable, bool success, DataSet<T> source, int ordinal, bool updateIdentity = false)
            where T : Model, new()
        {
            return MockInsert(dbTable, success, source, ordinal, ColumnMapper.AutoSelectInsertable, updateIdentity);
        }

        public static MySqlCommand MockInsert<TSource, TTarget>(this DbTable<TTarget> dbTable, bool success, DataSet<TSource> source, int ordinal,
            Action<ColumnMapper, TSource, TTarget> columnMapper, bool updateIdentity = false)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(source, nameof(source), ordinal, nameof(ordinal));
            var columnMappings = dbTable.Verify(columnMapper, nameof(columnMapper), source._);
            dbTable.VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            var result = dbTable.GetInsertScalarCommand(dbTable.BuildInsertScalarStatement(source, ordinal, columnMappings));
            dbTable.UpdateOrigin(source, success);

            return result;
        }

        private static MySqlCommand GetInsertScalarCommand<T>(this DbTable<T> dbTable, DbSelectStatement statement)
            where T : Model, new()
        {
            return dbTable.MySqlSession().GetInsertScalarCommand(statement);
        }

        public static MySqlCommand MockInsert<T>(this DbTable<T> dbTable, int rowsAffected, DbSet<T> source)
            where T : Model, new()
        {
            return MockInsert(dbTable, rowsAffected, source, ColumnMapper.AutoSelectInsertable);
        }

        public static MySqlCommand MockInsert<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DbSet<TSource> source,
            Action<ColumnMapper, TSource, TTarget> columnMapper)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(source, nameof(source));
            var columnMappings = dbTable.Verify(columnMapper, nameof(columnMapper), source._);

            var result = dbTable.GetInsertCommand(dbTable.BuildInsertStatement(source, columnMappings));
            dbTable.UpdateOrigin(source, rowsAffected);
            return result;
        }

        public static MySqlCommand MockInsert<T>(this DbTable<T> dbTable, int rowsAffected, DataSet<T> source)
            where T : Model, new()
        {
            return MockInsert(dbTable, rowsAffected, source, ColumnMapper.AutoSelectInsertable);
        }

        public static MySqlCommand MockInsert<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DataSet<TSource> source,
            Action<ColumnMapper, TSource, TTarget> columnMapper)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(source, nameof(source));

            dbTable.UpdateOrigin(source, rowsAffected);
            var mySqlSession = dbTable.MySqlSession();
            return mySqlSession.BuildInsertCommand(source, dbTable, columnMapper);
        }

        internal static MySqlCommand MockUpdate<T>(this DbTable<T> dbTable, int rowsAffected, Action<ColumnMapper, T> columnMapper, Func<T, _Boolean> where = null)
            where T : Model, new()
        {
            var columnMappings = dbTable.Verify(columnMapper, nameof(columnMapper));
            var statement = dbTable.BuildUpdateStatement(columnMappings, where);
            var result = dbTable.MySqlSession().GetUpdateCommand(statement);
            dbTable.UpdateOrigin(null, rowsAffected);
            return result;
        }

        internal static MySqlCommand MockUpdate<T>(this DbTable<T> dbTable, int rowsAffected, DbSet<T> dbSet)
            where T : Model, new()
        {
            return MockUpdate(dbTable, rowsAffected, dbSet, ColumnMapper.AutoSelectUpdatable, KeyMapping.Match);
        }

        internal static MySqlCommand MockUpdate<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DbSet<TSource> dbSet,
            Action<ColumnMapper, TSource, TTarget> columnMapper, Func<TSource, TTarget, KeyMapping> join)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(dbSet, nameof(dbSet));
            var columnMappings = dbTable.Verify(columnMapper, nameof(columnMapper), dbSet._);
            var keyMapping = dbTable.Verify(join, nameof(join), dbSet._).GetColumnMappings();
            var statement = dbTable.BuildUpdateStatement(dbSet, columnMappings, keyMapping);
            var result = dbTable.MySqlSession().GetUpdateCommand(statement);
            dbTable.UpdateOrigin(null, rowsAffected);
            return result;
        }

        internal static MySqlCommand MockUpdate<T>(this DbTable<T> dbTable, bool success, DataSet<T> source, int rowIndex)
            where T : Model, new()
        {
            return MockUpdate(dbTable, success, source, rowIndex, ColumnMapper.AutoSelectUpdatable, KeyMapping.Match);
        }

        internal static MySqlCommand MockUpdate<TSource, TTarget>(this DbTable<TTarget> dbTable, bool success, DataSet<TSource> source, int rowIndex,
            Action<ColumnMapper, TSource, TTarget> columnMapper, Func<TSource, TTarget, KeyMapping> joinMapper)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(source, nameof(source), rowIndex, nameof(rowIndex));
            var columnMappings = dbTable.Verify(columnMapper, nameof(columnMapper), source._);
            var join = dbTable.Verify(joinMapper, nameof(joinMapper), source._).GetColumnMappings();

            var statement = dbTable.BuildUpdateScalarStatement(source, rowIndex, columnMappings, join);
            var result = dbTable.MySqlSession().GetUpdateCommand(statement);
            dbTable.UpdateOrigin<TSource>(null, success);
            return result;
        }

        internal static MySqlCommand MockUpdate<TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DataSet<TTarget> source)
            where TTarget : Model, new()
        {
            return MockUpdate(dbTable, rowsAffected, source, ColumnMapper.AutoSelectUpdatable, KeyMapping.Match);
        }

        internal static MySqlCommand MockUpdate<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DataSet<TSource> source,
            Action<ColumnMapper, TSource, TTarget> columnMapper, Func<TSource, TTarget, KeyMapping> joinMapper)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            dbTable.UpdateOrigin(null, rowsAffected);
            var joinTo = dbTable.Verify(joinMapper, nameof(joinMapper), source._).TargetKey;
            return dbTable.MySqlSession().BuildUpdateCommand(source, dbTable, columnMapper, joinTo);
        }

        internal static MySqlCommand MockDelete<T>(this DbTable<T> dbTable, int rowsAffected, Func<T, _Boolean> where)
            where T : Model, new()
        {
            dbTable.UpdateOrigin(null, rowsAffected);
            var statement = dbTable.BuildDeleteStatement(where);
            return dbTable.MySqlSession().GetDeleteCommand(statement);
        }

        internal static MySqlCommand MockDelete<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DbSet<TSource> source, Func<TSource, TTarget, KeyMapping> keyMapper)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var keyMapping = dbTable.Verify(keyMapper, nameof(keyMapper), source._);
            dbTable.UpdateOrigin(source, rowsAffected);
            var statement = dbTable.BuildDeleteStatement(source, keyMapping.GetColumnMappings());
            return dbTable.MySqlSession().GetDeleteCommand(statement);
        }

        internal static MySqlCommand MockDelete<TSource, TTarget>(this DbTable<TTarget> dbTable, bool success, DataSet<TSource> source, int ordinal, Func<TSource, TTarget, KeyMapping> keyMapper)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var keyMapping = dbTable.Verify(keyMapper, nameof(keyMapper), source._);
            dbTable.UpdateOrigin<TSource>(source, success);
            var statement = dbTable.BuildDeleteScalarStatement(source, ordinal, keyMapping.GetColumnMappings());
            return dbTable.MySqlSession().GetDeleteCommand(statement);
        }

        internal static MySqlCommand MockDelete<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DataSet<TSource> source, Func<TSource, TTarget, KeyMapping> keyMapper)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(source, nameof(source));

            if (source.Count == 0)
                return null;

            if (source.Count == 1)
            {
                Debug.Assert(rowsAffected == 1 || rowsAffected == 0);
                return dbTable.MockDelete(rowsAffected != 0, source, 0, keyMapper);
            }

            var keyMapping = dbTable.Verify(keyMapper, nameof(keyMapper), source._);
            dbTable.UpdateOrigin(source, rowsAffected);
            return dbTable.MySqlSession().BuildDeleteCommand(source, dbTable, keyMapping.TargetKey);
        }
    }
}
