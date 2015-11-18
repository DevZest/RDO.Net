using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public sealed class DbSelectStatement : DbQueryStatement
    {
        public DbSelectStatement(Model model, IList<ColumnMapping> select, DbFromClause from, DbExpression where, IList<DbExpressionSort> orderBy, int offset, int fetch)
            : this(model, select, from, where, orderBy, offset, fetch, true)
        {
        }

        private DbSelectStatement(Model model, IList<ColumnMapping> select, DbFromClause from, DbExpression where, IList<DbExpressionSort> orderBy, int offset, int fetch, bool isSimple)
            : base(model)
        {
            Select = select == null ? null : new ReadOnlyCollection<ColumnMapping>(select);
            From = from;
            Where = where;
            OrderBy = orderBy == null ? null : new ReadOnlyCollection<DbExpressionSort>(orderBy);
            Offset = offset;
            Fetch = fetch;
            IsSimple = isSimple && offset == -1 && fetch == -1;
        }

        public DbSelectStatement(Model model, IList<ColumnMapping> select, DbFromClause from, DbExpression where, IList<DbExpression> groupBy, DbExpression having, IList<DbExpressionSort> orderBy, int offset, int fetch)
            : this(model, select, from, where, orderBy, offset, fetch, false)
        {
            GroupBy = groupBy == null ? null : new ReadOnlyCollection<DbExpression>(groupBy);
            Having = having;
        }

        public bool IsAggregate
        {
            get { return GroupBy != null; }
        }

        public bool IsSimple { get; private set; }

        public ReadOnlyCollection<ColumnMapping> Select { get; private set; }

        public DbFromClause From { get; private set; }

        public DbExpression Where { get; private set; }

        public ReadOnlyCollection<DbExpression> GroupBy { get; private set; }

        public DbExpression Having { get; private set; }

        public ReadOnlyCollection<DbExpressionSort> OrderBy { get; private set; }

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
                selectItems[i] = new ColumnMapping(Select[primaryKey[i].Column.Ordinal].Source, sequentialKeyModel.Columns[i]);

            return new DbSelectStatement(sequentialKeyModel, selectItems, From, Where, GroupBy, Having, OrderBy, Offset, Fetch);
        }

        internal override DbQueryStatement BuildQueryStatement(Model model, Action<DbQueryBuilder> action, DbTable<KeyOutput> sequentialKeys)
        {
            var queryBuilder = IsAggregate ? new DbAggregateQueryBuilder(model) : new DbQueryBuilder(model);
            return queryBuilder.BuildQueryStatement(this, action, sequentialKeys);
        }

        internal override DbSelectStatement BuildInsertStatement(Model model, IList<ColumnMapping> columnMappings, IList<ColumnMapping> keyMappings, bool joinParent)
        {
            return IsSimple ? new DbQueryBuilder(model).BuildInsertStatement(this, columnMappings, keyMappings, joinParent)
                : base.BuildInsertStatement(model, columnMappings, keyMappings, joinParent);
        }

        internal override DbSelectStatement BuildUpdateStatement(Model model, IList<ColumnMapping> columnMappings, IList<ColumnMapping> keyMappings)
        {
            return IsSimple ? new DbQueryBuilder(model).BuildUpdateStatement(this, columnMappings, keyMappings)
                : base.BuildUpdateStatement(model, columnMappings, keyMappings);
        }

        internal override DbSelectStatement BuildDeleteStatement(Model model, IList<ColumnMapping> keyMappings)
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
                if (!select.TargetColumn.IsSystem)
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
