using DevZest.Data.Primitives;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DevZest.Data.MySql.Helpers
{
    internal static class DbTableExtensions
    {
        //        internal static DbTable<TChild> MockCreateChild<T, TChild>(this DbTable<T> dbTable, Func<T, TChild> getChildModel)
        //            where T : Model, new()
        //            where TChild : Model, new()
        //        {
        //            return dbTable.MockCreateChild(null, getChildModel);
        //        }

        //        internal static DbTable<TChild> MockCreateChild<T, TChild>(this DbTable<T> dbTable, Action<TChild> initializer, Func<T, TChild> getChildModel)
        //            where T : Model, new()
        //            where TChild : Model, new()
        //        {
        //            var model = dbTable.VerifyCreateChild(initializer, getChildModel);

        //            var dbSession = dbTable.DbSession;
        //            var name = dbSession.AssignTempTableName(model);
        //            var result = DbTable<TChild>.CreateTemp(model, dbSession, name);
        //            return result;
        //        }

        private static MySqlSession MySqlSession<T>(this DbTable<T> dbTable)
            where T : Model, new()
        {
            return (MySqlSession)dbTable.DbSession;
        }

        private static MySqlCommand GetInsertCommand<T>(this DbTable<T> dbTable, DbSelectStatement statement, bool outputIdentity)
            where T : Model, new()
        {
            return dbTable.MySqlSession().GetInsertCommand(statement, outputIdentity);
        }

        private static Func<T, T, KeyMapping> GetJoinMapper<T>(bool skipExisting)
            where T : Model, new()
        {
            if (skipExisting)
                return KeyMapping.Match;
            else
                return null;
        }

        public static MySqlCommand MockInsert<T>(this DbTable<T> dbTable, bool success, DataSet<T> source, int ordinal, bool skipExisting = false, bool updateIdentity = false)
            where T : Model, new()
        {
            return MockInsert(dbTable, success, source, ordinal, ColumnMapper.AutoSelectInsertable, GetJoinMapper<T>(skipExisting), updateIdentity);
        }

        public static MySqlCommand MockInsert<TSource, TTarget>(this DbTable<TTarget> dbTable, bool success, DataSet<TSource> source, int ordinal,
            Action<ColumnMapper, TSource, TTarget> columnMapper, Func<TSource, TTarget, KeyMapping> joinMapper, bool updateIdentity = false)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(source, nameof(source), ordinal, nameof(ordinal));
            var columnMappings = dbTable.Verify(columnMapper, nameof(columnMapper), source._);
            var join = joinMapper == null ? null : dbTable.Verify(joinMapper, nameof(joinMapper), source._).GetColumnMappings();
            dbTable.VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            var result = dbTable.GetInsertScalarCommand(dbTable.BuildInsertScalarStatement(source, ordinal, columnMappings, join), updateIdentity);
            dbTable.UpdateOrigin(source, success);

            return result;
        }

        private static MySqlCommand GetInsertScalarCommand<T>(this DbTable<T> dbTable, DbSelectStatement statement, bool updateIdentity)
            where T : Model, new()
        {
            return dbTable.MySqlSession().InternalGetInsertScalarCommand(statement, updateIdentity);
        }

        public static MySqlCommand MockInsert<T>(this DbTable<T> dbTable, int rowsAffected, DbQuery<T> source, bool skipExisting = false)
            where T : Model, new()
        {
            return MockInsert(dbTable, rowsAffected, source, ColumnMapper.AutoSelectInsertable, GetJoinMapper<T>(skipExisting));
        }

        public static MySqlCommand MockInsert<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DbQuery<TSource> source,
            Action<ColumnMapper, TSource, TTarget> columnMapper, Func<TSource, TTarget, KeyMapping> joinMapper = null)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(source, nameof(source));
            var columnMappings = dbTable.Verify(columnMapper, nameof(columnMapper), source._);
            var join = joinMapper == null ? null : dbTable.Verify(joinMapper, nameof(joinMapper), source._).GetColumnMappings();

            var result = dbTable.GetInsertCommand(dbTable.BuildInsertStatement(source, columnMappings, join), false);
            dbTable.UpdateOrigin(source, rowsAffected);
            return result;
        }

        public static IList<MySqlCommand> MockInsert<T>(this DbTable<T> dbTable, int rowsAffected, DbTable<T> source, bool skipExisting = false, bool updateIdentity = false)
            where T : Model, new()
        {
            return MockInsert(dbTable, rowsAffected, source, ColumnMapper.AutoSelectInsertable, GetJoinMapper<T>(skipExisting), updateIdentity);
        }

        public static IList<MySqlCommand> MockInsert<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected,
            DbTable<TSource> source, Action<ColumnMapper, TSource, TTarget> columnMapper, Func<TSource, TTarget, KeyMapping> joinMapper = null, bool updateIdentity = false)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(source, nameof(source));
            dbTable.VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            var result = dbTable.MockInsertTable(rowsAffected, source, columnMapper, joinMapper, updateIdentity);
            dbTable.UpdateOrigin(source, rowsAffected);
            return result;
        }

        private static IList<MySqlCommand> MockInsertTable<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DbTable<TSource> source,
            Action<ColumnMapper, TSource, TTarget> columnMapper, Func<TSource, TTarget, KeyMapping> joinMapper, bool updateIdentity)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            CandidateKey joinTo = joinMapper == null ? null : dbTable.Verify(joinMapper, nameof(joinMapper), source._).TargetKey;
            var result = new List<MySqlCommand>();
            var mySqlSession = dbTable.MySqlSession();

            if (!updateIdentity)
                result.Add(dbTable.GetInsertCommand(dbTable.BuildInsertStatement(source, columnMapper, joinTo == null ? null : source._.PrimaryKey.UnsafeJoin(joinTo)), updateIdentity));
            else
                throw new NotImplementedException();

            if (!updateIdentity || rowsAffected == 0)
                return result;

            throw new NotImplementedException();

            return result;
        }

        public static IList<MySqlCommand> MockInsert<T>(this DbTable<T> dbTable, int rowsAffected, DataSet<T> source, bool skipExisting = false, bool updateIdentity = false)
            where T : Model, new()
        {
            return MockInsert(dbTable, rowsAffected, source, ColumnMapper.AutoSelectInsertable, GetJoinMapper<T>(skipExisting), updateIdentity);
        }

        public static IList<MySqlCommand> MockInsert<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DataSet<TSource> source,
            Action<ColumnMapper, TSource, TTarget> columnMapper, Func<TSource, TTarget, KeyMapping> joinMapper = null, bool updateIdentity = false)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(source, nameof(source));
            dbTable.VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            var result = new List<MySqlCommand>();

            if (source.Count == 0)
                return result;

            if (source.Count == 1)
            {
                result.Add(dbTable.MockInsert(rowsAffected > 0, source, 0, columnMapper, joinMapper, updateIdentity));
                return result;
            }

            dbTable.UpdateOrigin(source, rowsAffected);
            var sqlSession = dbTable.MySqlSession();

            if (!updateIdentity)
            {
                var joinTo = joinMapper == null ? null : dbTable.Verify(joinMapper, nameof(joinMapper), source._).TargetKey;
                result.Add(sqlSession.BuildInsertCommand(source, dbTable, columnMapper, joinTo));
                return result;
            }

            var tempTable = sqlSession.MockTempTable<TSource>(result);
            result.Add(sqlSession.BuildImportCommand(source, tempTable));
            result.AddRange(dbTable.MockInsertTable(rowsAffected, tempTable, columnMapper, joinMapper, updateIdentity));
            return result;
        }

        //        internal static SqlCommand MockUpdate<T>(this DbTable<T> dbTable, int rowsAffected, Action<ColumnMapper, T> columnMapper, Func<T, _Boolean> where = null)
        //            where T : Model, new()
        //        {
        //            var columnMappings = dbTable.Verify(columnMapper, nameof(columnMapper));
        //            var statement = dbTable.BuildUpdateStatement(columnMappings, where);
        //            var result = dbTable.SqlSession().GetUpdateCommand(statement);
        //            dbTable.UpdateOrigin(null, rowsAffected);
        //            return result;
        //        }

        //        internal static SqlCommand MockUpdate<T>(this DbTable<T> dbTable, int rowsAffected, DbSet<T> dbSet)
        //            where T : Model, new()
        //        {
        //            return MockUpdate(dbTable, rowsAffected, dbSet, ColumnMapper.AutoSelectUpdatable, KeyMapping.Match);
        //        }

        //        internal static SqlCommand MockUpdate<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DbSet<TSource> dbSet,
        //            Action<ColumnMapper, TSource, TTarget> columnMapper, Func<TSource, TTarget, KeyMapping> join)
        //            where TSource : Model, new()
        //            where TTarget : Model, new()
        //        {
        //            dbTable.Verify(dbSet, nameof(dbSet));
        //            var columnMappings = dbTable.Verify(columnMapper, nameof(columnMapper), dbSet._);
        //            var keyMapping = dbTable.Verify(join, nameof(join), dbSet._).GetColumnMappings();
        //            var statement = dbTable.BuildUpdateStatement(dbSet, columnMappings, keyMapping);
        //            var result = dbTable.SqlSession().GetUpdateCommand(statement);
        //            dbTable.UpdateOrigin(null, rowsAffected);
        //            return result;
        //        }

        //        internal static SqlCommand MockUpdate<T>(this DbTable<T> dbTable, bool success, DataSet<T> source, int rowIndex)
        //            where T : Model, new()
        //        {
        //            return MockUpdate(dbTable, success, source, rowIndex, ColumnMapper.AutoSelectUpdatable, KeyMapping.Match);
        //        }

        //        internal static SqlCommand MockUpdate<TSource, TTarget>(this DbTable<TTarget> dbTable, bool success, DataSet<TSource> source, int rowIndex,
        //            Action<ColumnMapper, TSource, TTarget> columnMapper, Func<TSource, TTarget, KeyMapping> joinMapper)
        //            where TSource : Model, new()
        //            where TTarget : Model, new()
        //        {
        //            dbTable.Verify(source, nameof(source), rowIndex, nameof(rowIndex));
        //            var columnMappings = dbTable.Verify(columnMapper, nameof(columnMapper), source._);
        //            var join = dbTable.Verify(joinMapper, nameof(joinMapper), source._).GetColumnMappings();

        //            var statement = dbTable.BuildUpdateScalarStatement(source, rowIndex, columnMappings, join);
        //            var result = dbTable.SqlSession().GetUpdateCommand(statement);
        //            dbTable.UpdateOrigin<TSource>(null, success);
        //            return result;
        //        }

        //        internal static SqlCommand MockUpdate<TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DataSet<TTarget> source)
        //            where TTarget : Model, new()
        //        {
        //            return MockUpdate(dbTable, rowsAffected, source, ColumnMapper.AutoSelectUpdatable, KeyMapping.Match);
        //        }

        //        internal static SqlCommand MockUpdate<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DataSet<TSource> source,
        //            Action<ColumnMapper, TSource, TTarget> columnMapper, Func<TSource, TTarget, KeyMapping> joinMapper)
        //            where TSource : Model, new()
        //            where TTarget : Model, new()
        //        {
        //            if (source == null)
        //                throw new ArgumentNullException(nameof(source));

        //            if (source.Count == 0)
        //                return null;

        //            if (source.Count == 1)
        //            {
        //                Debug.Assert(rowsAffected == 1 || rowsAffected == 0);
        //                MockUpdate(dbTable, rowsAffected > 0, source, 0, columnMapper, joinMapper);
        //            }

        //            dbTable.UpdateOrigin(null, rowsAffected);
        //            var joinTo = dbTable.Verify(joinMapper, nameof(joinMapper), source._).TargetKey;
        //            return dbTable.SqlSession().BuildUpdateCommand(source, dbTable, columnMapper, joinTo);
        //        }

        internal static MySqlCommand MockDelete<T>(this DbTable<T> dbTable, Func<T, _Boolean> where)
            where T : Model, new()
        {
            var statement = dbTable.BuildDeleteStatement(where);
            return dbTable.MySqlSession().GetDeleteCommand(statement);
        }

        internal static MySqlCommand MockDelete<TSource, TTarget>(this DbTable<TTarget> dbTable, DbSet<TSource> source, Func<TSource, TTarget, KeyMapping> keyMapper)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var keyMapping = dbTable.Verify(keyMapper, nameof(keyMapper), source._);
            var statement = dbTable.BuildDeleteStatement(source, keyMapping.GetColumnMappings());
            return dbTable.MySqlSession().GetDeleteCommand(statement);
        }

        internal static MySqlCommand MockDelete<TSource, TTarget>(this DbTable<TTarget> dbTable, DataSet<TSource> source, int ordinal, Func<TSource, TTarget, KeyMapping> keyMapper)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var keyMapping = dbTable.Verify(keyMapper, nameof(keyMapper), source._);
            var statement = dbTable.BuildDeleteScalarStatement(source, ordinal, keyMapping.GetColumnMappings());
            return dbTable.MySqlSession().GetDeleteCommand(statement);
        }

        internal static MySqlCommand MockDelete<TSource, TTarget>(this DbTable<TTarget> dbTable, DataSet<TSource> source, Func<TSource, TTarget, KeyMapping> keyMapper)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(source, nameof(source));

            if (source.Count == 0)
                return null;

            if (source.Count == 1)
                return dbTable.MockDelete(source, 0, keyMapper);

            var keyMapping = dbTable.Verify(keyMapper, nameof(keyMapper), source._);
            return dbTable.MySqlSession().BuildDeleteCommand(source, dbTable, keyMapping.TargetKey);
        }
    }
}
