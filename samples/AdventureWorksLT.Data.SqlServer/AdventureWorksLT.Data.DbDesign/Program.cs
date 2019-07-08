#if DbDesign
using DevZest.Data.DbDesign;
#endif

namespace DevZest.Samples.AdventureWorksLT
{
    class Program
    {
        static int Main(string[] args)
        {
#if DbDesign
            return args.RunDbDesign();
#else
            return 0;
#endif
        }
    }
}
