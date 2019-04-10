using DevZest.Data.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class MyMock : DbMock<DbSession>
    {
        protected override void Initialize()
        {
        }
    }

    public class MyMockWithoutWarning : DbMock<DbSession>
    {
        public static Task<DbSession> CreateAsync(DbSession db, IProgress<DbGenerationProgress> progress = null, CancellationToken ct = default(CancellationToken))
        {
            return new MyMockWithoutWarning().MockAsync(db, progress, ct);
        }

        protected override void Initialize()
        {
        }
    }
}
