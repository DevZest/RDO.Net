using DevZest.Data.Addons;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Base class to implement specific database session.
    /// </summary>
    /// <typeparam name="TConnection">Type of ADO.Net database connection.</typeparam>
    /// <typeparam name="TCommand">Type of ADO.Net database command.</typeparam>
    /// <typeparam name="TReader">Type of ADO.Net database reader.</typeparam>
    public abstract partial class DbSession<TConnection, TCommand, TReader> : DbSession
        where TConnection : DbConnection
        where TCommand : DbCommand
        where TReader : DbReader
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DbSession{TConnection, TCommand, TReader}"/> class.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        protected DbSession(TConnection connection)
        {
            connection.VerifyNotNull(nameof(connection));
            Connection = connection;
        }

        /// <summary>
        /// Gets the database connection.
        /// </summary>
        public new TConnection Connection { get; }

        internal sealed override DbConnection GetConnection()
        {
            return Connection;
        }

        /// <inheritdoc />
        public sealed override Task OpenConnectionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return CreateConnectionInterceptorInvoker().OpenAsync(cancellationToken);
        }

        /// <inheritdoc />
        public sealed override void CloseConnection()
        {
            CreateConnectionInterceptorInvoker().Close();
        }

        private ConnectionInvoker CreateConnectionInterceptorInvoker()
        {
            return new ConnectionInvoker(this, Connection);
        }

        /// <summary>
        /// Executes non query command.
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>The return code returned by command execution.</returns>
        protected Task<int> ExecuteNonQueryAsync(TCommand command, CancellationToken ct)
        {
            if (CurrentTransaction != null)
                return CurrentTransaction.ExecuteNonQueryAsync(command, ct);
            else
                return InternalExecuteNonQueryAsync(command, ct);
        }

        private Task<int> InternalExecuteNonQueryAsync(TCommand command, CancellationToken ct)
        {
            return new NonQueryCommandInvoker(this, command).ExecuteAsync(ct);
        }

        /// <summary>
        /// Gets the command to create database table.
        /// </summary>
        /// <param name="model">The model of the database table.</param>
        /// <param name="isTempTable">Specifies whether to create temporary database table.</param>
        /// <returns>The command to create database table.</returns>
        protected internal abstract TCommand GetCreateTableCommand(Model model, bool isTempTable);

        internal sealed override Task CreateTableAsync(Model model, bool isTempTable, CancellationToken cancellationToken)
        {
            return ExecuteNonQueryAsync(GetCreateTableCommand(model, isTempTable), cancellationToken);
        }

        /// <summary>
        /// Gets the command for database query statement.
        /// </summary>
        /// <param name="queryStatement">The query statement.</param>
        /// <returns>The command of query statement.</returns>
        protected abstract TCommand GetQueryCommand(DbQueryStatement queryStatement);

        /// <summary>
        /// Creates database reader invoker.
        /// </summary>
        /// <param name="model">The model of database recordset.</param>
        /// <param name="command">The command to be executed.</param>
        /// <returns>The database reader invoker.</returns>
        protected abstract ReaderInvoker CreateReaderInvoker(Model model, TCommand command);

        internal sealed override async Task<DbReader> ExecuteDbReaderAsync<T>(DbSet<T> dbSet, CancellationToken cancellationToken)
        {
            return await ExecuteReaderAsync(dbSet, cancellationToken);
        }

        /// <summary>
        /// Executes reader on database recordset.
        /// </summary>
        /// <param name="dbSet">The database recordset.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>The ADO.Net database reader.</returns>
        public Task<TReader> ExecuteReaderAsync(IDbSet dbSet, CancellationToken ct = default(CancellationToken))
        {
            dbSet.VerifyNotNull(nameof(dbSet));

            return ExecuteReaderAsync(dbSet.QueryStatement, ct);
        }

        private Task<TReader> ExecuteReaderAsync(DbQueryStatement queryStatement, CancellationToken ct)
        {
            var model = queryStatement.Model;
            var command = GetQueryCommand(queryStatement);
            return ExecuteReaderAsync(model, command, ct);
        }

        private Task<TReader> ExecuteReaderAsync(Model model, TCommand command, CancellationToken ct)
        {
            if (CurrentTransaction != null)
                return CurrentTransaction.ExecuteReaderAsync(model, command, ct);
            else
                return InternalExecuteReaderAsync(model, command, ct);
        }

        private Task<TReader> InternalExecuteReaderAsync(Model model, TCommand command, CancellationToken ct)
        {
            return CreateReaderInvoker(model, command).ExecuteAsync(ct);
        }

        /// <summary>
        /// Creates logger object.
        /// </summary>
        /// <returns>The created logger object.</returns>
        /// <remarks>Derived class can override this method to return custom logger.</remarks>
        protected virtual Logger CreateLogger()
        {
            return new Logger();
        }

        private Logger CurrentLogger
        {
            get { return this.GetAddon<Logger>(); }
        }

        /// <inheritdoc/>
        public sealed override void SetLogger(Action<string> value, LogCategory logCategory)
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
            using (var reader = await ExecuteReaderAsync(dbSet.SequentialQueryStatement, cancellationToken))
            {
                var columns = GetReaderColumns(dataSetModel);
                var parentRowIdColumn = reader.Model.GetSysParentRowIdColumn(createIfNotExist: false);
                var prevParentRowId = -1;
                DataSet dataSet = null;

                while (await reader.ReadAsync(cancellationToken))
                {
                    var parentRowId = parentRowIdColumn is null ? 0 : parentRowIdColumn[reader].Value;
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
                if (!(dbSetChildModels[i].DataSource is IDbSet childDbSet))
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
                result[i] = column ?? throw new NotSupportedException(DiagnosticMessages.DbSession_ColumnNotSupported(i, columns[i].Name));
            }
            return result;
        }

        internal sealed override async Task FillDataSetAsync(IDbSet dbSet, DataSet dataSet, CancellationToken cancellationToken)
        {
            using (var reader = await ExecuteReaderAsync(dbSet, cancellationToken))
            {
                var columns = GetReaderColumns(dataSet.Model);

                while (await reader.ReadAsync(cancellationToken))
                    NewDataRow(dataSet, columns, reader);
            }

            await FillChildrenDataSetAsync(dbSet, dataSet.Model, cancellationToken);
        }

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

        /// <summary>
        /// Gets the command to insert data into table.
        /// </summary>
        /// <param name="statement">The query statement of the DbTable insert.</param>
        /// <returns>The command.</returns>
        protected abstract TCommand GetInsertCommand(DbSelectStatement statement);

        internal sealed override Task<int> UpdateAsync(DbSelectStatement statement, CancellationToken cancellationToken)
        {
            return ExecuteTableEditAsync(statement, GetUpdateCommand, cancellationToken);
        }

        /// <summary>
        /// Gets the command to update table.
        /// </summary>
        /// <param name="statement">The query statement of the DbTable update.</param>
        /// <returns>The command.</returns>
        protected internal abstract TCommand GetUpdateCommand(DbSelectStatement statement);

        internal sealed override Task<int> DeleteAsync(DbSelectStatement statement, CancellationToken cancellationToken)
        {
            return ExecuteTableEditAsync(statement, GetDeleteCommand, cancellationToken);
        }

        /// <summary>
        /// Gets the command to delete data from table
        /// </summary>
        /// <param name="statement">The query statement of the DbTable delete.</param>
        /// <returns>The command.</returns>
        protected internal abstract TCommand GetDeleteCommand(DbSelectStatement statement);
    }
}
