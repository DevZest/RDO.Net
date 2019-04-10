using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    public interface IDbGenerator<T>
        where T : DbSession
    {
        Task<T> GenerateAsync(T db, IProgress<DbGenerationProgress> progress = null, CancellationToken ct = default(CancellationToken));
    }
}
