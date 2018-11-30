using DevZest.Data.Addons;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    public abstract partial class DbSession<TConnection, TTransaction, TCommand, TReader> : DbSession
        where TConnection : DbConnection
        where TTransaction : DbTransaction
        where TCommand : DbCommand
        where TReader : DbReader
    {
        protected DbSession(TConnection connection)
        {
            connection.VerifyNotNull(nameof(connection));
            Connection = connection;
        }

        public TConnection Connection { get; }

        public sealed override Task OpenConnectionAsync(CancellationToken cancellationToken)
        {
            return CreateConnectionInterceptorInvoker().OpenAsync(cancellationToken);
        }

        public sealed override void CloseConnection()
        {
            CreateConnectionInterceptorInvoker().Close();
        }

        private ConnectionInvoker CreateConnectionInterceptorInvoker()
        {
            return new ConnectionInvoker(this, Connection);
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

        public void ExecuteTransaction(Action action)
        {
            action.VerifyNotNull(nameof(action));
            CreateTransactionInvoker(null).Execute(_transactions, action);
        }

        public void ExecuteTransaction(IsolationLevel isolationLevel, Action action)
        {
            action.VerifyNotNull(nameof(action));
            CreateTransactionInvoker(isolationLevel).Execute(_transactions, action);
        }

        public Task ExecuteTransactionAsync(Func<Task> action)
        {
            action.VerifyNotNull(nameof(action));
            return CreateTransactionInvoker(null).ExecuteAsync(_transactions, action);
        }

        public Task ExecuteTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct)
        {
            action.VerifyNotNull(nameof(action));
            return CreateTransactionInvoker(null).ExecuteAsync(_transactions, action, ct);
        }

        protected abstract TransactionInvoker CreateTransactionInvoker(IsolationLevel? isolationLevel);

        private NonQueryCommandInvoker PrepareNonQueryCommandInvoker(TCommand command)
        {
            command.Transaction = CurrentTransaction;
            return new NonQueryCommandInvoker(this, command);
        }

        protected int ExecuteNonQuery(TCommand command)
        {
            return PrepareNonQueryCommandInvoker(command).Execute();
        }

        protected Task<int> ExecuteNonQueryAsync(TCommand command, CancellationToken cancellationToken)
        {
            return PrepareNonQueryCommandInvoker(command).ExecuteAsync(cancellationToken);
        }

        protected internal abstract TCommand GetCreateTableCommand(Model model, bool isTempTable);

        internal sealed override Task CreateTableAsync(Model model, bool isTempTable, CancellationToken cancellationToken)
        {
            return ExecuteNonQueryAsync(GetCreateTableCommand(model, isTempTable), cancellationToken);
        }

        private ReaderInvoker CreateReaderInvoker(DbQueryStatement queryStatement)
        {
            var command = GetQueryCommand(queryStatement);
            return PrepareReaderInvoker(queryStatement.Model, command);
        }

        protected abstract TCommand GetQueryCommand(DbQueryStatement queryStatement);

        private ReaderInvoker PrepareReaderInvoker(Model model, TCommand command)
        {
            command.Transaction = CurrentTransaction;
            return CreateReaderInvoker(model, command);
        }

        protected abstract ReaderInvoker CreateReaderInvoker(Model model, TCommand command);

        internal sealed override async Task<DbReader> ExecuteDbReaderAsync<T>(DbSet<T> dbSet, CancellationToken cancellationToken)
        {
            return await ExecuteReaderAsync(dbSet, cancellationToken);
        }


        public Task<TReader> ExecuteReaderAsync<T>(DbSet<T> dbSet, CancellationToken ct = default(CancellationToken))
            where T : class, IModelReference, new()
        {
            dbSet.VerifyNotNull(nameof(dbSet));
            return CreateReaderInvoker(dbSet.QueryStatement).ExecuteAsync(ct);
        }

        protected virtual Logger CreateLogger()
        {
            return new Logger();
        }

        private Logger CurrentLogger
        {
            get { return this.GetAddon<Logger>(); }
        }

        public sealed override void SetLog(Action<string> value, LogCategory logCategory)
        {
            var currentLogger = CurrentLogger;
            if (value == null)
            {
                if (currentLogger != null)
                    this.RemoveAddon(((IAddon)currentLogger).Key);
            }
            else
            {
                if (currentLogger == null)
                {
                    currentLogger = CreateLogger();
                    this.Add(currentLogger);
                }
                currentLogger.WriteAction = value;
                currentLogger.LogCategory = logCategory;
            }
        }

        internal sealed override Task RecursiveFillDataSetAsync(IDbSet dbSet, DataSet dataSet, CancellationToken cancellationToken)
        {
            return RecursiveFillDataSetAsync(dbSet, dataSet.Model, cancellationToken);
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
            model.SuspendIdentity();

            dataSet.AddRow(x =>
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    if (!column.IsReadOnly(x))
                        column.Read(reader, x);
                }
                x.IsPrimaryKeySealed = true;
            });

            model.ResumeIdentity();
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
                    throw new NotSupportedException(DiagnosticMessages.DbSession_ColumnNotSupported(i, columns[i].Name));
                result[i] = column;
            }
            return result;
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

            public _Int64 IdentityValue { get; private set; }
        }

        internal sealed override async Task<InsertScalarResult> InsertScalarAsync(DbSelectStatement statement, bool outputIdentity, CancellationToken cancellationToken)
        {
            var command = GetInsertScalarCommand(statement, outputIdentity);
            if (!outputIdentity)
            {
                var rowCount = await PrepareNonQueryCommandInvoker(command).ExecuteAsync(cancellationToken);
                return new InsertScalarResult(rowCount > 0, null);
            }

            var model = ScalarIdentityOutput.Singleton;
            long? identityValue = null;
            using (var reader = await PrepareReaderInvoker(model, command).ExecuteAsync(cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                    identityValue = model.IdentityValue[reader];
            }

            var sucess = identityValue.HasValue;
            return new InsertScalarResult(sucess, identityValue);
        }

        protected abstract TCommand GetInsertScalarCommand(DbSelectStatement statement, bool outputIdentity);

        private async Task<int> ExecuteTableEditAsync(DbSelectStatement statement, Func<DbSelectStatement, TCommand> getCommand, CancellationToken cancellationToken)
        {
            var command = getCommand(statement);
            var result = await ExecuteNonQueryAsync(command, cancellationToken);
            if (result != 0)
                statement.Model.DataSource.UpdateRevision();
            return result;
        }

        internal sealed override Task<int> InsertAsync(DbSelectStatement statement, CancellationToken cancellationToken)
        {
            return ExecuteTableEditAsync(statement, GetInsertCommand, cancellationToken);
        }

        protected abstract TCommand GetInsertCommand(DbSelectStatement statement);

        internal sealed override Task<int> UpdateAsync(DbSelectStatement statement, CancellationToken cancellationToken)
        {
            return ExecuteTableEditAsync(statement, GetUpdateCommand, cancellationToken);
        }

        protected internal abstract TCommand GetUpdateCommand(DbSelectStatement statement);

        internal sealed override Task<int> DeleteAsync(DbSelectStatement statement, CancellationToken cancellationToken)
        {
            return ExecuteTableEditAsync(statement, GetDeleteCommand, cancellationToken);
        }

        protected internal abstract TCommand GetDeleteCommand(DbSelectStatement statement);
    }
}
