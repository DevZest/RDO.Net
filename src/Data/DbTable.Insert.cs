using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    partial class DbTable<T>
    {
        private static Func<TSource, T, KeyMapping> GetJoinMapper<TSource>(bool skipExisting)
            where TSource : T, new()
        {
            if (skipExisting)
                return KeyMapping.Match;
            else
                return null;
        }

        public DbTableInsert<T> Insert<TSource>(DbQuery<TSource> source, bool skipExisting = false)
            where TSource : class, T, new()
        {
            return Insert(source, (m, s, t) => ColumnMapper.AutoSelectInsertable(m, s, t), GetJoinMapper<TSource>(skipExisting));
        }

        public DbTableInsert<T> Insert<TSource>(DbQuery<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, Func<TSource, T, KeyMapping> joinMapper = null)
            where TSource : class, IModelReference, new()
        {
            Verify(source, nameof(source));
            var columnMappings = Verify(columnMapper, nameof(columnMapper), source._);
            IReadOnlyList<ColumnMapping> join = joinMapper == null ? null : Verify(joinMapper, nameof(joinMapper), source._).GetColumnMappings();
            return DbTableInsert<T>.Create(this, source, columnMappings, join);
        }

        public DbTableInsert<T> Insert<TSource>(DbTable<TSource> source, bool skipExisting = false, bool updateIdentity = false)
            where TSource : class, T, new()
        {
            return Insert(source, (m, s, t) => ColumnMapper.AutoSelectInsertable(m, s, t), GetJoinMapper<TSource>(skipExisting), updateIdentity);
        }

        public DbTableInsert<T> Insert<TSource>(DbTable<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, Func<TSource, T, KeyMapping> joinMapper = null, bool updateIdentity = false)
            where TSource : class, IModelReference, new()
        {
            Verify(source, nameof(source));
            Verify(columnMapper, nameof(columnMapper));
            CandidateKey joinTo = joinMapper == null ? null : Verify(joinMapper, nameof(joinMapper), source._).TargetKey;
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));
            return DbTableInsert<T>.Create(this, source, columnMapper, joinTo, updateIdentity);
        }

        internal void VerifyUpdateIdentity(bool updateIdentity, string paramName)
        {
            if (!updateIdentity)
                return;

            if (Kind == DataSourceKind.DbTempTable || Model.GetIdentity(false) == null)
                throw new ArgumentException(DiagnosticMessages.DbTable_VerifyUpdateIdentity, paramName);
        }

        internal DbSelectStatement BuildInsertStatement<TSource>(DbSet<TSource> source, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
            where TSource : class, IModelReference, new()
        {
            var sourceModel = source._;
            return source.QueryStatement.BuildInsertStatement(Model, columnMappings, join, ShouldJoinParent(source));
        }

        private bool ShouldJoinParent(DataSource sourceData)
        {
            Debug.Assert(sourceData != null);

            var parentModel = Model.ParentModel;
            if (parentModel == null)
                return false;

            sourceData = sourceData.UltimateOriginalDataSource;
            if (sourceData == null)
                return true;
            var sourceParentModel = sourceData.Model.ParentModel;
            if (sourceParentModel == null)
                return true;
            var parentDataSource = sourceParentModel.DataSource;
            if (parentDataSource == null)
                return true;

            return parentModel.DataSource.UltimateOriginalDataSource != parentDataSource.UltimateOriginalDataSource;
        }

        public DbTableInsert<T> Insert<TSource>(DataSet<TSource> source, bool skipExisting = false, bool updateIdentity = false)
            where TSource : class, T, new()
        {
            return Insert(source, (m, s, t) => ColumnMapper.AutoSelectInsertable(m, s, t), GetJoinMapper<TSource>(skipExisting), updateIdentity);
        }

        public DbTableInsert<T> Insert<TSource>(DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, Func<TSource, T, KeyMapping> joinMapper, bool updateIdentity = false)
            where TSource : class, IModelReference, new()
        {
            Verify(source, nameof(source));
            if (source.Count == 1)
                return Insert(source, 0, columnMapper, joinMapper, updateIdentity);

            Verify(columnMapper, nameof(columnMapper));
            CandidateKey joinTo = joinMapper == null ? null : Verify(joinMapper, nameof(joinMapper), source._).TargetKey;
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            return DbTableInsert<T>.Create(this, source, columnMapper, joinTo, updateIdentity);
        }

        public DbTableInsert<T> Insert<TSource>(DataSet<TSource> source, int ordinal, bool skipExisting = false, bool updateIdentity = false)
            where TSource : class, T, new()
        {
            return Insert(source, ordinal, (m, s, t) => ColumnMapper.AutoSelectInsertable(m, s, t), GetJoinMapper<TSource>(skipExisting), updateIdentity);
        }

        public DbTableInsert<T> Insert<TSource>(DataSet<TSource> source, int ordinal,
            Action<ColumnMapper, TSource, T> columnMapper, Func<TSource, T, KeyMapping> joinMapper, bool updateIdentity = false)
            where TSource : class, IModelReference, new()
        {
            Verify(source, nameof(source), ordinal, nameof(ordinal));
            var columnMappings = Verify(columnMapper, nameof(columnMapper), source._);
            IReadOnlyList<ColumnMapping> join = joinMapper == null ? null : Verify(joinMapper, nameof(joinMapper), source._).GetColumnMappings();
            VerifyUpdateIdentity(updateIdentity, nameof(updateIdentity));

            return DbTableInsert<T>.Create(this, source, ordinal, columnMappings, join, updateIdentity);
        }

        internal DbSelectStatement BuildInsertScalarStatement<TSource>(DataSet<TSource> dataSet, int rowOrdinal,
            IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
            where TSource : class, IModelReference, new()
        {
            var sourceModel = dataSet._;
            var parentMappings = ShouldJoinParent(dataSet) ? this.Model.GetParentRelationship(columnMappings) : null;

            var paramManager = new ScalarParamManager(dataSet[rowOrdinal]);
            var select = GetScalarMapping(paramManager, columnMappings);
            IDbTable parentTable = null;
            if (parentMappings != null)
            {
                parentTable = (IDbTable)Model.ParentModel.DataSource;
                Debug.Assert(parentTable != null);
                var parentRowIdMapping = new ColumnMapping(Model.GetSysParentRowIdColumn(createIfNotExist: false),
                    parentTable.Model.GetSysRowIdColumn(createIfNotExist: false));
                select = select.Append(parentRowIdMapping);
            }

            DbFromClause from = GetScalarDataSource(paramManager, join, parentMappings);
            DbExpression where = null;
            if (from != null)
            {
                if (parentMappings != null)
                    from = new DbJoinClause(DbJoinKind.InnerJoin, from, parentTable.FromClause, parentMappings);

                if (join != null)
                {
                    from = new DbJoinClause(DbJoinKind.LeftJoin, from, FromClause, join);
                    where = new DbFunctionExpression(FunctionKeys.IsNull, new DbExpression[] { join[0].Target.DbExpression });
                }
            }

            return new DbSelectStatement(Model, select, from, where, null, -1, -1);
        }
    }
}
