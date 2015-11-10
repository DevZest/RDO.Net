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

        internal override DbSelectStatement GetSequentialKeySelectStatement(SequentialKeyModel sequentialKeyModel)
        {
            var primaryKey = Model.PrimaryKey;
            Debug.Assert(primaryKey != null);

            var selectItems = new ColumnMapping[primaryKey.Count];
            for (int i = 0; i < selectItems.Length; i++)
                selectItems[i] = new ColumnMapping(Select[primaryKey[i].Column.Ordinal].Source, sequentialKeyModel.Columns[i]);

            return new DbSelectStatement(sequentialKeyModel, selectItems, From, Where, GroupBy, Having, OrderBy, Offset, Fetch);
        }

        internal override DbQueryBuilder MakeQueryBuilder(Model model)
        {
            if (IsAggregate)
                return new DbAggregateQueryBuilder(model, this);
            else
                return new DbQueryBuilder(model, this);
        }
        internal sealed override DbSelectStatement TryBuildSimpleSelect(IDbTable dbTable, IList<ColumnMapping> columnMappings)
        {
            if (!IsSimple || FromClauseContains(dbTable))
                return null;
            var transformedColumnMappings = TransformSimpleSelect(columnMappings);
            return new DbSelectStatement(dbTable.Model, transformedColumnMappings, From, Where, OrderBy, Offset, Fetch);
        }

        private sealed class FromClauseContainsVisitor : DbFromClauseVisitor<bool>
        {
            public FromClauseContainsVisitor(IDbTable dbTable)
            {
                _dbTable = dbTable;
            }

            private IDbTable _dbTable;

            public override bool Visit(DbJoinClause join)
            {
                return join.Left.Accept(this) || join.Right.Accept(this);
            }

            public override bool Visit(DbSelectStatement select)
            {
                return false;
            }

            public override bool Visit(DbTableClause table)
            {
                return table.Name == _dbTable.Name;
            }

            public override bool Visit(DbUnionStatement union)
            {
                return false;
            }
        }

        private bool FromClauseContains(IDbTable dbTable)
        {
            return From.Accept(new FromClauseContainsVisitor(dbTable));
        }

        internal sealed override IList<ColumnMapping> TransformSimpleSelect(IList<ColumnMapping> mappings)
        {
            Debug.Assert(IsSimple);

            if (mappings == null)
                return null;

            var result = new ColumnMapping[mappings.Count];
            var columnReplacer = new ColumnReplacer(this);
            for (int i = 0; i < mappings.Count; i++)
            {
                var mapping = mappings[i];
                result[i] = new ColumnMapping(columnReplacer.Replace(mapping.Source), mapping.TargetColumn);
            }

            return result;
        }

        internal override DbSelectStatement BuildToTempTableStatement(IDbTable dbTable)
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

        internal override DbExpression GetSource(int ordinal)
        {
            return Select[ordinal].Source;
        }

        internal override SubQueryEliminator SubQueryEliminator
        {
            get { return IsSimple ? new SubQueryEliminator(this) : null; }
        }
    }
}
