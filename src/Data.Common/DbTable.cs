﻿using DevZest.Data.Primitives;
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

        internal int InitialRowCount { get; set; }

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
                selectList[i] = new ColumnMapping(column, column);
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
        internal override DbQueryStatement QueryStatement
        {
            get { return _queryStatement ?? (_queryStatement = SelectStatement.RemoveSystemColumns()); }
        }

        internal override DbQueryStatement SequentialQueryStatement
        {
            get { return SelectStatement; }
        }

        private DbFromClause _fromClause;
        internal override DbFromClause FromClause
        {
            get { return _fromClause ?? (_fromClause = new DbTableClause(Model, Name)); }
        }

        public override string ToString()
        {
            return Name;
        }

        public DbTable<TChild> CreateChild<TChild>(Func<T, TChild> getChildModel, Action<T> initializer = null)
            where TChild : Model, new()
        {
            var model = VerifyCreateChild(getChildModel);

            var dbSession = DbSession;
            var name = dbSession.AssignTempTableName(model);
            var result = DbTable<TChild>.CreateTemp(model, dbSession, name);
            dbSession.CreateTable(model, name, true);
            return result;
        }

        public Task<DbTable<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, Action<T> initializer = null)
            where TChild : Model, new()
        {
            return CreateChildAsync(getChildModel, initializer, CancellationToken.None);
        }

        public async Task<DbTable<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel, Action<T> initializer, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            var model = VerifyCreateChild(getChildModel);

            var dbSession = DbSession;
            var name = dbSession.AssignTempTableName(model);
            var result = DbTable<TChild>.CreateTemp(model, dbSession, name);
            await dbSession.CreateTableAsync(model, name, true, cancellationToken);
            return result;
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

        private IList<ColumnMapping> BuildColumnMappings(Action<ColumnMappingsBuilder, T> columnMappingsBuilder)
        {
            return new ColumnMappingsBuilder(null, _).Build(builder => columnMappingsBuilder(builder, _));
        }

        private IList<ColumnMapping> GetKeyMappings(Model sourceModel, Func<T, ModelKey> joinOn = null)
        {
            var sourceKey = sourceModel.PrimaryKey;
            if (sourceKey == null)
                throw new InvalidOperationException(Strings.DbTable_NoPrimaryKey(sourceModel));

            var targetKey = joinOn == null ? Model.PrimaryKey : joinOn(_);
            if (targetKey == null)
                throw new InvalidOperationException(Strings.DbTable_GetKeyMappings_CannotMatch);

            if (targetKey.GetType() != sourceKey.GetType() || targetKey.Count != sourceKey.Count)
                throw new InvalidOperationException(Strings.DbTable_GetKeyMappings_CannotMatch);

            var result = new ColumnMapping[targetKey.Count];
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
                var sourceColumn = mapping.SourceColumn;
                var source = paramManager.GetParam(sourceColumn);
                result[i] = new ColumnMapping(source, mapToSource ? sourceColumn : mapping.TargetColumn);
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

        private DbExpression VerifyWhere(Func<T, _Boolean> where)
        {
            if (where == null)
                return null;

            var whereExpr = where(_);
            if (whereExpr == null)
                return null;

            var parentModelSet = whereExpr.ParentModelSet;
            if (parentModelSet.Count == 0 || (parentModelSet.Count == 1 && parentModelSet.Contains(_)))
                return whereExpr.DbExpression;

            throw new ArgumentException(Strings.DbTable_VerifyWhere, nameof(where));
        }

        private IList<ColumnMapping> GetColumnMappings<TSource>(TSource sourceModel, Action<ColumnMappingsBuilder, TSource, T> columnMappingsBuilder)
            where TSource : Model, new()
        {
            return _.GetColumnMappings(sourceModel, columnMappingsBuilder);
        }

        internal int UpdateOrigin(DataSource origin, int rowsAffected)
        {
            if (rowsAffected != 0)
                UpdateOriginalDataSource(origin, true);
            return rowsAffected;
        }

        internal bool UpdateOrigin<TSource>(DataSet<TSource> origin, bool scalarInsertSuccess)
            where TSource : Model, new()
        {
            if (scalarInsertSuccess)
                UpdateOriginalDataSource(origin == null || origin.Count != 1 ? null : origin, true);

            return scalarInsertSuccess;
        }

        internal void VerifySource<TSource>(DbSet<TSource> source)
            where TSource : Model, new()
        {
            Check.NotNull(source, nameof(source));
            if (source.DbSession != DbSession)
                throw new ArgumentException(Strings.DbTable_InvalidDbSetSource);
        }

        internal void VerifySource<TSource>(DataSet<TSource> source)
            where TSource : Model, new()
        {
            Check.NotNull(source, nameof(source));
        }

        internal void VerifySource<TSource>(DataSet<TSource> source, int ordinal)
            where TSource : Model, new()
        {
            VerifySource(source);
            if (ordinal < 0 || ordinal >= source.Count)
                throw new ArgumentOutOfRangeException(nameof(ordinal));
        }
    }
}
