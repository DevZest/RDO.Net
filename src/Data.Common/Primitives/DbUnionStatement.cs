using System;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public sealed class DbUnionStatement : DbQueryStatement
    {
        internal DbUnionStatement(Model model, DbQueryStatement query1, DbQueryStatement query2, DbUnionKind kind)
            : base(model)
        {
            Debug.Assert(query1 != null);
            Debug.Assert(query2 != null);

            Query1 = query1;
            Query2 = query2;
            Kind = kind;
        }

        public DbQueryStatement Query1 { get; private set; }

        public DbQueryStatement Query2 { get; private set; }

        public DbUnionKind Kind { get; private set; }

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
                selectItems[i] = sequentialKeyModel.Columns[i].CreateMapping(primaryKey[i].Column);

            return new DbSelectStatement(sequentialKeyModel, selectItems, this, null, null, null, null, -1, -1);
        }

        internal override DbQueryBuilder MakeQueryBuilder(Model model, bool isSequential)
        {
            var sequentialKeyIdentity = isSequential ? SequentialKeyTempTable.Model.GetIdentity(true) : null;
            return DbQueryBuilder.SelectAll(model, this, sequentialKeyIdentity);
        }

        internal override DbSelectStatement BuildToTempTableStatement(IDbTable dbTable)
        {
            return new DbSelectStatement(Model, null, this, null, null, -1, -1);
        }

        internal override DbExpression GetSource(int ordinal)
        {
            return null;
        }
    }
}
