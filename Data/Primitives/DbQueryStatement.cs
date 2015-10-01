using DevZest.Data.Resources;
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

        internal abstract DbQueryBuilder MakeQueryBuilder(DbSession dbSession, Model model, bool isSequential);

        internal DbTable<T> ToTempTable<T>(T model, DbSession dbSession)
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

        internal abstract DbSelectStatement BuildToTempTableStatement(IDbTable dbTable);

        internal async Task<DbTable<T>> ToTempTableAsync<T>(T model, DbSession dbSession, CancellationToken cancellationToken)
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

        internal DbSelectStatement BuildInsertStatement(IDbTable dbTable, IList<ColumnMapping> columnMappings, IList<ColumnMapping> keyMappings)
        {
            var statement = TryBuildSimpleSelect(dbTable, columnMappings);

            var select = statement == null ? columnMappings : statement.Select;
            var from = statement == null ? this : statement.From;
            var where = statement == null ? null : statement.Where;
            var orderBy = statement == null ? null : statement.OrderBy;
            var offset = statement == null ? -1 : statement.Offset;
            var fetch = statement == null ? -1 : statement.Fetch;

            var parentMappings = columnMappings.GetParentMappings(dbTable);
            if (parentMappings != null)
            {
                parentMappings = IfTransformSimpleSelect(statement != null, parentMappings);
                var parentTable = (IDbTable)dbTable.Model.DataSource;
                var parentRowIdSelect = IfTransformSimpleSelect(statement != null, new ColumnMapping[]
                {
                    new ColumnMapping(Model.GetSysParentRowIdColumn(createIfNotExist: false), parentTable.Model.GetSysRowIdColumn(createIfNotExist: false))
                });
                select = select.Concat(parentRowIdSelect).ToList();
                from = new DbJoinClause(DbJoinKind.InnerJoin, from, parentTable.FromClause, new ReadOnlyCollection<ColumnMapping>(parentMappings));
            }

            if (keyMappings != null)
            {
                keyMappings = IfTransformSimpleSelect(statement != null, keyMappings);
                from = new DbJoinClause(DbJoinKind.LeftJoin, from, dbTable.FromClause, new ReadOnlyCollection<ColumnMapping>(keyMappings));
                var isNullExpr = new DbFunctionExpression(FunctionKeys.IsNull, new DbExpression[] { keyMappings[0].Target.DbExpression });
                if (where == null)
                    where = isNullExpr;
                else
                    where = new DbBinaryExpression(BinaryExpressionKind.And, where, isNullExpr);
            }

            if (statement == null && ShouldOmitSelectList(dbTable, select))
                select = null;
            return new DbSelectStatement(dbTable.Model, select, from, where, orderBy, offset, fetch);
        }

        internal DbSelectStatement BuildUpdateStatement(IDbTable dbTable, IList<ColumnMapping> keyMappings, IList<ColumnMapping> columnMappings)
        {
            var statement = TryBuildSimpleSelect(dbTable, columnMappings);

            var select = statement == null ? columnMappings : statement.Select;
            var from = statement == null ? this : statement.From;
            var where = statement == null ? null : statement.Where;
            var orderBy = statement == null ? null : statement.OrderBy;
            var offset = statement == null ? -1 : statement.Offset;
            var fetch = statement == null ? -1 : statement.Fetch;

            keyMappings = IfTransformSimpleSelect(statement != null, keyMappings);
            from = new DbJoinClause(DbJoinKind.InnerJoin, from, dbTable.FromClause, new ReadOnlyCollection<ColumnMapping>(keyMappings));

            if (statement == null && ShouldOmitSelectList(dbTable, select))
                select = null;
            return new DbSelectStatement(dbTable.Model, select, from, where, orderBy, offset, fetch);
        }

        private bool ShouldOmitSelectList(IDbTable dbTable, IList<ColumnMapping> columnMappings)
        {
            var updatableColumns = dbTable.Model.GetUpdatableColumns().ToList();
            if (columnMappings.Count != updatableColumns.Count)
                return false;

            var selectColumns = Model.GetSelectColumns().ToList();
            var sourceColumns = columnMappings.Select(x => x.Source).ToList();
            if (sourceColumns.Count != selectColumns.Count)
                return false;

            for (int i = 0; i < sourceColumns.Count; i++)
            {
                if (sourceColumns[i] != selectColumns[i])
                    return false;
            }

            return true;
        }

        internal DbSelectStatement BuildDeleteStatement(IDbTable dbTable, IList<ColumnMapping> keyMappings)
        {
            var statement = TryBuildSimpleSelect(dbTable, null);

            var from = statement == null ? this : statement.From;
            var where = statement == null ? null : statement.Where;
            var orderBy = statement == null ? null : statement.OrderBy;
            var offset = statement == null ? -1 : statement.Offset;
            var fetch = statement == null ? -1 : statement.Fetch;

            keyMappings = IfTransformSimpleSelect(statement != null, keyMappings);
            from = new DbJoinClause(DbJoinKind.InnerJoin, from, dbTable.FromClause, new ReadOnlyCollection<ColumnMapping>(keyMappings));

            return new DbSelectStatement(dbTable.Model, null, from, where, orderBy, offset, fetch);
        }

        internal virtual DbSelectStatement TryBuildSimpleSelect(IDbTable dbTable, IList<ColumnMapping> columnMappings)
        {
            return null;
        }

        internal IList<ColumnMapping> IfTransformSimpleSelect(bool isSimpleSelect, IList<ColumnMapping> mappings)
        {
            return isSimpleSelect ? TransformSimpleSelect(mappings) : mappings;
        }

        internal virtual IList<ColumnMapping> TransformSimpleSelect(IList<ColumnMapping> mappings)
        {
            throw new NotSupportedException();
        }

        internal virtual DbQueryStatement RemoveSystemColumns()
        {
            return this;
        }

        internal abstract Column GetSourceColumn(int ordinal);
    }
}
