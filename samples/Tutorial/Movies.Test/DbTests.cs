using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Movies
{
    [TestClass]
    public class DbTests
    {
        private static string GetConnectionString()
        {
            string mdfFilename = "EmptyDb.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string attachDbFilename = Path.Combine(outputFolder, mdfFilename);
            return string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
        }

        private static Db CreateDb()
        {
            return new Db(GetConnectionString());
        }

        [TestMethod]
        public async Task Db_GetMovies()
        {
            using (var db = await MockMovie.CreateAsync(CreateDb()))
            {
                var result = await db.GetMoviesAsync("comedy");
                Assert.AreEqual(3, result.Count);
            }

            using (var db = await MockMovie.CreateAsync(CreateDb()))
            {
                var result = await db.GetMoviesAsync("ghost");
                Assert.AreEqual(2, result.Count);
            }
        }

        [TestMethod]
        public async Task Db_GetMovie()
        {
            using (var db = await MockMovie.CreateAsync(CreateDb()))
            {
                var result = await db.GetMovieAsync(1);
                Assert.AreEqual(1, result.Count);
            }
        }
    }
}
