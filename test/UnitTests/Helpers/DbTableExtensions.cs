using DevZest.Data.Primitives;
using DevZest.Data.SqlServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;

namespace DevZest.Data.Helpers
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

        private static Func<T, T, KeyMapping> GetJoinMapper<T>(bool skipExisting)
            where T : Model, new()
        {
            if (skipExisting)
                return KeyMapping.Match;
            else
                return null;
        }

        public static SqlCommand MockInsert<T>(this DbTable<T> dbTable, bool success, DataSet<T> source, int ordinal, bool skipExisting = false, bool updateIdentity = false)
            where T : Model, new()
        {
            return MockInsert(dbTable, success, source, ordinal, ColumnMapper.AutoSelectInsertable, GetJoinMapper<T>(skipExisting), updateIdentity);
        }

        public static SqlCommand MockInsert<TSource, TTarget>(this DbTable<TTarget> dbTable, bool success, DataSet<TSource> source, int ordinal,
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

        private static SqlCommand GetInsertScalarCommand<T>(this DbTable<T> dbTable, DbSelectStatement statement, bool updateIdentity)
            where T : Model, new()
        {
            return dbTable.SqlSession().InternalGetInsertScalarCommand(statement, updateIdentity);
        }

        public static SqlCommand MockInsert<T>(this DbTable<T> dbTable, int rowsAffected, DbQuery<T> source, bool skipExisting = false)
            where T : Model, new()
        {
            return MockInsert(dbTable, rowsAffected, source, ColumnMapper.AutoSelectInsertable, GetJoinMapper<T>(skipExisting));
        }

        public static SqlCommand MockInsert<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DbQuery<TSource> source,
            Action<ColumnMapper, TSource, TTarget> columnMapper, Func<TSource, TTarget, KeyMapping> joinMapper = null)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(source, nameof(source));
            var columnMappings = dbTable.Verify(columnMapper, nameof(columnMapper), source._);
            var join = joinMapper == null ? null : dbTable.Verify(joinMapper, nameof(joinMapper), source._).GetColumnMappings();

            var result = dbTable.GetInsertCommand(dbTable.BuildInsertStatement(source, columnMappings, join));
            dbTable.UpdateOrigin(source, rowsAffected);
            return result;
        }

        public static IList<SqlCommand> MockInsert<T>(this DbTable<T> dbTable, int rowsAffected, DbTable<T> source, bool skipExisting = false, bool updateIdentity = false)
            where T : Model, new()
        {
            return MockInsert(dbTable, rowsAffected, source, ColumnMapper.AutoSelectInsertable, GetJoinMapper<T>(skipExisting), updateIdentity);
        }

        public static IList<SqlCommand> MockInsert<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected,
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

        private static IList<SqlCommand> MockInsertTable<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DbTable<TSource> source,
            Action<ColumnMapper, TSource, TTarget> columnMapper, Func<TSource, TTarget, KeyMapping> joinMapper = null, bool updateIdentity = false)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            CandidateKey joinTo = joinMapper == null ? null : dbTable.Verify(joinMapper, nameof(joinMapper), source._).TargetKey;
            var result = new List<SqlCommand>();
            var sqlSession = dbTable.SqlSession();

            var identityMappings = updateIdentity ? MockIdentityMappings(dbTable, source, result) : null;
            if (identityMappings == null)
                result.Add(dbTable.GetInsertCommand(dbTable.BuildInsertStatement(source, columnMapper, joinTo == null ? null : source._.PrimaryKey.UnsafeJoin(joinTo))));
            else
            {
                var identityOutput = MockIdentityOutputTable(identityMappings, sqlSession, result);
                var statement = dbTable.BuildInsertStatement(source, columnMapper, joinTo == null ? null : source._.PrimaryKey.UnsafeJoin(joinTo));
                result.Add(sqlSession.GetInsertCommand(statement, identityOutput));
                result.Add(sqlSession.GetInsertIntoIdentityMappingsCommand(source, identityMappings, joinTo == null ? null : dbTable));
                result.Add(sqlSession.GetUpdateIdentityMappingsCommand(identityMappings, identityOutput));
            }

            if (identityMappings == null || rowsAffected == 0)
                return result;

            var statements = source.BuildUpdateIdentityStatement(identityMappings);
            foreach (var statement in statements)
                result.Add(sqlSession.GetUpdateCommand(statement));

            return result;
        }

        private static IDbTable MockIdentityOutputTable(IDbTable identityMappings, SqlSession sqlSession, IList<SqlCommand> commands)
        {
            if (identityMappings is DbTable<Int32IdentityMapping>)
                return sqlSession.MockTempTable<Int32IdentityOutput>(commands);
            else if (identityMappings is DbTable<Int64IdentityMapping>)
                return sqlSession.MockTempTable<Int64IdentityOutput>(commands);
            else
            {
                Debug.Assert(identityMappings is DbTable<Int16IdentityMapping>);
                return sqlSession.MockTempTable<Int16IdentityOutput>(commands);
            }
        }

        private static IDbTable MockIdentityMappings<TSource, TTarget>(DbTable<TTarget> dbTable, DbTable<TSource> source, IList<SqlCommand> commands)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var identity = source.Model.GetIdentity(false);
            if (identity == null)
                return null;

            var identityColumn = identity.Column;
            if (identityColumn is _Int32 int32IdentityColumn)
                return dbTable.SqlSession().MockTempTable<Int32IdentityMapping>(commands);
            else if (identityColumn is _Int64 int64IdentityColumn)
                return dbTable.SqlSession().MockTempTable<Int64IdentityMapping>(commands);
            else if (identityColumn is _Int16 int16IdentityColumn)
                return dbTable.SqlSession().MockTempTable<Int16IdentityMapping>(commands);
            else
                return null;
        }

        public static IList<SqlCommand> MockInsert<T>(this DbTable<T> dbTable, int rowsAffected, DataSet<T> source, bool skipExisting = false, bool updateIdentity = false)
            where T : Model, new()
        {
            return MockInsert(dbTable, rowsAffected, source, ColumnMapper.AutoSelectInsertable, GetJoinMapper<T>(skipExisting), updateIdentity);
        }

        public static IList<SqlCommand> MockInsert<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected, DataSet<TSource> source,
            Action<ColumnMapper, TSource, TTarget> columnMapper, Func<TSource, TTarget, KeyMapping> joinMapper = null, bool updateIdentity = false)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(source, nameof(source));
            dbTable.VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));
            var joinTo = joinMapper == null ? null : dbTable.Verify(joinMapper, nameof(joinMapper), source._).TargetKey;

            var result = new List<SqlCommand>();

            dbTable.UpdateOrigin(source, rowsAffected);
            var sqlSession = dbTable.SqlSession();

            if (joinTo != null && updateIdentity)
                throw new NotImplementedException();
            else
            {
                var identityOutput = updateIdentity ? MockIdentityOutputTable(source.Model, sqlSession, result) : null;
                result.Add(sqlSession.BuildInsertCommand(source, dbTable, columnMapper, identityOutput));
                return result;
            }
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
            return MockUpdate(dbTable, rowsAffected, dbSet, ColumnMapper.AutoSelectUpdatable, KeyMapping.Match);
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
            return MockUpdate(dbTable, success, source, rowIndex, ColumnMapper.AutoSelectUpdatable, KeyMapping.Match);
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
            return MockUpdate(dbTable, rowsAffected, source, ColumnMapper.AutoSelectUpdatable, KeyMapping.Match);
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

        internal static SqlCommand MockDelete<T>(this DbTable<T> dbTable, Func<T, _Boolean> where)
            where T : Model, new()
        {
            var statement = dbTable.BuildDeleteStatement(where);
            return dbTable.SqlSession().GetDeleteCommand(statement);
        }

        internal static SqlCommand MockDelete<TSource, TTarget>(this DbTable<TTarget> dbTable, DbSet<TSource> source, Func<TSource, TTarget, KeyMapping> keyMapper)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var keyMapping = dbTable.Verify(keyMapper, nameof(keyMapper), source._);
            var statement = dbTable.BuildDeleteStatement(source, keyMapping.GetColumnMappings());
            return dbTable.SqlSession().GetDeleteCommand(statement);
        }

        internal static SqlCommand MockDelete<TSource, TTarget>(this DbTable<TTarget> dbTable, DataSet<TSource> source, int ordinal, Func<TSource, TTarget, KeyMapping> keyMapper)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var keyMapping = dbTable.Verify(keyMapper, nameof(keyMapper), source._);
            var statement = dbTable.BuildDeleteScalarStatement(source, ordinal, keyMapping.GetColumnMappings());
            return dbTable.SqlSession().GetDeleteCommand(statement);
        }

        internal static SqlCommand MockDelete<TSource, TTarget>(this DbTable<TTarget> dbTable, DataSet<TSource> source, Func<TSource, TTarget, KeyMapping> keyMapper)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            dbTable.Verify(source, nameof(source));

            if (source.Count == 0)
                return null;

            if (source.Count == 1)
                return dbTable.MockDelete(source, 0, keyMapper);

            var keyMapping = dbTable.Verify(keyMapper, nameof(keyMapper), source._);
            return dbTable.SqlSession().BuildDeleteCommand(source, dbTable, keyMapping.TargetKey);
        }
    }
}
