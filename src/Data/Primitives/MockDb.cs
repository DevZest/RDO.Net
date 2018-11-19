using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using DevZest.Data.Addons;

namespace DevZest.Data.Primitives
{
    public abstract class MockDb : IMockDb
    {
        internal async Task InternalInitializeAsync(DbSession db, IProgress<string> progress, CancellationToken ct)
        {
            Debug.Assert(db != null);
            Debug.Assert(db.Mock == null);
            Debug.Assert(Db == null);
            Db = db;
            db.Mock = this;

            _isInitializing = true;
            CreateMockDb();
            Initialize();
            RemoveDependencyMockTables();
            RemoveDependencyForeignKeys();
            await CreateMockTablesAsync(progress, ct);
            _pendingMockTables.Clear();
            _isInitializing = false;
        }

        public DbSession Db { get; private set; }

        private void RemoveDependencyMockTables()
        {
            var toRemove = new List<MockTable>();
            foreach (var mockTable in _mockTables)
            {
                if (!_pendingMockTables.ContainsKey(mockTable.Table))
                    toRemove.Add(mockTable);
            }

            foreach (var mockTable in toRemove)
                _mockTables.Remove(mockTable);
        }

        private void RemoveDependencyForeignKeys()
        {
            var tableNames = new HashSet<string>();
            foreach (var mockTable in _mockTables)
                tableNames.Add(mockTable.Table.Name);

            foreach (var mockTable in _mockTables)
            {
                var model = mockTable.Table.Model;
                var foreignKeys = model.GetAddons<DbForeignKey>();
                foreach (var item in foreignKeys)
                {
                    if (!tableNames.Contains(item.ReferencedTableName))
                        model.BrutalRemoveAddon(((IAddon)item).Key);
                }
            }
        }

        internal virtual string GetTableDescription(IDbTable table)
        {
            return null;
        }

        private async Task CreateMockTablesAsync(IProgress<string> progress, CancellationToken ct)
        {
            foreach (var mockTable in _mockTables)
            {
                ct.ThrowIfCancellationRequested();
                var table = mockTable.Table;
                if (progress != null)
                    progress.Report(table.Name);
                await Db.CreateTableAsync(table.Model, false, ct);
                _pendingMockTables[table]?.Invoke();
            }
        }

        protected abstract void Initialize();

        private Dictionary<IDbTable, Action> _pendingMockTables = new Dictionary<IDbTable, Action>();

        internal void AddMockTable(IDbTable dbTable, Action action)
        {
            dbTable.VerifyNotNull(nameof(dbTable));
            if (dbTable.DbSession != Db)
                throw new ArgumentException(DiagnosticMessages.MockDb_InvalidTable, nameof(dbTable));
            if (!_isInitializing)
                throw new InvalidOperationException(DiagnosticMessages.MockDb_MockOnlyAllowedDuringInitialization);
            if (_pendingMockTables.ContainsKey(dbTable))
                throw new ArgumentException(DiagnosticMessages.MockDb_DuplicateTable(dbTable.Name), nameof(dbTable));

            _pendingMockTables.Add(dbTable, action);
        }

        private bool _isInitializing;
        private HashSet<string> _creatingTableNames = new HashSet<string>();

        private struct MockTable
        {
            public MockTable(string name, IDbTable table)
            {
                Name = name;
                Table = table;
            }

            public readonly string Name;
            public readonly IDbTable Table;
        }

        private sealed class MockTableCollection : KeyedCollection<string, MockTable>
        {
            protected override string GetKeyForItem(MockTable item)
            {
                return item.Name;
            }
        }

        private MockTableCollection _mockTables = new MockTableCollection();

        internal abstract void CreateMockDb();

        internal abstract string GetMockTableName(string name);

        DbTable<TModel> IMockDb.GetMockTable<TModel>(string tableName, params Func<TModel, DbForeignKey>[] foreignKeys)
        {
            if (_mockTables.Contains(tableName))
            {
                var result = _mockTables[tableName];
                var resultModelType = result.Table.GetType().GenericTypeArguments[0];
                if (typeof(TModel) != resultModelType)
                    throw new InvalidOperationException(DiagnosticMessages.MockDb_ModelTypeMismatch(typeof(TModel).FullName, resultModelType.FullName, tableName));
                return (DbTable<TModel>)result.Table;
            }

            if (!_isInitializing)
                return null;

            if (_creatingTableNames.Contains(tableName))
                throw new InvalidOperationException(DiagnosticMessages.MockDb_CircularReference(tableName));

            _creatingTableNames.Add(tableName);

            var model = new TModel().ApplyForeignKey(foreignKeys);
            var table = DbTable<TModel>.Create(model, Db, GetMockTableName(tableName));
            _mockTables.Add(new MockTable(tableName, table));
            _creatingTableNames.Remove(tableName);
            return table;
        }
    }
}
