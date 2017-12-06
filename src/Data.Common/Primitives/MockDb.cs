using System;
using DevZest.Data.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public abstract class MockDb : IMockDb
    {
        internal void InternalInitialize(DbSession db)
        {
            Debug.Assert(db != null);
            Debug.Assert(db.Mock == null);
            Debug.Assert(_db == null);
            _db = db;
            db.Mock = this;

            _isInitializing = true;
            CreateMockDb();
            Initialize();
            FinalizeInitialization();
            _isInitializing = false;
        }

        private DbSession _db;

        private void FinalizeInitialization()
        {
            RemoveDependencyMockTables();
            RemoveDependencyForeignKeys();
            CreateMockTables();
            _pendingMockTables.Clear();
        }

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
                var foreignKeys = model.GetExtensions<ForeignKeyConstraint>();
                foreach (var item in foreignKeys)
                {
                    if (!tableNames.Contains(item.ReferencedTableName))
                        model.BrutalRemoveExtension(((IExtension)item).Key);
                }
            }
        }

        private void CreateMockTables()
        {
            foreach (var mockTable in _mockTables)
            {
                var table = mockTable.Table;
                _db.CreateTable(table.Model, table.Name, false);
                var action = _pendingMockTables[table];
                if (action != null)
                    action();
            }
        }

        protected abstract void Initialize();

        private Dictionary<IDbTable, Action> _pendingMockTables = new Dictionary<IDbTable, Action>();

        internal void AddMockTable<TModel>(DbTable<TModel> dbTable, Action action)
            where TModel : Model, new()
        {
            Check.NotNull(dbTable, nameof(dbTable));
            if (dbTable.DbSession != _db)
                throw new ArgumentException(Strings.MockDb_InvalidTable, nameof(dbTable));
            if (!_isInitializing)
                throw new InvalidOperationException(Strings.MockDb_MockOnlyAllowedDuringInitialization);
            if (_pendingMockTables.ContainsKey(dbTable))
                throw new ArgumentException(Strings.MockDb_DuplicateTable(dbTable.Name), nameof(dbTable));

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

        DbTable<TModel> IMockDb.GetMockTable<TModel>(string tableName, params Func<TModel, ForeignKeyConstraint>[] foreignKeys)
        {
            if (_mockTables.Contains(tableName))
            {
                var result = _mockTables[tableName];
                var resultModelType = result.Table.GetType().GenericTypeArguments[0];
                if (typeof(TModel) != resultModelType)
                    throw new InvalidOperationException(Strings.MockDb_ModelTypeMismatch(typeof(TModel).FullName, resultModelType.FullName, tableName));
                return (DbTable<TModel>)result.Table;
            }

            if (!_isInitializing)
                return null;

            if (_creatingTableNames.Contains(tableName))
                throw new InvalidOperationException(Strings.MockDb_CircularReference(tableName));

            _creatingTableNames.Add(tableName);

            var model = new TModel().ApplyForeignKey(foreignKeys);
            var table = DbTable<TModel>.Create(model, _db, GetMockTableName(tableName));
            _mockTables.Add(new MockTable(tableName, table));
            _creatingTableNames.Remove(tableName);
            return table;
        }
    }
}
