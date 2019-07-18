using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Movies.WPF
{
    public partial class App : Application
    {
        private static readonly string ConnectionString = GetConnectionString();

        private static string GetConnectionString()
        {
            string mdfFilename = "Movies.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string attachDbFilename = Path.Combine(outputFolder, mdfFilename);
            return string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
        }

        private static Db CreateDb()
        {
            return new Db(App.ConnectionString);
        }

        public static async Task ExecuteAsync(Func<Db, Task> func)
        {
            using (var db = CreateDb())
            {
                await func(db);
            }
        }

        public static async Task<T> ExecuteAsync<T>(Func<Db, Task<T>> func)
        {
            using (var db = CreateDb())
            {
                return await func(db);
            }
        }

    }
}
