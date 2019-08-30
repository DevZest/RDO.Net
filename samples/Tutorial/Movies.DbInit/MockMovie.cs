using DevZest.Data;
using DevZest.Data.DbInit;
using DevZest.Data.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Movies
{
    public sealed class MockMovie : DbMock<Db>
    {
        public static Task<Db> CreateAsync(Db db, IProgress<DbInitProgress> progress = null, CancellationToken ct = default(CancellationToken))
        {
            return new MockMovie().MockAsync(db, progress, ct);
        }

        private static DataSet<Movie> GetMovies()
        {
            DataSet<Movie> result = DataSet<Movie>.Create().AddRows(4);
            Movie _ = result._;
            _.SuspendIdentity();
            _.ID[0] = 1;
            _.ID[1] = 2;
            _.ID[2] = 3;
            _.ID[3] = 4;
            _.Title[0] = "When Harry Met Sally";
            _.Title[1] = "Ghostbusters";
            _.Title[2] = "Ghostbusters 2";
            _.Title[3] = "Rio Bravo";
            _.ReleaseDate[0] = Convert.ToDateTime("1989-02-12T00:00:00");
            _.ReleaseDate[1] = Convert.ToDateTime("1984-03-13T00:00:00");
            _.ReleaseDate[2] = Convert.ToDateTime("1986-02-23T00:00:00");
            _.ReleaseDate[3] = Convert.ToDateTime("1959-04-15T00:00:00");
            _.Genre[0] = "Romantic Comedy";
            _.Genre[1] = "Comedy";
            _.Genre[2] = "Comedy";
            _.Genre[3] = "Western";
            _.Price[0] = 7.9900M;
            _.Price[1] = 8.9900M;
            _.Price[2] = 9.9900M;
            _.Price[3] = 3.9900M;
            _.ResumeIdentity();
            return result;
        }

        protected override void Initialize()
        {
            Mock(Db.Movie, GetMovies);
        }
    }
}
