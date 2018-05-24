using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public sealed class DbSelectStatement : DbQueryStatement
    {
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
            IsSimple = isSimple && offset == -1 && fetch == -1;
        }

        public DbSelectStatement(Model model, IReadOnlyList<ColumnMapping> select, DbFromClause from, DbExpression where, IReadOnlyList<DbExpression> groupBy, DbExpression having, IReadOnlyList<DbExpressionSort> orderBy, int offset, int fetch)
            : this(model, select, from, where, orderBy, offset, fetch, false)
        {
            GroupBy = groupBy;
            Having = having;
        }

        public bool IsAggregate
        {
            get { return GroupBy != null; }
        }

        public bool IsSimple { get; private set; }

        public IReadOnlyList<ColumnMapping> Select { get; private set; }

        public DbFromClause From { get; private set; }

        public DbExpression Where { get; private set; }

        public IReadOnlyList<DbExpression> GroupBy { get; private set; }

        public DbExpression Having { get; private set; }

        public IReadOnlyList<DbExpressionSort> OrderBy { get; private set; }

        public int Offset { get; private set; }

        public int Fetch { get; private set; }

        public override void Accept(DbFromClauseVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override T Accept<T>(DbFromClauseVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        internal override DbSelectStatement GetSequentialKeySelectStatement(KeyOutput sequentialKeyModel)
        {
            var primaryKey = Model.PrimaryKey;
            Debug.Assert(primaryKey != null);

            var selectItems = new ColumnMapping[primaryKey.Count];
            for (int i = 0; i < selectItems.Length; i++)
                selectItems[i] = new ColumnMapping(Select[primaryKey[i].Column.Ordinal].SourceExpression, sequentialKeyModel.Columns[i]);

            return new DbSelectStatement(sequentialKeyModel, selectItems, From, Where, GroupBy, Having, OrderBy, Offset, Fetch);
        }

        internal override DbQueryStatement BuildQueryStatement(Model model, Action<DbQueryBuilder> action, DbTable<KeyOutput> sequentialKeys)
        {
            var queryBuilder = IsAggregate ? new DbAggregateQueryBuilder(model) : new DbQueryBuilder(model);
            return queryBuilder.BuildQueryStatement(this, action, sequentialKeys);
        }

        internal override DbSelectStatement BuildInsertStatement(Model model, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> keyMappings, bool joinParent)
        {
            return IsSimple ? new DbQueryBuilder(model).BuildInsertStatement(this, columnMappings, keyMappings, joinParent)
                : base.BuildInsertStatement(model, columnMappings, keyMappings, joinParent);
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

        internal override DbSelectStatement BuildToTempTableStatement()
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
