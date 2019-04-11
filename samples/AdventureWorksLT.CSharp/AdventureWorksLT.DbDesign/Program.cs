namespace DevZest.Samples.AdventureWorksLT
{
    class Program
    {
        static int Main(string[] args)
        {
#if DbDesign
            return Data.DbDesign.Program.Run(args);
#else
            return 0;
#endif
        }
    }
}
