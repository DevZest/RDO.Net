using DevZest.Data.Primitives;
using System;
using DevZest.Data.Utilities;

namespace DevZest.Data
{
    public abstract class MockDb<T> : MockDb
        where T : DbSession
    {
        public new T Db
        {
            get { return (T)base.Db; }
        }

        public object Tag { get; private set; }

        public T Initialize(T db)
        {
            Check.NotNull(db, nameof(db));
            db.VerifyNotMocked();
            if (Db != null)
                throw new InvalidOperationException(DiagnosticMessages.MockDb_InitializeTwice);

            InternalInitialize(db, null);
            return db;
        }

        protected void Mock<TModel>(DbTable<TModel> dbTable)
            where TModel : Model, new()
        {
            Action action = null;
            AddMockTable(dbTable, action);
        }

        protected void Mock<TModel>(DbTable<TModel> dbTable, params DataSet<TModel>[] dataSets)
            where TModel : Model, new()
        {
            AddMockTable(dbTable, () => {
                foreach (var dataSet in dataSets)
                {
                    if (dataSet != null)
                        dbTable.Insert(dataSet);
                }
            });
        }

        internal sealed override void CreateMockDb()
        {
            Tag = Db.CreateMockDb();
        }

        internal sealed override string GetMockTableName(string name)
        {
            return Db.GetMockTableName(name, Tag);
        }
    }
}
