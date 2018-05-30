using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    public abstract class DbQueryStatement : DbFromClause
    {
        internal DbQueryStatement(Model model)
        {
            Debug.Assert(model != null);
            Model = model;
        }

        public Model Model { get; private set; }

        internal DbTable<KeyOutput> SequentialKeyTempTable
        {
            get { return Model.SequentialKeyTempTable; }
            set { Model.SequentialKeyTempTable = value; }
        }

        internal async Task EnsureSequentialTempTableCreatedAsync(DbSession dbSession, CancellationToken cancellationToken)
        {
            if (SequentialKeyTempTable == null)
                SequentialKeyTempTable = await CreateSequentialKeyTempTableAsync(dbSession, cancellationToken);
        }

        private async Task<DbTable<KeyOutput>> CreateSequentialKeyTempTableAsync(DbSession dbSession, CancellationToken cancellationToken)
        {
            var sequentialKeyModel = Model.CreateSequentialKey();
            var selectStatement = GetSequentialKeySelectStatement(sequentialKeyModel);
            return await selectStatement.ToTempTableAsync(sequentialKeyModel, dbSession, cancellationToken);
        }

        internal abstract DbSelectStatement GetSequentialKeySelectStatement(KeyOutput sequentialKeyModel);

        internal virtual DbQueryStatement BuildQueryStatement(Model model, Action<DbQueryBuilder> action, DbTable<KeyOutput> sequentialKeys)
        {
            return new DbQueryBuilder(model).BuildQueryStatement(this.Model, action, sequentialKeys);
        }

        internal abstract DbSelectStatement BuildToTempTableStatement();

        private async Task<DbTable<T>> ToTempTableAsync<T>(T model, DbSession dbSession, CancellationToken cancellationToken)
            where T : Model, new()
        {
            Debug.Assert(model == this.Model);
            Debug.Assert(model.DataSource == null);
            var name = dbSession.AssignTempTableName(model);
            var result = DbTable<T>.CreateTemp(model, dbSession, name);
            await dbSession.CreateTableAsync(model, name, null, true, cancellationToken);
            result.InitialRowCount = await dbSession.InsertAsync(BuildToTempTableStatement(), cancellationToken);
            return result;
        }

        internal virtual DbSelectStatement BuildInsertStatement(Model model, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> keyMappings, bool joinParent)
        {
            return new DbQueryBuilder(model).BuildInsertStatement(Model, columnMappings, keyMappings, joinParent);
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
