using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    public abstract class DbSession<TConnection, TTransaction, TCommand, TReader> : DbSession
        where TConnection : DbConnection
        where TTransaction : DbTransaction
        where TCommand : DbCommand
        where TReader : DbReader
    {
        protected DbSession(TConnection connection)
        {
            Check.NotNull(connection, nameof(connection));
            _connection = connection;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                CloseConnection();
            base.Dispose(disposing);
        }

        private TConnection _connection;
        public TConnection GetConnection()
        {
            return _connection;
        }

        protected void OpenConnection()
        {
            CreateConnectionInvoker().Open();
        }

        protected Task OpenConnectionAsync(CancellationToken cancellationToken)
        {
            return CreateConnectionInvoker().OpenAsync(cancellationToken);
        }

        protected void CloseConnection()
        {
            CreateConnectionInvoker().Close();
        }

        private DbConnectionInvoker<TConnection> CreateConnectionInvoker()
        {
            return new DbConnectionInvoker<TConnection>(this, GetConnection());
        }

        private Stack<TTransaction> _transactions = new Stack<TTransaction>();
        public sealed override int TransactionCount
        {
            get { return _transactions.Count; }
        }

        public TTransaction CurrentTransaction
        {
            get { return _transactions.Count > 0 ? _transactions.Peek() : null; }
        }

        public void ExecuteTransaction(IsolationLevel isolationLevel, Action action)
        {
            CreateTransactionInvoker(isolationLevel).Execute(_transactions, action);
        }

        protected abstract DbTransactionInvoker<TConnection, TTransaction> CreateTransactionInvoker(IsolationLevel isolationLevel);

        private DbNonQueryInvoker<TCommand> CreateNonQueryInvoker(TCommand command)
        {
            return new DbNonQueryInvoker<TCommand>(this, command);
        }

        protected int ExecuteNonQuery(TCommand command)
        {
            return CreateNonQueryInvoker(command).Execute();
        }

        protected Task<int> ExecuteNonQueryAsync(TCommand command, CancellationToken cancellationToken)
        {
            return CreateNonQueryInvoker(command).ExecuteAsync(cancellationToken);
        }

        protected internal abstract TCommand GetCreateTableCommand(Model model, string tableName, bool isTempTable);

        internal sealed override void CreateTable(Model model, string tableName, bool isTempTable)
        {
            ExecuteNonQuery(GetCreateTableCommand(model, tableName, isTempTable));
        }

        internal sealed override Task CreateTableAsync(Model model, string tableName, bool isTempTable, CancellationToken cancellationToken)
        {
            return ExecuteNonQueryAsync(GetCreateTableCommand(model, tableName, isTempTable), cancellationToken);
        }

        private DbReaderInvoker<TCommand, TReader> CreateReaderInvoker(DbQueryStatement queryStatement)
        {
            return CreateReaderInvoker(GetQueryCommand(queryStatement), queryStatement.Model);
        }

        protected abstract TCommand GetQueryCommand(DbQueryStatement queryStatement);

        protected abstract DbReaderInvoker<TCommand, TReader> CreateReaderInvoker(TCommand command, Model model);

        internal sealed override DbReader ExecuteDbReader<T>(DbSet<T> dbSet)
        {
            return ExecuteReader(dbSet);
        }

        internal sealed override async Task<DbReader> ExecuteDbReaderAsync<T>(DbSet<T> dbSet, CancellationToken cancellationToken)
        {
            return await ExecuteReaderAsync(dbSet, cancellationToken);
        }

        public TReader ExecuteReader<T>(DbSet<T> dbSet)
            where T : Model, new()
        {
            Check.NotNull(dbSet, nameof(dbSet));
            return CreateReaderInvoker(dbSet.QueryStatement).Execute();
        }

        public Task<TReader> ExecuteReaderAsync<T>(DbSet<T> dbSet)
            where T : Model, new()
        {
            return ExecuteReaderAsync<T>(dbSet, CancellationToken.None);
        }

        public Task<TReader> ExecuteReaderAsync<T>(DbSet<T> dbSet, CancellationToken cancellationToken)
            where T : Model, new()
        {
            Check.NotNull(dbSet, nameof(dbSet));
            return CreateReaderInvoker(dbSet.QueryStatement).ExecuteAsync(cancellationToken);
        }

        protected virtual DbLogger<TConnection, TTransaction, TCommand, TReader> CreateDbLogger()
        {
            return new DbLogger<TConnection, TTransaction, TCommand, TReader>();
        }

        private DbLogger<TConnection, TTransaction, TCommand, TReader> CurrentDbLogger
        {
            get { return this.GetInterceptor<DbLogger<TConnection, TTransaction, TCommand, TReader>>(); }
        }

        public void SetLog(Action<string> value)
        {
            SetLog(value, LogCategory.All);
        }

        public void SetLog(Action<string> value, LogCategory logCategory)
        {
            var currentDbLogger = CurrentDbLogger;
            if (value == null)
            {
                if (currentDbLogger != null)
                    this.RemoveInterceptor(currentDbLogger.FullName);
            }
            else
            {
                if (currentDbLogger == null)
                {
                    currentDbLogger = CreateDbLogger();
                    this.AddInterceptor(currentDbLogger);
                }
                currentDbLogger.WriteAction = value;
                currentDbLogger.LogCategory = logCategory;
            }
        }

        internal sealed override void RecursiveFillDataSet(IDbSet dbSet, DataSet dataSet)
        {
            RecursiveFillDataSet(dbSet, dataSet.Model);
        }

        internal sealed override Task RecursiveFillDataSetAsync(IDbSet dbSet, DataSet dataSet, CancellationToken cancellationToken)
        {
            return RecursiveFillDataSetAsync(dbSet, dataSet.Model, cancellationToken);
        }

        private void RecursiveFillDataSet(IDbSet dbSet, Model dataSetModel)
        {
            using (var reader = CreateReaderInvoker(dbSet.SequentialQueryStatement).Execute())
            {
                var columns = GetReaderColumns(dataSetModel);
                var parentRowIdColumn = reader.Model.GetSysParentRowIdColumn(createIfNotExist: false);
                var prevParentRowId = -1;
                DataSet dataSet = null;

                while (reader.Read())
                {
                    var parentRowId = object.ReferenceEquals(parentRowIdColumn, null) ? 0 : parentRowIdColumn[reader].Value;
                    if (parentRowId != prevParentRowId)
                        dataSet = GetDataSet(dataSetModel, parentRowId);
                    NewDataRow(dataSet, columns, reader);
                }
            }
            FillChildrenDataSet(dbSet, dataSetModel);
        }

        private async Task RecursiveFillDataSetAsync(IDbSet dbSet, Model dataSetModel, CancellationToken cancellationToken)
        {
            using (var reader = await CreateReaderInvoker(dbSet.SequentialQueryStatement).ExecuteAsync(cancellationToken))
            {
                var columns = GetReaderColumns(dataSetModel);
                var parentRowIdColumn = reader.Model.GetSysParentRowIdColumn(createIfNotExist: false);
                var prevParentRowId = -1;
                DataSet dataSet = null;

                while (await reader.ReadAsync(cancellationToken))
                {
                    var parentRowId = object.ReferenceEquals(parentRowIdColumn, null) ? 0 : parentRowIdColumn[reader].Value;
                    if (parentRowId != prevParentRowId)
                        dataSet = GetDataSet(dataSetModel, parentRowId);
                    NewDataRow(dataSet, columns, reader);
                }
            }
            await FillChildrenDataSetAsync(dbSet, dataSetModel, cancellationToken);
        }

        private static DataSet GetDataSet(Model dataSetModel, int parentRowId)
        {
            if (parentRowId == 0)
            {
                Debug.Assert(dataSetModel.ParentModel == null);
                return dataSetModel.DataSet;
            }
            else
            {
                var parentRow = dataSetModel.ParentModel.DataSet[parentRowId - 1];
                return parentRow[dataSetModel.Ordinal];
            }
        }

        private static void NewDataRow(DataSet dataSet, IList<IColumn<TReader>> columns, TReader reader)
        {
            var model = dataSet.Model;
            model.EnterDataSetInitialization();

            var dataRow = dataSet.AddRow();
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                if (!column.IsReadOnly(dataRow.Ordinal))
                    column.Read(reader, dataRow);
            }

            model.ExitDataSetInitialization();
        }

        private void FillChildrenDataSet(IDbSet dbSet, Model dataSetModel)
        {
            var dbSetChildModels = dbSet.Model.ChildModels;
            var dataSetChildModels = dataSetModel.ChildModels;
            for (int i = 0; i < dbSetChildModels.Count; i++)
            {
                var childDbSet = dbSetChildModels[i].DataSource as IDbSet;
                if (childDbSet == null)
                    continue;

                var childDataSetModel = dataSetChildModels[i];
                if (childDataSetModel == null)
                    continue;

                RecursiveFillDataSet(childDbSet, childDataSetModel);
            }
        }

        private async Task FillChildrenDataSetAsync(IDbSet dbSet, Model dataSetModel, CancellationToken cancellationToken)
        {
            var dbSetChildModels = dbSet.Model.ChildModels;
            var dataSetChildModels = dataSetModel.ChildModels;
            for (int i = 0; i < dbSetChildModels.Count; i++)
            {
                var childDbSet = dbSetChildModels[i].DataSource as IDbSet;
                if (childDbSet == null)
                    continue;

                var childDataSetModel = dataSetChildModels[i];
                if (childDataSetModel == null)
                    continue;

                await RecursiveFillDataSetAsync(childDbSet, childDataSetModel, cancellationToken);
            }
        }

        private static IList<IColumn<TReader>> GetReaderColumns(Model model)
        {
            var columns = model.Columns;
            var result = new IColumn<TReader>[columns.Count];
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i] as IColumn<TReader>;
                if (column == null)
                    throw new NotSupportedException(Strings.DbSession_ColumnNotSupported(i, columns[i].Name));
                result[i] = column;
            }
            return result;
        }

        internal sealed override void FillDataSet(IDbSet dbSet, DataSet dataSet)
        {
            using (var reader = CreateReaderInvoker(dbSet.QueryStatement).Execute())
            {
                var columns = GetReaderColumns(dataSet.Model);

                while (reader.Read())
                    NewDataRow(dataSet, columns, reader);
            }

            FillChildrenDataSet(dbSet, dataSet.Model);
        }

        internal sealed override async Task FillDataSetAsync(IDbSet dbSet, DataSet dataSet, CancellationToken cancellationToken)
        {
            using (var reader = await CreateReaderInvoker(dbSet.QueryStatement).ExecuteAsync(cancellationToken))
            {
                var columns = GetReaderColumns(dataSet.Model);

                while (await reader.ReadAsync(cancellationToken))
                    NewDataRow(dataSet, columns, reader);
            }

            await FillChildrenDataSetAsync(dbSet, dataSet.Model, cancellationToken);
        }

        private sealed class ScalarIdentityOutput : Model
        {
            // This will NOT work. Static field is initialized before static constructor!
            //public static readonly ScalarIdentityOutput Singleton = new ScalarIdentityOutput();

            private static ScalarIdentityOutput s_singleton;
            public static ScalarIdentityOutput Singleton
            {
                get
                {
                    if (s_singleton == null)
                        s_singleton = new ScalarIdentityOutput();
                    return s_singleton;
                }
            }

            static ScalarIdentityOutput()
            {
                RegisterColumn((ScalarIdentityOutput x) => x.IdentityValue);
            }

            private ScalarIdentityOutput()
            {
            }

            public _Int32 IdentityValue { get; private set; }
        }

        internal sealed override InsertScalarResult InsertScalar(DbSelectStatement statement, bool outputIdentity)
        {
            var command = GetInsertScalarCommand(statement, outputIdentity);
            if (!outputIdentity)
            {
                var rowCount = ExecuteNonQuery(command);
                return new InsertScalarResult(rowCount > 0, null);
            }

            var model = ScalarIdentityOutput.Singleton;
            int? identityValue = null;
            using (var reader = CreateReaderInvoker(command, model).Execute())
            {
                if (reader.Read())
                    identityValue = model.IdentityValue[reader];
            }

            var sucess = identityValue.HasValue;
            return new InsertScalarResult(sucess, identityValue);
        }

        internal sealed override async Task<InsertScalarResult> InsertScalarAsync(DbSelectStatement statement, bool outputIdentity, CancellationToken cancellationToken)
        {
            var command = GetInsertScalarCommand(statement, outputIdentity);
            if (!outputIdentity)
            {
                var rowCount = await ExecuteNonQueryAsync(command, cancellationToken);
                return new InsertScalarResult(rowCount > 0, null);
            }

            var model = ScalarIdentityOutput.Singleton;
            int? identityValue = null;
            using (var reader = await CreateReaderInvoker(command, model).ExecuteAsync(cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                    identityValue = model.IdentityValue[reader];
            }

            var sucess = identityValue.HasValue;
            return new InsertScalarResult(sucess, identityValue);
        }

        protected abstract TCommand GetInsertScalarCommand(DbSelectStatement statement, bool outputIdentity);

        private int ExecuteTableEdit(DbSelectStatement statement, Func<DbSelectStatement, TCommand> getCommand)
        {
            var command = getCommand(statement);
            var result = ExecuteNonQuery(command);
            if (result != 0)
                statement.Model.DataSource.UpdateRevision();
            return result;
        }

        private async Task<int> ExecuteTableEditAsync(DbSelectStatement statement, Func<DbSelectStatement, TCommand> getCommand, CancellationToken cancellationToken)
        {
            var command = getCommand(statement);
            var result = await ExecuteNonQueryAsync(command, cancellationToken);
            if (result != 0)
                statement.Model.DataSource.UpdateRevision();
            return result;
        }

        internal sealed override int Insert(DbSelectStatement statement)
        {
            return ExecuteTableEdit(statement, GetInsertCommand);
        }

        internal sealed override Task<int> InsertAsync(DbSelectStatement statement, CancellationToken cancellationToken)
        {
            return ExecuteTableEditAsync(statement, GetInsertCommand, cancellationToken);
        }

        protected abstract TCommand GetInsertCommand(DbSelectStatement statement);

        internal sealed override int Update(DbSelectStatement statement)
        {
            return ExecuteTableEdit(statement, GetUpdateCommand);
        }

        internal sealed override Task<int> UpdateAsync(DbSelectStatement statement, CancellationToken cancellationToken)
        {
            return ExecuteTableEditAsync(statement, GetUpdateCommand, cancellationToken);
        }

        protected internal abstract TCommand GetUpdateCommand(DbSelectStatement statement);

        internal sealed override int Delete(DbSelectStatement statement)
        {
            return ExecuteTableEdit(statement, GetDeleteCommand);
        }

        internal sealed override Task<int> DeleteAsync(DbSelectStatement statement, CancellationToken cancellationToken)
        {
            return ExecuteTableEditAsync(statement, GetDeleteCommand, cancellationToken);
        }

        protected internal abstract TCommand GetDeleteCommand(DbSelectStatement statement);
    }
}
