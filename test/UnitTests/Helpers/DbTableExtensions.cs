﻿
using DevZest.Data.Primitives;
using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DevZest.Data.Helpers
{
    internal static class DbTableExtensions
    {
        internal static DbTable<TChild> MockCreateChild<T, TChild>(this DbTable<T> dbTable, Func<T, TChild> getChildModel, Action<T> initializer = null)
            where T : Model, new()
            where TChild : Model, new()
        {
            var model = dbTable.VerifyCreateChild(getChildModel);

            var dbSession = dbTable.DbSession;
            var name = dbSession.AssignTempTableName(model);
            var result = DbTable<TChild>.CreateTemp(model, dbSession, name);
            return result;
        }

        internal static void Verify(this IList<SqlCommand> commands, params string[] commandTextList)
        {
            Assert.AreEqual(commandTextList.Length, commands.Count);
            for (int i = 0; i < commands.Count; i++)
                Assert.AreEqual(commandTextList[i].Trim(), commands[i].ToTraceString().Trim());
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

        private static SqlCommand GetUpdateCommand<T>(this DbTable<T> dbTable, DbSelectStatement statement)
            where T : Model, new()
        {
            return dbTable.SqlSession().GetUpdateCommand(statement);
        }

        private static SqlCommand GetDeleteCommand<T>(this DbTable<T> dbTable, DbSelectStatement statement)
            where T : Model, new()
        {
            return dbTable.SqlSession().GetDeleteCommand(statement);
        }

        internal static SqlCommand MockInsert<TSource, TTarget>(this DbTable<TTarget> dbTable, bool success,
            DataSet<TSource> source, int rowOrdinal,
            Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder = null,
            bool autoJoin = false, bool updateIdentity = false)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            Check.NotNull(source, nameof(source));
            dbTable.VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            var result = dbTable.GetInsertCommand(dbTable.BuildInsertScalarStatement(source, rowOrdinal, columnMappingsBuilder, autoJoin));
            dbTable.UpdateOrigin(source, success);

            return result;
        }

        internal static SqlCommand MockInsert<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected,
            DbQuery<TSource> source, Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder = null, bool autoJoin = false)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            Check.NotNull(source, nameof(source));

            var result = dbTable.GetInsertCommand(dbTable.BuildInsertStatement(source, columnMappingsBuilder, autoJoin));
            dbTable.UpdateOrigin(source, rowsAffected);
            return result;
        }

        internal static IList<SqlCommand> MockInsert<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected,
            DbTable<TSource> source, Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder = null, bool autoJoin = false, bool updateIdentity = false)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var result = dbTable.MockInsertTable(rowsAffected, source, columnMappingsBuilder, autoJoin, updateIdentity);
            dbTable.UpdateOrigin(source, rowsAffected);
            return result;
        }

        private static IList<SqlCommand> MockInsertTable<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected,
            DbTable<TSource> source, Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder = null, bool autoJoin = false, bool updateIdentity = false)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            Check.NotNull(source, nameof(source));
            dbTable.VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            var result = new List<SqlCommand>();
            var sqlSession = dbTable.SqlSession();

            var identityMappings = updateIdentity ? dbTable.SqlSession().MockTempTable<IdentityMapping>(result) : null;
            if (identityMappings == null)
                result.Add(dbTable.GetInsertCommand(dbTable.BuildInsertStatement(source, columnMappingsBuilder, autoJoin)));
            else
            {
                var identityOutput = sqlSession.MockTempTable<IdentityOutput>(result);
                var statement = dbTable.BuildInsertStatement(source, columnMappingsBuilder, autoJoin);
                result.Add(sqlSession.GetInsertCommand(statement, identityOutput));
            }

            if (identityMappings == null || rowsAffected == 0)
                return result;

            var statements = dbTable.BuildUpdateIdentityStatement(identityMappings);
            foreach (var statement in statements)
                result.Add(sqlSession.GetUpdateCommand(statement));

            return result;
        }

        internal static IList<SqlCommand> MockInsert<TSource, TTarget>(this DbTable<TTarget> dbTable, int rowsAffected,
            DataSet<TSource> source, Action<ColumnMappingsBuilder, TSource, TTarget> columnMappingsBuilder = null, bool autoJoin = false, bool updateIdentity = false)
            where TSource : Model, new()
            where TTarget : Model, new()
        {
            var result = new List<SqlCommand>();

            if (source.Count == 0)
                return result;

            if (source.Count == 1)
            {
                result.Add(dbTable.MockInsert(rowsAffected > 0, source, 0, columnMappingsBuilder, autoJoin, updateIdentity));
                return result;
            }

            var sqlSession = dbTable.SqlSession();
            if (!updateIdentity)
            {
                result.Add(dbTable.MockInsert(rowsAffected, sqlSession.GetDbQuery(source, columnMappingsBuilder), null, autoJoin));
                return result;
            }

            var tempTable = sqlSession.MockTempTable<TSource>(result);
            result.AddRange(dbTable.MockInsertTable(rowsAffected, tempTable, columnMappingsBuilder, autoJoin, updateIdentity));
            dbTable.UpdateOrigin(source, rowsAffected);
            return result;
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
