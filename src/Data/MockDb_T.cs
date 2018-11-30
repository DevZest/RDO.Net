using DevZest.Data.Primitives;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace DevZest.Data
{
    public abstract class MockDb<T> : MockDb
        where T : DbSession
    {
        public new T Db
        {
            get { return (T)base.Db; }
        }

        public async Task<T> GenerateAsync(T db, CancellationToken ct = default(CancellationToken))
        {
            Verify(db, nameof(db));

            _isDbGenerator = true;
            await InternalInitializeAsync(db, null, ct);
            return db;
        }

        public async Task<T> InitializeAsync(T db, CancellationToken ct = default(CancellationToken))
        {
            Verify(db, nameof(db));

            await InternalInitializeAsync(db, null, ct);
            return db;
        }

        private void Verify(T db, string paramName)
        {
            db.VerifyNotNull(paramName);
            db.VerifyNotMocked();
            if (Db != null)
                throw new InvalidOperationException(DiagnosticMessages.MockDb_InitializeTwice);
        }

        protected void Mock<TModel>(DbTable<TModel> dbTable)
            where TModel : Model, new()
        {
            AddMockTable(dbTable, null);
        }

        protected void Mock<TModel>(DbTable<TModel> dbTable, Func<DataSet<TModel>> getDataSet)
            where TModel : Model, new()
        {
            getDataSet.VerifyNotNull(nameof(getDataSet));

            AddMockTable(dbTable, async (ct) => {
                var dataSet = getDataSet();
                if (dataSet != null)
                    await dbTable.Insert(dataSet).ExecuteAsync(ct);
            });
        }

        private object _tag;
        private bool _isDbGenerator;

        internal sealed override void CreateMockDb()
        {
            if (!_isDbGenerator)
                _tag = Db.CreateMockDb();
        }

        internal sealed override string GetMockTableName(string name)
        {
            return _isDbGenerator ? name : Db.GetMockTableName(name, _tag);
        }
    }
}
