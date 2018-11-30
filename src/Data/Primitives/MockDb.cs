using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using DevZest.Data.Addons;
using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Primitives
{
    public abstract class MockDb
    {
        internal async Task InternalInitializeAsync(DbSession db, string paramName, IProgress<MockDbProgress> progress, CancellationToken ct)
        {
            Verify(db, paramName);
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

        private void Verify(DbSession db, string paramName)
        {
            db.VerifyNotNull(paramName);
            db.VerifyNotMocked();
            if (Db != null)
                throw new InvalidOperationException(DiagnosticMessages.MockDb_InitializeTwice);
        }

        public DbSession Db { get; private set; }

        private void RemoveDependencyMockTables()
        {
            var toRemove = new List<IDbTable>();
            foreach (var table in _mockTables)
            {
                if (!_pendingMockTables.ContainsKey(table))
                    toRemove.Add(table);
            }

            foreach (var mockTable in toRemove)
                _mockTables.Remove(mockTable);
        }

        private void RemoveDependencyForeignKeys()
        {
            var tableNames = new HashSet<string>();
            foreach (var table in _mockTables)
                tableNames.Add(table.Name);

            foreach (var table in _mockTables)
            {
                var model = table.Model;
                var fkConstraints = model.GetAddons<DbForeignKeyConstraint>();
                foreach (var fkConstraint in fkConstraints)
                {
                    if (!tableNames.Contains(fkConstraint.ReferencedTableName))
                        model.BrutalRemoveAddon(((IAddon)fkConstraint).Key);
                }
            }
        }

        private async Task CreateMockTablesAsync(IProgress<MockDbProgress> progress, CancellationToken ct)
        {
            for (int i = 0; i < _mockTables.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var table = _mockTables[i];
                if (progress != null)
                    progress.Report(new MockDbProgress(table, i, _mockTables.Count));
                await Db.CreateTableAsync(table.Model, false, ct);
                var action = _pendingMockTables[table];
                if (action != null)
                    await action.Invoke(ct);
            }
        }

        protected abstract void Initialize();

        private Dictionary<IDbTable, Func<CancellationToken, Task>> _pendingMockTables = new Dictionary<IDbTable, Func<CancellationToken, Task>>();

        internal void AddMockTable(IDbTable dbTable, Func<CancellationToken, Task> action)
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

        private sealed class MockTableCollection : KeyedCollection<string, IDbTable>
        {
            protected override string GetKeyForItem(IDbTable item)
            {
                return item.Identifier;
            }
        }

        private MockTableCollection _mockTables = new MockTableCollection();

        internal abstract void CreateMockDb();

        internal abstract string GetMockTableName(string name);

        internal DbTable<T> GetMockTable<T>(string propertyName)
            where T : Model, new()
        {
            if (_mockTables.Contains(propertyName))
            {
                var result = _mockTables[propertyName];
                var resultModelType = result.GetType().GenericTypeArguments[0];
                if (typeof(T) != resultModelType)
                    throw new InvalidOperationException(DiagnosticMessages.MockDb_ModelTypeMismatch(typeof(T).FullName, resultModelType.FullName, propertyName));
                return (DbTable<T>)result;
            }

            if (!_isInitializing)
                return null;

            if (_creatingTableNames.Contains(propertyName))
                throw new InvalidOperationException(DiagnosticMessages.MockDb_CircularReference(propertyName));

            _creatingTableNames.Add(propertyName);

            var table = DbTable<T>.Create(new T(), Db, propertyName, Initialize);
            _mockTables.Add(table);
            _creatingTableNames.Remove(propertyName);
            return table;
        }

        private void Initialize<T>(DbTable<T> dbTable)
            where T : Model, new()
        {
            DbTablePropertyAttribute.WireupAttributes(dbTable);
            dbTable.Name = GetMockTableName(dbTable.Name);
        }
    }
}
