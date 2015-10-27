using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public sealed partial class DbTable<T> : DbSet<T>, IDbTable
        where T : Model, new()
    {
        internal static DbTable<T> Create(T model, DbSession dbSession, string name)
        {
            return new DbTable<T>(model, dbSession, name, DataSourceKind.DbTable);
        }

        internal static DbTable<T> CreateTemp(T model, DbSession dbSession, string name)
        {
            return new DbTable<T>(model, dbSession, name, DataSourceKind.DbTempTable);
        }

        private DbTable(T model, DbSession dbSession, string name, DataSourceKind kind)
            : base(model, dbSession)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            Name = name;
            _kind = kind;
            model.SetDataSource(this);
        }

        public int InitialRowCount { get; internal set; }

        public string Name { get; private set; }

        private DataSourceKind _kind;
        public override DataSourceKind Kind
        {
            get { return _kind; }
        }

        private DbSelectStatement _selectStatement;
        private DbSelectStatement SelectStatement
        {
            get { return _selectStatement ?? (_selectStatement = GetSelectStatement()); }
        }

        private DbSelectStatement GetSelectStatement()
        {
            var columns = this.Model.Columns;
            var selectList = new ColumnMapping[columns.Count];
            for (int i = 0; i < selectList.Length; i++)
            {
                var column = columns[i];
                selectList[i] = column.CreateMapping(column);
            }
            return new DbSelectStatement(this.Model, selectList, this.FromClause, null, GetOrderBy(), -1, -1);
        }

        private IList<DbExpressionSort> GetOrderBy()
        {
            if (Kind == DataSourceKind.DbTable)
                return null;

            Debug.Assert(Kind == DataSourceKind.DbTempTable);
            var identity = Model.GetIdentity(true);
            if (identity == null)
                return null;
            else
                return new DbExpressionSort[] {
                    new DbExpressionSort(identity.Column.DbExpression, SortDirection.Ascending)
                };
        }

        private DbQueryStatement _queryStatement;
        public override DbQueryStatement QueryStatement
        {
            get { return _queryStatement ?? (_queryStatement = SelectStatement.RemoveSystemColumns()); }
        }

        public override DbQueryStatement SequentialQueryStatement
        {
            get { return SelectStatement; }
        }

        private DbFromClause _fromClause;
        public override DbFromClause FromClause
        {
            get { return _fromClause ?? (_fromClause = new DbTableClause(Model, Name)); }
        }

        public override string ToString()
        {
            return Name;
        }

        public DbTable<TChild> CreateChild<TChild>(Func<T, TChild> getChildModel, DbSet<TChild> sourceData)
            where TChild : Model, new()
        {
            Check.NotNull(sourceData, nameof(sourceData));
            var model = VerifyCreateChild(getChildModel);

            return sourceData.QueryStatement.MakeQueryBuilder(DbSession, model, false).ToTempTable<TChild>(model);
        }

        public Task<DbTable<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, DbSet<TChild> sourceData)
            where TChild : Model, new()
        {
            return CreateChildAsync(getChildModel, sourceData, CancellationToken.None);
        }

        public async Task<DbTable<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, DbSet<TChild> sourceData, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            Check.NotNull(sourceData, nameof(sourceData));
            var model = VerifyCreateChild(getChildModel);

            return await sourceData.QueryStatement.MakeQueryBuilder(DbSession, model, false).ToTempTableAsync(model, cancellationToken);
        }

        public DbTable<TChild> CreateChild<TChild>(Func<T, TChild> getChildModel, Action<DbQueryBuilder, TChild> buildQuery)
            where TChild : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));
            var childModel = VerifyCreateChild(getChildModel);

            var queryBuilder = new DbQueryBuilder(DbSession, childModel);
            buildQuery(queryBuilder, childModel);
            return queryBuilder.ToTempTable<TChild>(childModel);
        }

        public DbTable<TChild> CreateChild<TChild>(Func<T, TChild> getChildModel, Action<DbAggregateQueryBuilder, TChild> buildQuery)
            where TChild : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));
            var childModel = VerifyCreateChild(getChildModel);

            var queryBuilder = new DbAggregateQueryBuilder(DbSession, childModel);
            buildQuery(queryBuilder, childModel);
            return queryBuilder.ToTempTable<TChild>(childModel);
        }

        public Task<DbTable<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, Action<DbQueryBuilder, TChild> buildQuery)
            where TChild : Model, new()
        {
            return CreateChildAsync(getChildModel, buildQuery, CancellationToken.None);
        }

        public Task<DbTable<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, Action<DbAggregateQueryBuilder, TChild> buildQuery)
            where TChild : Model, new()
        {
            return CreateChildAsync(getChildModel, buildQuery, CancellationToken.None);
        }

        public async Task<DbTable<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, Action<DbQueryBuilder, TChild> buildQuery, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));
            var childModel = VerifyCreateChild(getChildModel);

            var queryBuilder = new DbQueryBuilder(DbSession, childModel);
            buildQuery(queryBuilder, childModel);
            return await queryBuilder.ToTempTableAsync<TChild>(childModel, cancellationToken);
        }

        public async Task<DbTable<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, Action<DbAggregateQueryBuilder, TChild> buildQuery, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            Check.NotNull(buildQuery, nameof(buildQuery));
            var childModel = VerifyCreateChild(getChildModel);

            var queryBuilder = new DbAggregateQueryBuilder(DbSession, childModel);
            buildQuery(queryBuilder, childModel);
            return await queryBuilder.ToTempTableAsync<TChild>(childModel, cancellationToken);
        }

        public DbTable<TChild> GetChild<TChild>(Func<T, TChild> getChildModel)
            where TChild : Model, new()
        {
            Check.NotNull(getChildModel, nameof(getChildModel));
            var childModel = getChildModel(_);
            if (childModel == null)
                return null;
            return childModel.DataSource as DbTable<TChild>;
        }

        public override int GetInitialRowCount()
        {
            return InitialRowCount;
        }

        public override Task<int> GetInitialRowCountAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(InitialRowCount);
        }

        internal List<ColumnMapping> GetColumnMappings(Model sourceModel)
        {
            var targetModel = this._;
            var result = new List<ColumnMapping>();
            var sourceColumns = sourceModel.Columns;
            foreach (var column in targetModel.GetUpdatableColumns())
            {
                if (column.IsSystem)
                    continue;
                var sourceColumn = sourceColumns[column.Key];
                if (sourceColumn != null)
                    result.Add(column.CreateMapping(sourceColumn));
            }

            return result;
        }

        private IList<ColumnMapping> BuildColumnMappings<TSource>(Action<ColumnMappingsBuilder, T, TSource> columnMappingsBuilder, TSource sourceModel)
            where TSource : Model, new()
        {
            return new ColumnMappingsBuilder(_, sourceModel).Build(builder => columnMappingsBuilder(builder, _, sourceModel));
        }

        private IList<ColumnMapping> BuildColumnMappings(Action<ColumnMappingsBuilder, T> columnMappingsBuilder)
        {
            return new ColumnMappingsBuilder(_, null).Build(builder => columnMappingsBuilder(builder, _));
        }

        private IList<ColumnMapping> GetKeyMappings(Model sourceModel)
        {
            var targetKey = this._.PrimaryKey;
            if (targetKey == null)
                throw new InvalidOperationException(Strings.DbTable_NoPrimaryKey(Model));

            var sourceKey = sourceModel.PrimaryKey;
            if (sourceKey == null)
                throw new InvalidOperationException(Strings.DbTable_NoPrimaryKey(sourceModel));

            if (targetKey.Count() != sourceKey.Count())
                throw new InvalidOperationException(Strings.DbTable_GetKeyMappings_CannotMatch);

            var result = new ColumnMapping[targetKey.Count()];
            for (int i = 0; i < result.Length; i++)
            {
                var targetColumn = targetKey[i].Column;
                var sourceColumn = sourceKey[i].Column;
                if (targetColumn.DataType != sourceColumn.DataType)
                    throw new InvalidOperationException(Strings.DbTable_GetKeyMappings_CannotMatch);

                result[i] = new ColumnMapping(sourceColumn, targetColumn);
            }
            return result;
        }

        private sealed class ScalarParamManager
        {
            public ScalarParamManager(DataRow dataRow)
            {
                Debug.Assert(dataRow != null);
                _dataRow = dataRow;
            }

            private DataRow _dataRow;
            private Dictionary<Column, Column> _params = new Dictionary<Column, Column>();

            public Model Model
            {
                get { return _dataRow.Model; }
            }

            public Column GetParam(Column column)
            {
                Debug.Assert(column.ParentModel == _dataRow.Model);

                if (_params.ContainsKey(column))
                    return _params[column];

                var result = column.CreateParam(_dataRow);
                _params.Add(column, result);
                return result;
            }
        }

        private static IList<ColumnMapping> GetScalarMapping(ScalarParamManager paramManager, IList<ColumnMapping> mappings, bool mapToSource = false)
        {
            IList<ColumnMapping> result;
            result = new ColumnMapping[mappings.Count];
            for (int i = 0; i < mappings.Count; i++)
            {
                var mapping = mappings[i];
                var source = paramManager.GetParam(mapping.Source);
                result[i] = new ColumnMapping(source, mapToSource ? mapping.Source : mapping.Target);
            }
            return result;
        }

        private static DbSelectStatement GetScalarDataSource(ScalarParamManager paramManager, IList<ColumnMapping> keyMappings, IList<ColumnMapping> parentMappings = null)
        {
            if (keyMappings == null && parentMappings == null)
                return null;

            IList<ColumnMapping> mappings;
            if (parentMappings == null)
                mappings = keyMappings;
            else if (keyMappings == null)
                mappings = parentMappings;
            else
                mappings = keyMappings.Concat(parentMappings).ToList();
            var select = GetScalarMapping(paramManager, mappings, true);
            return new DbSelectStatement(paramManager.Model, select, null, null, null, -1, -1);
        }

        private DbExpression VerifyWhere(Func<T, _Boolean> getWhere)
        {
            if (getWhere == null)
                return null;

            var where = getWhere(_);
            if (where == null)
                return null;

            var parentModelSet = where.ParentModelSet;
            if (parentModelSet.Count == 0 || (parentModelSet.Count == 1 && parentModelSet.Contains(_)))
                return where.DbExpression;

            throw new ArgumentException(Strings.DbTable_VerifyWhere, nameof(getWhere));
        }

        internal override Column GetSourceColumn(int ordinal)
        {
            return Model.Columns[ordinal];
        }
    }
}
