using DevZest.Data.DbInit;

namespace Movies
{
    class Program
    {
        static int Main(string[] args)
        {
            return args.RunDbInit();
        }
    }
}
