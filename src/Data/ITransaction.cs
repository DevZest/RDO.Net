using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public interface ITransaction : IDisposable
    {
        string Name { get; }
        int Level { get; }
        Task CommitAsync(CancellationToken ct = default(CancellationToken));
        Task RollbackAsync(CancellationToken ct = default(CancellationToken));
    }
}
