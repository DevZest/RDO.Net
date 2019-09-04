using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace DevZest.Data
{
    /// <summary>
    /// Builds database query which can be translated to native SQL.
    /// </summary>
    public partial class DbQueryBuilder
    {
        internal DbQueryBuilder(Model model)
        {
            Debug.Assert(model != null && (
                model.DataSource == null || 
                model.DataSource.Kind == DataSourceKind.DbTable ||
                model.DataSource.Kind == DataSourceKind.DbTempTable));

            Model = model;
            Offset = -1;
            Fetch = -1;
        }

        internal virtual void Initialize(DbSelectStatement query)
        {
            _subQueryEliminator = query.SubQueryEliminator;
            FromClause = query.From;
            _sourceModels = _sourceModels.Add(FromClause);
            WhereExpression = query.Where;
            OrderByList = query.OrderBy;
        }

        private void Select(IReadOnlyList<ColumnMapping> select)
        {
            foreach (var columnMapping in select)
            {
                var source = EliminateSubQuery(columnMapping.SourceExpression);
                SelectCore(source, columnMapping.Target);
            }
        }

        internal Model Model { get; private set; }

        #region FROM

        IModels _sourceModels = Models.Empty;
        SubQueryEliminator _subQueryEliminator;
        IAutoColumnSelector _autoColumnSelector;

        /// <summary>
        /// Gets the SQL FROM clause.
        /// </summary>
        public DbFromClause FromClause { get; private set; }

        /// <summary>
        /// Constructs SQL FROM clause.
        /// </summary>
        /// <typeparam name="T">Entity type of the DbSet.</typeparam>
        /// <param name="dbSet">The first DbSet in FROM clause.</param>
        /// <param name="_">The entity object for further SQL construction.</param>
        /// <returns>This query builder for fluent coding.</returns>
        public DbQueryBuilder From<T>(DbSet<T> dbSet, out T _)
            where T : class, IEntity, new()
        {
            dbSet.VerifyNotNull(nameof(dbSet));

            _ = dbSet._;
            if (_sourceModels.Count > 0)
                throw new InvalidOperationException(DiagnosticMessages.DbQueryBuilder_DuplicateFrom);

            From(_.Model);
            return this;
        }

        private void From(Model model)
        {
            _subQueryEliminator = model.FromClause.SubQueryEliminator;
            if (_subQueryEliminator != null)
                WhereExpression = And(WhereExpression, _subQueryEliminator.WhereExpression);
            AddSourceModel(model);
            FromClause =_subQueryEliminator == null ? model.FromClause : _subQueryEliminator.FromClause;
            Debug.Assert(FromClause != null);
        }

        private void AddSourceModel(Model model)
        {
            Debug.Assert(!_sourceModels.Contains(model));

            _sourceModels = _sourceModels.Add(model);
            if (_autoColumnSelector == null)
                _autoColumnSelector = model.Columns;
            else
                _autoColumnSelector = _autoColumnSelector.Merge(model.Columns);
        }

        /// <summary>
        /// Constructs SQL INNER JOIN.
        /// </summary>
        /// <typeparam name="T">Entity type of the target DbSet.</typeparam>
        /// <typeparam name="TKey">Type of the candidate key to join.</typeparam>
        /// <param name="dbSet">The target DbSet.</param>
        /// <param name="left">Left side key of the join.</param>
        /// <param name="_">The entity object of the target DbSet for further SQL construction.</param>
        /// <returns>This query builder for fluent coding.</returns>
        public DbQueryBuilder InnerJoin<T, TKey>(DbSet<T> dbSet, TKey left, out T _)
            where T : class, IEntity<TKey>, new()
            where TKey : CandidateKey
        {
            return InnerJoin(dbSet, left, GetPrimaryKey, out _);
        }

        private static T GetPrimaryKey<T>(IEntity<T> _)
            where T : CandidateKey
        {
            return _.Model.PrimaryKey;
        }

        /// <summary>
        /// Constructs SQL INNER JOIN.
        /// </summary>
        /// <typeparam name="T">Entity type of the target DbSet.</typeparam>
        /// <typeparam name="TKey">Type of the candidate key to join.</typeparam>
        /// <param name="dbSet">The target DbSet.</param>
        /// <param name="left">Left side key of the join.</param>
        /// <param name="right">The delegate to get right side key of the join.</param>
        /// <param name="_">The entity object of the target DbSet for further SQL construction.</param>
        /// <returns>This query builder for fluent coding.</returns>
        public DbQueryBuilder InnerJoin<T, TKey>(DbSet<T> dbSet, TKey left, Func<T, TKey> right, out T _)
            where T : class, IEntity, new()
            where TKey : CandidateKey
        {
            Join(dbSet, left, right(dbSet._), DbJoinKind.InnerJoin, out _);
            return this;
        }

        /// <summary>
        /// Constructs SQL LEFT JOIN.
        /// </summary>
        /// <typeparam name="T">Entity type of the target DbSet.</typeparam>
        /// <typeparam name="TKey">Type of the candidate key to join.</typeparam>
        /// <param name="dbSet">The target DbSet.</param>
        /// <param name="left">Left side key of the join.</param>
        /// <param name="_">The entity object of the target DbSet for further SQL construction.</param>
        /// <returns>This query builder for fluent coding.</returns>
        public DbQueryBuilder LeftJoin<T, TKey>(DbSet<T> dbSet, TKey left, out T _)
            where T : class, IEntity<TKey>, new()
            where TKey : CandidateKey
        {
            return LeftJoin(dbSet, left, GetPrimaryKey, out _);
        }


        /// <summary>
        /// Constructs SQL LEFT JOIN.
        /// </summary>
        /// <typeparam name="T">Entity type of the target DbSet.</typeparam>
        /// <typeparam name="TKey">Type of the candidate key to join.</typeparam>
        /// <param name="dbSet">The target DbSet.</param>
        /// <param name="left">Left side key of the join.</param>
        /// <param name="right">The delegate to get right side key of the join.</param>
        /// <param name="_">The entity object of the target DbSet for further SQL construction.</param>
        /// <returns>This query builder for fluent coding.</returns>
        public DbQueryBuilder LeftJoin<T, TKey>(DbSet<T> dbSet, TKey left, Func<T, TKey> right, out T _)
            where T : class, IEntity, new()
            where TKey : CandidateKey
        {
            Join(dbSet, left, right(dbSet._), DbJoinKind.LeftJoin, out _);
            return this;
        }

        /// <summary>
        /// Constructs SQL RIGHT JOIN.
        /// </summary>
        /// <typeparam name="T">Entity type of the target DbSet.</typeparam>
        /// <typeparam name="TKey">Type of the candidate key to join.</typeparam>
        /// <param name="dbSet">The target DbSet.</param>
        /// <param name="left">Left side key of the join.</param>
        /// <param name="_">The entity object of the target DbSet for further SQL construction.</param>
        /// <returns>This query builder for fluent coding.</returns>
        public DbQueryBuilder RightJoin<T, TKey>(DbSet<T> dbSet, TKey left, out T _)
            where T : class, IEntity<TKey>, new()
            where TKey : CandidateKey
        {
            return RightJoin(dbSet, left, GetPrimaryKey, out _);
        }


        /// <summary>
        /// Constructs SQL RIGHT JOIN.
        /// </summary>
        /// <typeparam name="T">Entity type of the target DbSet.</typeparam>
        /// <typeparam name="TKey">Type of the candidate key to join.</typeparam>
        /// <param name="dbSet">The target DbSet.</param>
        /// <param name="left">Left side key of the join.</param>
        /// <param name="right">The delegate to get right side key of the join.</param>
        /// <param name="_">The entity object of the target DbSet for further SQL construction.</param>
        /// <returns>This query builder for fluent coding.</returns>
        public DbQueryBuilder RightJoin<T, TKey>(DbSet<T> dbSet, TKey left, Func<T, TKey> right, out T _)
            where T : class, IEntity, new()
            where TKey : CandidateKey
        {
            Join(dbSet, left, right(dbSet._), DbJoinKind.RightJoin, out _);
            return this;
        }

        private void Join<T, TKey>(DbSet<T> dbSet, TKey left, TKey right, DbJoinKind kind, out T _)
            where T : class, IEntity, new()
            where TKey : CandidateKey
        {
            dbSet.VerifyNotNull(nameof(dbSet));
            left.VerifyNotNull(nameof(left));
            if (!_sourceModels.Contains(left.ParentModel))
                throw new ArgumentException(DiagnosticMessages.DbQueryBuilder_Join_InvalidLeftKey, nameof(left));
            right.VerifyNotNull(nameof(right));
            if (right.ParentModel != dbSet.Model)
                throw new ArgumentException(DiagnosticMessages.DbQueryBuilder_Join_InvalidRightKey, nameof(right));

            Join(dbSet, kind, left.UnsafeJoin(right), out _);
        }

        private void Join<T>(DbSet<T> dbSet, DbJoinKind kind, IReadOnlyList<ColumnMapping> relationship, out T _)
            where T : class, IEntity, new()
        {
            _ = (T)Join(dbSet._, kind, relationship);
        }

        private IEntity Join(IEntity _, DbJoinKind kind, IReadOnlyList<ColumnMapping> relationship)
        {
            Debug.Assert(relationship[0].Target.ParentModel == _.Model);

            var result = MakeAlias(_);
            var resultFromClause = result.Model.FromClause;
            if (result != _)
            {
                resultFromClause = resultFromClause.Clone(result.Model);
                relationship = relationship.Select(x => new ColumnMapping(x.SourceExpression, result.Model.Columns[x.Target.Ordinal])).ToList();
            }

            AddSourceModel(result.Model);
            FromClause = new DbJoinClause(kind, FromClause, resultFromClause, EliminateSubQuery(relationship));
            return result;
        }

        internal DbExpression EliminateSubQuery(DbExpression expression)
        {
            return _subQueryEliminator == null ? expression : _subQueryEliminator.GetExpressioin(expression);
        }

        private IReadOnlyList<ColumnMapping> EliminateSubQuery(IReadOnlyList<ColumnMapping> relationship)
        {
            if (relationship == null)
                return relationship;

            ColumnMapping[] result = null;
            for (int i = 0; i < relationship.Count; i++)
            {
                var mapping = relationship[i];
                var source = mapping.SourceExpression;
                var replacedSource = EliminateSubQuery(source);
                if (source != replacedSource  && result == null)
                {
                    result = new ColumnMapping[relationship.Count];
                    for (int j = 0; j < i; j++)
                        result[j] = relationship[j];
                }
                if (result != null)
                    result[i] = new ColumnMapping(replacedSource, mapping.TargetExpression.Column);
            }
            return result ?? relationship;
        }

        /// <summary>
        /// Constructs SQL CROSS JOIN.
        /// </summary>
        /// <typeparam name="T">Entity type of the target DbSet.</typeparam>
        /// <param name="dbSet">The target DbSet.</param>
        /// <param name="_">The entity object of the target DbSet for further SQL construction.</param>
        /// <returns>This query builder for fluent coding.</returns>
        public DbQueryBuilder CrossJoin<T>(DbSet<T> dbSet, out T _)
            where T : class, IEntity, new()
        {
            dbSet.VerifyNotNull(nameof(dbSet));
            Join(dbSet, DbJoinKind.CrossJoin, null, out _);
            return this;
        }

        private IEntity MakeAlias(IEntity _)
        {
            Debug.Assert(_ != null);

            if (!_sourceModels.Contains(_.Model))
                return _;

            return _.MakeCopy(true);
        }

        private static DbExpression And(DbExpression where1, DbExpression where2)
        {
            if (where1 == null)
                return where2;
            if (where2 == null)
                return where1;

            return new DbBinaryExpression(typeof(bool?), BinaryExpressionKind.And, where1, where2);
        }

        #endregion

        #region SELECT

        HashSet<Column> _targetColumns = new HashSet<Column>();
        List<ColumnMapping> _selectList = new List<ColumnMapping>();
        /// <summary>
        /// Gets a list of selected column mappings.
        /// </summary>
        public List<ColumnMapping> SelectList
        {
            get { return _selectList; }
        }

        /// <summary>
        /// Constructs SQL SELECT by automatically matching columns.
        /// </summary>
        /// <returns>This query builder for fluent coding.</returns>
        public DbQueryBuilder AutoSelect()
        {
            if (_autoColumnSelector == null)
                throw new InvalidOperationException(DiagnosticMessages.DbQueryBuilder_EmptyFrom);
            foreach (var targetColumn in Model.Columns)
            {
                if (_targetColumns.Contains(targetColumn))
                    continue;

                var sourceColumn = _autoColumnSelector.Select(targetColumn);
                if (sourceColumn != null)
                    SelectCore(sourceColumn, targetColumn);
            }
            return this;
        }

        /// <summary>
        /// Constructs SQL SELECT by automatically matching columns between specified source model and target projection.
        /// </summary>
        /// <param name="from">The source model.</param>
        /// <param name="to">The target projection.</param>
        /// <returns>This query builder for fluent coding.</returns>
        public DbQueryBuilder AutoSelect(Model from, Projection to)
        {
            to.VerifyNotNull(nameof(to));
            from.VerifyNotNull(nameof(from));
            if (_sourceModels == null)
                throw new InvalidOperationException(DiagnosticMessages.DbQueryBuilder_EmptyFrom);
            if (!_sourceModels.Contains(from))
                throw new ArgumentException(DiagnosticMessages.DbQueryBuilder_InvalidAutoSelectSourceModel, nameof(from));

            var targetColumns = to.Columns;
            for (int i = 0; i < targetColumns.Count; i++)
            {
                var targetColumn = targetColumns[i];
                if (_targetColumns.Contains(targetColumn))
                    continue;
                VerifyTargetColumn(targetColumn, string.Format(CultureInfo.InvariantCulture, "{0}.Columns[{1}]", nameof(to), i));
                var sourceColumn = from.Columns.AutoSelect(targetColumn);
                if (sourceColumn != null)
                    SelectCore(sourceColumn, targetColumn);
            }

            return this;
        }

        /// <summary>
        /// Constructs SQL SELECT by matching between specified source column and target column.
        /// </summary>
        /// <typeparam name="T">Type of the columns.</typeparam>
        /// <param name="from">The source column.</param>
        /// <param name="to">The target column.</param>
        /// <returns>This query builder for fluent coding.</returns>
        public DbQueryBuilder Select<T>(T from, T to)
            where T : Column, new()
        {
            VerifySourceColumn(from, nameof(from));
            VerifyTargetColumn(to, nameof(to));
            SelectCore(from, to);
            return this;
        }

        /// <summary>
        /// Constructs SQL SELECT by matching between specified source column and target new adhoc column.
        /// </summary>
        /// <typeparam name="T">Type of lthe column.</typeparam>
        /// <param name="from">The source column.</param>
        /// <param name="to">The target new adhoc model.</param>
        /// <param name="name">Name to create the adhoc column.</param>
        /// <returns>This query builder for fluent coding.</returns>
        public DbQueryBuilder Select<T>(T from, Adhoc to, string name = null)
            where T : Column, new()
        {
            return Select(from, to.AddColumn(from, false, c => c.DbColumnName = string.IsNullOrEmpty(name) ? from.DbColumnName : name));
        }

        private void VerifyTargetColumn(Column target, string paramName)
        {
            target.VerifyNotNull(paramName);
            if (target.ParentModel != Model || _targetColumns.Contains(target))
                throw new ArgumentException(DiagnosticMessages.DbQueryBuilder_VerifyTargetColumn, paramName);
        }

        private void SelectCore(Column source, Column target)
        {
            _targetColumns.Add(target);

            if (source == null)
            {
                SelectCore(source, DbConstantExpression.Null, target);
                return;
            }

            var sourceExpression = EliminateSubQuery(source.DbExpression);
            SelectCore(source, sourceExpression, target);
        }

        private void SelectCore(Column source, DbExpression sourceExpression, Column target)
        {
            SelectCore(sourceExpression, target);
            OnSelect(source, sourceExpression, target);
        }

        private void SelectCore(DbExpression source, Column target)
        {
            SelectList.Add(new ColumnMapping(source, target));
        }

        internal virtual void OnSelect(Column source, DbExpression sourceExpression, Column target)
        {
        }

        #endregion

        #region WHERE

        /// <summary>
        /// Gets the SQL WHERE expression.
        /// </summary>
        public DbExpression WhereExpression { get; private set; }

        /// <summary>
        /// Constructs SQL WHERE.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>This query builder for fluent coding.</returns>
        public DbQueryBuilder Where(_Boolean condition)
        {
            condition.VerifyNotNull(nameof(condition));
            VerifySourceColumn(condition, nameof(condition));
            WhereExpression = And(WhereExpression, EliminateSubQuery(condition.DbExpression));

            return this;
        }

        internal void VerifySourceColumn(Column column, string exceptionParamName)
        {
            VerifySourceColumn(column, exceptionParamName, this.GetType() == typeof(DbAggregateQueryBuilder));
        }

        internal void VerifySourceColumn(Column column, string paramName, bool allowsAggregate)
        {
            VerifySourceColumn(column, paramName, _sourceModels, allowsAggregate);
        }

        private void VerifySourceColumn(Column sourceColumn, string exceptionParamName, IModels sourceModelSet, bool allowsAggregate)
        {
            VerifyScalarSourceModels(sourceModelSet, sourceColumn, exceptionParamName);
            if (allowsAggregate)
                VerifyAggregateSourceModels(sourceColumn, exceptionParamName, sourceModelSet);
            else if (sourceColumn.AggregateSourceModels.Count > 0)
                throw new ArgumentException(DiagnosticMessages.DbQueryBuilder_AggregateNotAllowed, exceptionParamName);
        }

        private void VerifyScalarSourceModels(IModels containsBy, Column sourceColumn, string exceptionParamName)
        {
            if (sourceColumn.ScalarSourceModels.Count == 0 && sourceColumn.GetExpression() == null)
                throw new ArgumentException(DiagnosticMessages.Column_EmptyScalarSourceModels, exceptionParamName);

            foreach (var model in sourceColumn.ScalarSourceModels)
            {
                if (!containsBy.Contains(model))
                    throw new ArgumentException(DiagnosticMessages.DbQueryBuilder_InvalidScalarSourceModel(model), exceptionParamName);
            }
        }

        private static void VerifyAggregateSourceModels(Column sourceColumn, string exceptionParamName, IModels modelSet)
        {
            foreach (var model in sourceColumn.AggregateSourceModels)
            {
                if (!modelSet.Contains(model))
                    throw new ArgumentException(DiagnosticMessages.DbQueryBuilder_InvalidAggregateSourceModel(model), exceptionParamName);
            }
        }

        #endregion

        #region ORDER BY

        /// <summary>
        /// Gets the SQL ORDER BY list.
        /// </summary>
        public IReadOnlyList<DbExpressionSort> OrderByList { get; private set; }

        /// <summary>
        /// Gets a value specifies how many rows to skip within the query result.
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// Gets a value specifies how many rows to return in the query result.
        /// </summary>
        public int Fetch { get; private set; }

        /// <summary>
        /// Constructs SQL ORDER BY.
        /// </summary>
        /// <param name="orderByList">The order by list.</param>
        /// <returns>This query builder for fluent coding.</returns>
        public DbQueryBuilder OrderBy(params ColumnSort[] orderByList)
        {
            return OrderBy(-1, -1, orderByList);
        }

        /// <summary>
        /// Constructs SQL ORDER BY with specified offset and fetch.
        /// </summary>
        /// <param name="offset">Specifies how many rows to skip within the query result.</param>
        /// <param name="fetch">Specifies how many rows to skip within the query result.</param>
        /// <param name="orderByList">The order by list.</param>
        /// <returns>This query builder for fluent coding.</returns>
        public DbQueryBuilder OrderBy(int offset, int fetch, params ColumnSort[] orderByList)
        {
            orderByList.VerifyNotNull(nameof(orderByList));
            VerifyOrderByList(orderByList);
            VerifyOffsetFetch(offset, fetch);
            OrderByList = new ReadOnlyCollection<DbExpressionSort>(EliminateSubQuery(orderByList));
            Offset = offset;
            Fetch = fetch;

            return this;
        }

        private IList<DbExpressionSort> EliminateSubQuery(IList<ColumnSort> orderByList)
        {
            if (orderByList == null)
                return null;

            var result = new DbExpressionSort[orderByList.Count];
            for (int i = 0; i < orderByList.Count; i++)
            {
                var orderBy = orderByList[i];
                result[i] = new DbExpressionSort(EliminateSubQuery(orderBy.Column.DbExpression), orderBy.Direction);
            }

            return result;
        }

        private static void VerifyOffsetFetch(int offset, int fetch)
        {
            if (offset == -1 && fetch == -1)
                return;

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (fetch < -1 || fetch == 0)
                throw new ArgumentOutOfRangeException(nameof(fetch));
        }

        private void VerifyOrderByList(ColumnSort[] orderByList)
        {
            Debug.Assert(orderByList != null);
            for (int i = 0; i < orderByList.Length; i++)
            {
                var orderBy = orderByList[i];
                VerifySourceColumn(orderBy.Column, string.Format("orderByList[{0}]", i));
            }
        }

        #endregion

        internal virtual DbSelectStatement BuildSelectStatement(IReadOnlyList<ColumnMapping> select, DbFromClause from, DbExpression where, IReadOnlyList<DbExpressionSort> orderBy)
        {
            return new DbSelectStatement(Model, select, from, where, orderBy, Offset, Fetch);
        }

        private DbUnionStatement GetEliminatableUnionStatement(IReadOnlyList<ColumnMapping> selectList)
        {
            var fromQuery = FromClause as DbUnionStatement;
            if (fromQuery == null)
                return null;

            if (WhereExpression != null || OrderByList != null)
                return null;

            var fromColumns = fromQuery.Model.Columns;
            if (selectList.Count != fromColumns.Count)
                return null;

            for (int i = 0; i < selectList.Count; i++)
            {
                if (selectList[i].SourceExpression != fromColumns[i].DbExpression)
                    return null;
            }
            return fromQuery;
        }

        private DbUnionStatement EliminateUnionSubQuery(IReadOnlyList<ColumnMapping> selectList)
        {
            var fromQuery = GetEliminatableUnionStatement(selectList);
            return fromQuery == null ? null : new DbUnionStatement(Model, fromQuery.Query1, fromQuery.Query2, fromQuery.Kind);
        }

        private bool CanEliminateUnionSubQuery(IReadOnlyList<ColumnMapping> selectList)
        {
            return GetEliminatableUnionStatement(selectList) != null;
        }

        /// <summary>
        /// Constructs SQL SELECT by matching between specified source column and target column, without type safety.
        /// </summary>
        /// <param name="from">The source column.</param>
        /// <param name="to">The target column.</param>
        /// <returns>This query builder for fluent coding.</returns>
        public DbQueryBuilder UnsafeSelect(Column from, Column to)
        {
            SelectCore(from, to);
            return this;
        }
    }
}
