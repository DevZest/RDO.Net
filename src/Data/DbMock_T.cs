using DevZest.Data.Primitives;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Data;

namespace DevZest.Data
{
    public abstract class DbMock<T> : DbGenerator, IDbGenerator<T>
        where T : DbSession
    {
        public new T Db
        {
            get { return (T)base.Db; }
        }

        public async Task<T> GenerateAsync(T db, IProgress<DbGenerationProgress> progress = null, CancellationToken ct = default(CancellationToken))
        {
            db.VerifyNotNull(nameof(db));

            _isDbGeneration = true;
            await InitializeAsync(db, nameof(db), progress, ct);
            return db;
        }

        protected async Task<T> MockAsync(T db, IProgress<DbGenerationProgress> progress = null, CancellationToken ct = default(CancellationToken))
        {
            db.VerifyNotNull(nameof(db));

            await InitializeAsync(db, nameof(db), progress, ct);
            return db;
        }

        protected void Mock<TModel>(DbTable<TModel> dbTable)
            where TModel : Model, new()
        {
            AddTable(dbTable, null);
        }

        protected void Mock<TModel>(DbTable<TModel> dbTable, Func<DataSet<TModel>> getDataSet)
            where TModel : Model, new()
        {
            getDataSet.VerifyNotNull(nameof(getDataSet));

            AddTable(dbTable, async (ct) => {
                var dataSet = getDataSet();
                if (dataSet != null)
                {
                    dbTable._.SuspendIdentity();
                    try
                    {
                        await dbTable.InsertAsync(dataSet, ct);
                    }
                    finally
                    {
                        dbTable._.ResumeIdentity();
                    }
                }
            });
        }

        private object _tag;
        private bool _isDbGeneration;

        internal sealed override async Task OnInitializingAsync(CancellationToken ct)
        {
            if (!_isDbGeneration)
                _tag = await Db.CreateMockDbAsync(ct);
        }

        internal sealed override string GetTableName(string name)
        {
            return _isDbGeneration ? name : Db.GetMockTableName(name, _tag);
        }
    }
}
