﻿using System;
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

        public override DbSelectStatement GetSequentialKeySelectStatement(SequentialKey sequentialKey)
        {
            var primaryKey = Model.PrimaryKey;
            Debug.Assert(primaryKey != null);

            var select = new ColumnMapping[primaryKey.Count];
            for (int i = 0; i < select.Length; i++)
                select[i] = new ColumnMapping(primaryKey[i].Column, sequentialKey.Columns[i]);

            return new DbSelectStatement(sequentialKey, select, this, null, null, null, null, -1, -1);
        }

        public override DbSelectStatement BuildToTempTableStatement()
        {
            return new DbSelectStatement(Model, null, this, null, null, -1, -1);
        }
    }
}
