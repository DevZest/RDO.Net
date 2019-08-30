#if DbInit
using DevZest.Data.DbInit;
#endif

namespace DevZest.Samples.AdventureWorksLT
{
    class Program
    {
        static int Main(string[] args)
        {
#if DbInit
            return args.RunDbInit();
#else
            return 0;
#endif
        }
    }
}
