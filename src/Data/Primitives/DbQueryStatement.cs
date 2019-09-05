using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents SQL query statement.
    /// </summary>
    public abstract class DbQueryStatement : DbFromClause
    {
        internal DbQueryStatement(Model model)
        {
            Debug.Assert(model != null);
            Model = model;
        }

        /// <summary>
        /// Gets the model of this query.
        /// </summary>
        public Model Model { get; private set; }

        /// <summary>
        /// Gets the temporary table which contains the sequential ids and primary key of the parent database recordset.
        /// </summary>
        /// <remarks>Child query/temporary table joins this table automatically to enforce parent-child relationship.</remarks>
        public DbTable<SequentialKey> SequentialKeyTempTable
        {
            get { return Model.SequentialKeyTempTable; }
            internal set { Model.SequentialKeyTempTable = value; }
        }

        internal async Task EnsureSequentialTempTableCreatedAsync(DbSession dbSession, CancellationToken cancellationToken)
        {
            if (SequentialKeyTempTable == null)
                SequentialKeyTempTable = await CreateSequentialKeyTempTableAsync(dbSession, cancellationToken);
        }

        private async Task<DbTable<SequentialKey>> CreateSequentialKeyTempTableAsync(DbSession dbSession, CancellationToken cancellationToken)
        {
            var sequentialKey = new SequentialKey(Model);
            var selectStatement = GetSequentialKeySelectStatement(sequentialKey);
            return await selectStatement.ToTempTableAsync(sequentialKey, dbSession, cancellationToken);
        }

        /// <summary>
        /// Gets the SQL SELECT statement of the sequential key table.
        /// </summary>
        /// <param name="sequentialKey"></param>
        /// <returns></returns>
        public abstract DbSelectStatement GetSequentialKeySelectStatement(SequentialKey sequentialKey);

        internal virtual DbQueryStatement BuildQueryStatement(Model model, Action<DbQueryBuilder> action, DbTable<SequentialKey> sequentialKeys)
        {
            return new DbQueryBuilder(model).BuildQueryStatement(this.Model, action, sequentialKeys);
        }

        /// <summary>
        /// Builds SQL statement to create temporary table.
        /// </summary>
        /// <returns></returns>
        public abstract DbSelectStatement BuildToTempTableStatement();

        private async Task<DbTable<T>> ToTempTableAsync<T>(T model, DbSession dbSession, CancellationToken cancellationToken)
            where T : Model, new()
        {
            Debug.Assert(model == this.Model);
            Debug.Assert(model.DataSource == null);
            var name = dbSession.AssignTempTableName(model);
            var result = DbTable<T>.CreateTemp(model, dbSession, name);
            await dbSession.CreateTableAsync(model, true, cancellationToken);
            result.InitialRowCount = await dbSession.InsertAsync(BuildToTempTableStatement(), cancellationToken);
            return result;
        }

        internal virtual DbSelectStatement BuildInsertStatement(Model model, IReadOnlyList<ColumnMapping> columnMappings, bool joinParent)
        {
            return new DbQueryBuilder(model).BuildInsertStatement(Model, columnMappings, joinParent);
        }

        internal virtual DbSelectStatement BuildUpdateStatement(Model model, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join)
        {
            return new DbQueryBuilder(model).BuildUpdateStatement(Model, columnMappings, join);
        }

        internal virtual DbSelectStatement BuildDeleteStatement(Model model, IReadOnlyList<ColumnMapping> join)
        {
            return new DbQueryBuilder(model).BuildDeleteStatement(Model, join);
        }

        internal virtual DbQueryStatement RemoveSystemColumns()
        {
            return this;
        }

        internal sealed override void OnClone(Model model)
        {
            Model = model;
        }
    }
}
