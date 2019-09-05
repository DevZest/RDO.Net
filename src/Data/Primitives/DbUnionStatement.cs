using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents SQL UNION statement.
    /// </summary>
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

        /// <summary>
        /// Gets the first query statement.
        /// </summary>
        public DbQueryStatement Query1 { get; private set; }

        /// <summary>
        /// Gets the second query statement.
        /// </summary>
        public DbQueryStatement Query2 { get; private set; }

        /// <summary>
        /// Gets the kind of SQL UNION.
        /// </summary>
        public DbUnionKind Kind { get; private set; }

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

            var select = new ColumnMapping[primaryKey.Count];
            for (int i = 0; i < select.Length; i++)
                select[i] = new ColumnMapping(primaryKey[i].Column, sequentialKey.Columns[i]);

            return new DbSelectStatement(sequentialKey, select, this, null, null, null, null, -1, -1);
        }

        /// <inheritdoc/>
        public override DbSelectStatement BuildToTempTableStatement()
        {
            return new DbSelectStatement(Model, null, this, null, null, -1, -1);
        }
    }
}
