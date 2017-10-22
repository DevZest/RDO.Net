using DevZest.Data.Primitives;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public static class DbSessionExtensions
    {
        public static T Open<T>(this T dbSession)
            where T : DbSession
        {
            dbSession.OpenConnection();
            return dbSession;
        }

        public static Task<T> OpenAsync<T>(this T dbSession)
            where T : DbSession
        {
            return dbSession.OpenAsync(CancellationToken.None);
        }


        public static async Task<T> OpenAsync<T>(this T dbSession, CancellationToken ct)
            where T : DbSession
        {
            await dbSession.OpenConnectionAsync(ct);
            return dbSession;
        }
    }
}
