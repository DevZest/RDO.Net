namespace DevZest.Samples.AdventureWorksLT
{
    class Program
    {
        static int Main(string[] args)
        {
#if DbDesign
            return DevZest.Data.DbDesign.Run(args);
#else
            return 0;
#endif
        }
    }
}
