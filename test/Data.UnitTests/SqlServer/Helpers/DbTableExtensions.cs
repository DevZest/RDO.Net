using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;

namespace DevZest.Data.SqlServer.Helpers
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

        private static SqlSession SqlSession<T>(this DbTable<T> dbTable)
            where T : Model, new()
        {
            return (SqlSession)dbTable.DbSession;
        }

        private static SqlCommand GetInsertCommand<T>(this DbTable<T> dbTable, DbSelectStatement statement)
            where T : Model, new()
        {
            return dbTable.SqlSession().GetInsertCommand(statement);
        }

        private static void AutoSelectInsertable<T>(ColumnMapper columnMapper, T source, T target)
        {
            columnMapper.AutoSelectInsertable();
        }

        private static void AutoSelectUpdatable<T>(ColumnMapper columnMapper, T source, T target)
        {
            columnMapper.AutoSelectUpdatable();
        }

        public static SqlCommand MockInsert<T>(this DbTable<T> dbTable, bool success, DataSet<T> source, int ordinal, bool updateIdentity = false)
            where T : Model, new()
        {
            return MockInsert(dbTable, success, source, ordinal, AutoSelectInsertable, updateIdentity);
        }

        public static SqlCommand MockInsert<TSource, TTarget>(this DbTable<TTarget> dbTable, bool success, DataSet<TSource> source, int ordinal,
            Action<ColumnMapper, TSource, TTarget> columnMapper, bool updateIdentity = false)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(source, nameof(source), ordinal, nameof(ordinal));
            var columnMappings = dbTable.Verify(columnMapper, nameof(columnMapper), source._);
            dbTable.VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            var result = dbTable.GetInsertScalarCommand(dbTable.BuildInsertScalarStatement(source, ordinal, columnMappings), updateIdentity);
            dbTable.UpdateOrigin(source, success);

            return result;
        }

        private static SqlCommand GetInsertScalarCommand<T>(this DbTable<T> dbTable, DbSelectStatement statement, bool updateIdentity)
            where T : Model, new()
        {
            return dbTable.SqlSession().GetInsertScalarCommand(statement, updateIdentity, out _);
        }

        public static SqlCommand MockInsert<T>(this DbTable<T> dbTable, int rowsAffected, DbSet<T> source)
            where T : Model, new()
        {
            return MockInsert(dbTable, rowsAffected, source, AutoSelectInsertable);
        }

        public static SqlCommand MockInsert<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DbSet<TSource> source, Action<ColumnMapper, TSource, TTarget> columnMapper)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(source, nameof(source));
            var columnMappings = dbTable.Verify(columnMapper, nameof(columnMapper), source._);

            var result = dbTable.GetInsertCommand(dbTable.BuildInsertStatement(source, columnMappings));
            dbTable.UpdateOrigin(source, rowsAffected);
            return result;
        }

        public static IList<SqlCommand> MockInsert<T>(this DbTable<T> dbTable, int rowsAffected, DataSet<T> source, bool updateIdentity = false)
            where T : Model, new()
        {
            return MockInsert(dbTable, rowsAffected, source, AutoSelectInsertable, updateIdentity);
        }

        public static IList<SqlCommand> MockInsert<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DataSet<TSource> source,
            Action<ColumnMapper, TSource, TTarget> columnMapper, bool updateIdentity = false)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(source, nameof(source));
            dbTable.VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            var result = new List<SqlCommand>();

            dbTable.UpdateOrigin(source, rowsAffected);
            var sqlSession = dbTable.SqlSession();

            var identityOutput = updateIdentity ? MockIdentityOutputTable(source.Model, sqlSession, result) : null;
            result.Add(sqlSession.BuildInsertCommand(source, dbTable, columnMapper, identityOutput));

            return result;
        }

        private static IDbTable MockIdentityOutputTable(Model model, SqlSession sqlSession, IList<SqlCommand> commands)
        {
            var identity = model.GetIdentity(false);
            if (identity == null)
                return null;

            var column = identity.Column;
            if (column is _Int32)
                return sqlSession.MockTempTable<Int32IdentityOutput>(commands);
            else if (column is _Int64)
                return sqlSession.MockTempTable<Int64IdentityOutput>(commands);
            else if (column is _Int16)
                return sqlSession.MockTempTable<Int16IdentityOutput>(commands);
            else
                return null;
        }

        internal static SqlCommand MockUpdate<T>(this DbTable<T> dbTable, int rowsAffected, Action<ColumnMapper, T> columnMapper, Func<T, _Boolean> where = null)
            where T : Model, new()
        {
            var columnMappings = dbTable.Verify(columnMapper, nameof(columnMapper));
            var statement = dbTable.BuildUpdateStatement(columnMappings, where);
            var result = dbTable.SqlSession().GetUpdateCommand(statement);
            dbTable.UpdateOrigin(null, rowsAffected);
            return result;
        }

        internal static SqlCommand MockUpdate<T>(this DbTable<T> dbTable, int rowsAffected, DbSet<T> dbSet)
            where T : Model, new()
        {
            return MockUpdate(dbTable, rowsAffected, dbSet, AutoSelectUpdatable, KeyMapping.Match);
        }

        internal static SqlCommand MockUpdate<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DbSet<TSource> dbSet,
            Action<ColumnMapper, TSource, TTarget> columnMapper, Func<TSource, TTarget, KeyMapping> join)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(dbSet, nameof(dbSet));
            var columnMappings = dbTable.Verify(columnMapper, nameof(columnMapper), dbSet._);
            var keyMapping = dbTable.Verify(join, nameof(join), dbSet._).GetColumnMappings();
            var statement = dbTable.BuildUpdateStatement(dbSet, columnMappings, keyMapping);
            var result = dbTable.SqlSession().GetUpdateCommand(statement);
            dbTable.UpdateOrigin(null, rowsAffected);
            return result;
        }

        internal static SqlCommand MockUpdate<T>(this DbTable<T> dbTable, bool success, DataSet<T> source, int rowIndex)
            where T : Model, new()
        {
            return MockUpdate(dbTable, success, source, rowIndex, AutoSelectUpdatable, KeyMapping.Match);
        }

        internal static SqlCommand MockUpdate<TSource, TTarget>(this DbTable<TTarget> dbTable, bool success, DataSet<TSource> source, int rowIndex,
            Action<ColumnMapper, TSource, TTarget> columnMapper, Func<TSource, TTarget, KeyMapping> joinMapper)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(source, nameof(source), rowIndex, nameof(rowIndex));
            var columnMappings = dbTable.Verify(columnMapper, nameof(columnMapper), source._);
            var join = dbTable.Verify(joinMapper, nameof(joinMapper), source._).GetColumnMappings();

            var statement = dbTable.BuildUpdateScalarStatement(source, rowIndex, columnMappings, join);
            var result = dbTable.SqlSession().GetUpdateCommand(statement);
            dbTable.UpdateOrigin<TSource>(null, success);
            return result;
        }

        internal static SqlCommand MockUpdate<TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DataSet<TTarget> source)
            where TTarget : Model, new()
        {
            return MockUpdate(dbTable, rowsAffected, source, AutoSelectUpdatable, KeyMapping.Match);
        }

        internal static SqlCommand MockUpdate<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DataSet<TSource> source,
            Action<ColumnMapper, TSource, TTarget> columnMapper, Func<TSource, TTarget, KeyMapping> joinMapper)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source.Count == 0)
                return null;

            if (source.Count == 1)
            {
                Debug.Assert(rowsAffected == 1 || rowsAffected == 0);
                MockUpdate(dbTable, rowsAffected > 0, source, 0, columnMapper, joinMapper);
            }

            dbTable.UpdateOrigin(null, rowsAffected);
            var joinTo = dbTable.Verify(joinMapper, nameof(joinMapper), source._).TargetKey;
            return dbTable.SqlSession().BuildUpdateCommand(source, dbTable, columnMapper, joinTo);
        }

        internal static SqlCommand MockDelete<T>(this DbTable<T> dbTable, int rowsAffected, Func<T, _Boolean> where)
            where T : Model, new()
        {
            dbTable.UpdateOrigin(null, rowsAffected);
            var statement = dbTable.BuildDeleteStatement(where);
            return dbTable.SqlSession().GetDeleteCommand(statement);
        }

        internal static SqlCommand MockDelete<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DbSet<TSource> source, Func<TSource, TTarget, KeyMapping> keyMapper)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var keyMapping = dbTable.Verify(keyMapper, nameof(keyMapper), source._);
            dbTable.UpdateOrigin(source, rowsAffected);
            var statement = dbTable.BuildDeleteStatement(source, keyMapping.GetColumnMappings());
            return dbTable.SqlSession().GetDeleteCommand(statement);
        }

        internal static SqlCommand MockDelete<TSource, TTarget>(this DbTable<TTarget> dbTable, bool success, DataSet<TSource> source, int ordinal, Func<TSource, TTarget, KeyMapping> keyMapper)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var keyMapping = dbTable.Verify(keyMapper, nameof(keyMapper), source._);
            dbTable.UpdateOrigin<TSource>(source, success);
            var statement = dbTable.BuildDeleteScalarStatement(source, ordinal, keyMapping.GetColumnMappings());
            return dbTable.SqlSession().GetDeleteCommand(statement);
        }

        internal static SqlCommand MockDelete<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DataSet<TSource> source, Func<TSource, TTarget, KeyMapping> keyMapper)
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
            return dbTable.SqlSession().BuildDeleteCommand(source, dbTable, keyMapping.TargetKey);
        }
    }
}
