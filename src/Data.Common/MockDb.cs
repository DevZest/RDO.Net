using DevZest.Data.Primitives;
using System;
using DevZest.Data.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data
{
    public abstract class MockDb<T> : IMockDb
        where T : DbSession
    {
        public T Db { get; private set; }

        public object Tag { get; private set; }

        public T Initialize(T db)
        {
            Check.NotNull(db, nameof(db));
            if (Db != null)
                throw new InvalidOperationException(Strings.MockDb_InitializeTwice);

            Db = db;
            db.Mock = this;

            _isInitializing = true;
            CreateMockDb();
            Initialize();
            FinalizeInitialization();
            _isInitializing = false;

            return db;
        }

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
                Db.CreateTable(table.Model, table.Name, false);
                var action = _pendingMockTables[table];
                if (action != null)
                    action();
            }
        }

        protected abstract void Initialize();

        private Dictionary<IDbTable, Action> _pendingMockTables = new Dictionary<IDbTable, Action>();

        protected void Mock<TModel>(DbTable<TModel> dbTable)
            where TModel : Model, new()
        {
            Action action = null;
            Mock(dbTable, action);
        }

        protected void Mock<TModel>(DbTable<TModel> dbTable, params DataSet<TModel>[] dataSets)
            where TModel : Model, new()
        {
            Mock(dbTable, () => {
                foreach (var dataSet in dataSets)
                {
                    if (dataSet != null)
                        dbTable.Insert(dataSet);
                }
            });
        }

        private void Mock<TModel>(DbTable<TModel> dbTable, Action action)
            where TModel : Model, new()
        {
            Check.NotNull(dbTable, nameof(dbTable));
            if (dbTable.DbSession != Db)
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

        protected virtual void CreateMockDb()
        {
            Tag = Db.CreateMockDb();
        }

        protected virtual string GetMockTableName(string name)
        {
            return Db.GetMockTableName(name, Tag);
        }

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
            var table = DbTable<TModel>.Create(model, Db, GetMockTableName(tableName));
            _mockTables.Add(new MockTable(tableName, table));
            _creatingTableNames.Remove(tableName);
            return table;
        }
    }
}
