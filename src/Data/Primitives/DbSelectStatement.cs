using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents SQL SELECT statement.
    /// </summary>
    public sealed class DbSelectStatement : DbQueryStatement
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DbSelectStatement"/> class.
        /// </summary>
        /// <param name="model">The model of this query.</param>
        /// <param name="select">The column mappings of the SELECT statement.</param>
        /// <param name="from">The FROM clause.</param>
        /// <param name="where">The WHERE expression.</param>
        /// <param name="orderBy">The ORDER BY list.</param>
        /// <param name="offset">Specifies how many rows to skip within the query result.</param>
        /// <param name="fetch">Specifies how many rows to return in the query result.</param>
        public DbSelectStatement(Model model, IReadOnlyList<ColumnMapping> select, DbFromClause from, DbExpression where, IReadOnlyList<DbExpressionSort> orderBy, int offset, int fetch)
            : this(model, select, from, where, orderBy, offset, fetch, true)
        {
        }

        private DbSelectStatement(Model model, IReadOnlyList<ColumnMapping> select, DbFromClause from, DbExpression where, IReadOnlyList<DbExpressionSort> orderBy, int offset, int fetch, bool isSimple)
            : base(model)
        {
            Select = select;
            From = from;
            Where = where;
            OrderBy = orderBy;
            Offset = offset;
            Fetch = fetch;
            IsSimple = isSimple && From != null && offset == -1 && fetch == -1;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DbSelectStatement"/> class.
        /// </summary>
        /// <param name="model">The model of this query.</param>
        /// <param name="select">The column mappings of the SELECT statement.</param>
        /// <param name="from">The FROM clause.</param>
        /// <param name="where">The WHERE expression.</param>
        /// <param name="groupBy">The GROUP BY list.</param>
        /// <param name="having">The HAVING expression.</param>
        /// <param name="orderBy">The ORDER BY list.</param>
        /// <param name="offset">Specifies how many rows to skip within the query result.</param>
        /// <param name="fetch">Specifies how many rows to return in the query result.</param>
        public DbSelectStatement(Model model, IReadOnlyList<ColumnMapping> select, DbFromClause from, DbExpression where, IReadOnlyList<DbExpression> groupBy, DbExpression having, IReadOnlyList<DbExpressionSort> orderBy, int offset, int fetch)
            : this(model, select, from, where, orderBy, offset, fetch, false)
        {
            GroupBy = groupBy;
            Having = having;
        }

        /// <summary>
        /// Gets a value indicating whether this is an aggregate query.
        /// </summary>
        public bool IsAggregate
        {
            get { return GroupBy != null; }
        }

        /// <summary>
        /// Gets a value indicating whether this is a simple query.
        /// </summary>
        /// <remarks>Simple query can be merged when used as FROM.</remarks>
        public bool IsSimple { get; private set; }

        /// <summary>
        /// Gets the column mappings of the SELECT statement.
        /// </summary>
        public IReadOnlyList<ColumnMapping> Select { get; private set; }

        /// <summary>
        /// Gets the FROM clause.
        /// </summary>
        public DbFromClause From { get; private set; }

        /// <summary>
        /// Gets the WHERE expression.
        /// </summary>
        public DbExpression Where { get; private set; }

        /// <summary>
        /// Gets the GROUP BY list.
        /// </summary>
        public IReadOnlyList<DbExpression> GroupBy { get; private set; }

        /// <summary>
        /// Gets the HAVING expression.
        /// </summary>
        public DbExpression Having { get; private set; }

        /// <summary>
        /// Gets the ORDER BY list.
        /// </summary>
        public IReadOnlyList<DbExpressionSort> OrderBy { get; private set; }

        /// <summary>
        /// Gets a value that specifies how many rows to skip within the query result.
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// Gets avalue that specifies how many rows to return in the query result.
        /// </summary>
        public int Fetch { get; private set; }

        /// <inheritdoc/>
        public override void Accept(DbFromClauseVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <inheritdoc/>
        public override T Accept<T>(DbFromClauseVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        /// <inheritdoc/>
        public override DbSelectStatement GetSequentialKeySelectStatement(SequentialKey sequentialKey)
        {
            var primaryKey = Model.PrimaryKey;
            Debug.Assert(primaryKey != null);

            var selectItems = new ColumnMapping[primaryKey.Count];
            for (int i = 0; i < selectItems.Length; i++)
                selectItems[i] = new ColumnMapping(Select[primaryKey[i].Column.Ordinal].SourceExpression, sequentialKey.Columns[i]);

            return new DbSelectStatement(sequentialKey, selectItems, From, Where, GroupBy, Having, OrderBy, Offset, Fetch);
        }

        internal override DbQueryStatement BuildQueryStatement(Model model, Action<DbQueryBuilder> action, DbTable<SequentialKey> sequentialKeys)
        {
            var queryBuilder = IsAggregate ? new DbAggregateQueryBuilder(model) : new DbQueryBuilder(model);
            return queryBuilder.BuildQueryStatement(this, action, sequentialKeys);
        }

        internal override DbSelectStatement BuildInsertStatement(Model model, IReadOnlyList<ColumnMapping> columnMappings, bool joinParent)
        {
            return IsSimple ? new DbQueryBuilder(model).BuildInsertStatement(this, columnMappings, joinParent)
                : base.BuildInsertStatement(model, columnMappings, joinParent);
        }

        internal override DbSelectStatement BuildUpdateStatement(Model model, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> keyMappings)
        {
            return IsSimple ? new DbQueryBuilder(model).BuildUpdateStatement(this, columnMappings, keyMappings)
                : base.BuildUpdateStatement(model, columnMappings, keyMappings);
        }

        internal override DbSelectStatement BuildDeleteStatement(Model model, IReadOnlyList<ColumnMapping> keyMappings)
        {
            return IsSimple ? new DbQueryBuilder(model).BuildDeleteStatement(this, keyMappings)
                : base.BuildDeleteStatement(model, keyMappings);
        }

        /// <inheritdoc/>
        public override DbSelectStatement BuildToTempTableStatement()
        {
            return this;
        }

        internal override DbQueryStatement RemoveSystemColumns()
        {
            if (Model.Columns.SystemColumnCount == 0 || IsAggregate)
                return this;

            var newSelect = new List<ColumnMapping>();

            foreach (var select in Select)
            {
                if (!select.Target.IsSystem)
                    newSelect.Add(select);
            }

            Debug.Assert(newSelect.Count < Select.Count);
            return new DbSelectStatement(Model, newSelect, From, Where, OrderBy, Offset, Fetch);
        }

        internal override SubQueryEliminator SubQueryEliminator
        {
            get { return IsSimple && IsDbQuery ? new SubQueryEliminator(this) : null; }
        }

        private bool IsDbQuery
        {
            get
            {
                var dataSource = Model.DataSource;
                return dataSource == null ? false : dataSource.Kind == DataSourceKind.DbQuery;
            }
        }
    }
}
