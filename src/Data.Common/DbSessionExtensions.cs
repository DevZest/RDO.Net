using DevZest.Data.Primitives;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public static class DbSessionExtensions
    {
        public static T OpenConnection<T>(this T dbSession)
            where T : DbSession
        {
            dbSession.InternalOpenConnection();
            return dbSession;
        }

        public static Task<T> OpenConnectionAsync<T>(this T dbSession)
            where T : DbSession
        {
            return dbSession.OpenConnectionAsync(CancellationToken.None);
        }


        public static async Task<T> OpenConnectionAsync<T>(this T dbSession, CancellationToken ct)
            where T : DbSession
        {
            await dbSession.InternalOpenConnectionAsync(ct);
            return dbSession;
        }
    }
}
