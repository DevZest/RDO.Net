using DevZest.Data.Primitives;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public static class DbSessionExtensions
    {
        public static async Task<T> OpenAsync<T>(this T dbSession, CancellationToken ct = default(CancellationToken))
            where T : DbSession
        {
            await dbSession.OpenConnectionAsync(ct);
            return dbSession;
        }
    }
}
