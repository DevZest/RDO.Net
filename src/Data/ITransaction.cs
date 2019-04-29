using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public interface ITransaction : IDisposable
    {
        Task CommitAsync(CancellationToken ct = default(CancellationToken));
        Task RollbackAsync(CancellationToken ct = default(CancellationToken));
    }
}
