using DevZest.Data;
using DevZest.Data.DbDesign;
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
            DataSet<Movie> result = DataSet<Movie>.Create().AddRows(3);
            Movie _ = result._;
            _.SuspendIdentity();
            _.ID[0] = 1;
            _.ID[1] = 2;
            _.ID[2] = 3;
            _.Title[0] = "When Harry Met Sally";
            _.Title[1] = "Ghostbusters 2";
            _.Title[2] = "Rio Bravo";
            _.ReleaseDate[0] = Convert.ToDateTime("1989-02-12T00:00:00");
            _.ReleaseDate[1] = Convert.ToDateTime("1986-02-23T00:00:00");
            _.ReleaseDate[2] = Convert.ToDateTime("1959-04-15T00:00:00");
            _.Genre[0] = "Romantic Comedy";
            _.Genre[1] = "Comedy";
            _.Genre[2] = "Western";
            _.Price[0] = 7.9900M;
            _.Price[1] = 9.9900M;
            _.Price[2] = 3.9900M;
            _.ResumeIdentity();
            return result;
        }

        protected override void Initialize()
        {
            Mock(Db.Movie, GetMovies);
        }
    }
}
