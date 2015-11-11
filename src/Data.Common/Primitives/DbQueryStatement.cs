using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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

        internal DbTable<SequentialKeyModel> SequentialKeyTempTable
        {
            get { return Model.SequentialKeyTempTable; }
            set { Model.SequentialKeyTempTable = value; }
        }

        internal void EnsureSequentialTempTableCreated(DbSession dbSession)
        {
            if (SequentialKeyTempTable == null)
                SequentialKeyTempTable = CreateSequentialKeyTempTable(dbSession);
        }

        internal async Task EnsureSequentialTempTableCreatedAsync(DbSession dbSession, CancellationToken cancellationToken)
        {
            if (SequentialKeyTempTable == null)
                SequentialKeyTempTable = await CreateSequentialKeyTempTableAsync(dbSession, cancellationToken);
        }

        private DbTable<SequentialKeyModel> CreateSequentialKeyTempTable(DbSession dbSession)
        {
            var sequentialKeyModel = Model.CreateSequentialKey();
            var selectStatement = GetSequentialKeySelectStatement(sequentialKeyModel);
            return selectStatement.ToTempTable(sequentialKeyModel, dbSession);
        }

        private async Task<DbTable<SequentialKeyModel>> CreateSequentialKeyTempTableAsync(DbSession dbSession, CancellationToken cancellationToken)
        {
            var sequentialKeyModel = Model.CreateSequentialKey();
            var selectStatement = GetSequentialKeySelectStatement(sequentialKeyModel);
            return await selectStatement.ToTempTableAsync(sequentialKeyModel, dbSession, cancellationToken);
        }

        internal abstract DbSelectStatement GetSequentialKeySelectStatement(SequentialKeyModel sequentialKeyModel);

        internal virtual DbQueryStatement BuildQueryStatement(Model model, Action<DbQueryBuilder> action, DbTable<SequentialKeyModel> sequentialKeys)
        {
            return new DbQueryBuilder(model).BuildQueryStatement(this.Model, action, sequentialKeys);
        }

        internal abstract DbSelectStatement BuildToTempTableStatement(IDbTable dbTable);

        private DbTable<T> ToTempTable<T>(T model, DbSession dbSession)
            where T : Model, new()
        {
            Debug.Assert(model == this.Model);
            Debug.Assert(model.DataSource == null);
            var name = dbSession.AssignTempTableName(model);
            var result = DbTable<T>.CreateTemp(model, dbSession, name);
            dbSession.CreateTable(model, name, true);
            result.InitialRowCount = dbSession.Insert(BuildToTempTableStatement(result));
            return result;
        }

        private async Task<DbTable<T>> ToTempTableAsync<T>(T model, DbSession dbSession, CancellationToken cancellationToken)
            where T : Model, new()
        {
            Debug.Assert(model == this.Model);
            Debug.Assert(model.DataSource == null);
            var name = dbSession.AssignTempTableName(model);
            var result = DbTable<T>.CreateTemp(model, dbSession, name);
            await dbSession.CreateTableAsync(model, name, true, cancellationToken);
            result.InitialRowCount = await dbSession.InsertAsync(BuildToTempTableStatement(result), cancellationToken);
            return result;
        }

        internal virtual DbSelectStatement BuildInsertStatement(Model model, IList<ColumnMapping> columnMappings, IList<ColumnMapping> keyMappings)
        {
            return new DbQueryBuilder(model).BuildInsertStatement(Model, columnMappings, keyMappings);
        }

        internal virtual DbSelectStatement BuildUpdateStatement(Model model, IList<ColumnMapping> columnMappings, IList<ColumnMapping> keyMappings)
        {
            return new DbQueryBuilder(model).BuildUpdateStatement(Model, columnMappings, keyMappings);
        }

        internal virtual DbSelectStatement BuildDeleteStatement(Model model, IList<ColumnMapping> keyMappings)
        {
            return new DbQueryBuilder(model).BuildDeleteStatement(Model, keyMappings);
        }

        internal virtual DbQueryStatement RemoveSystemColumns()
        {
            return this;
        }

        internal abstract DbExpression GetSource(int ordinal);

        internal sealed override void OnClone(Model model)
        {
            Model = model;
        }
    }
}
