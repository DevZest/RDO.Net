namespace DevZest.Data.MySql
{
    static class App
    {
        public static string GetConnectionString()
        {
            return "Server=127.0.0.1;Port=3306;Database=AdventureWorksLT;Uid=root;Allow User Variables=True";
        }
    }
}
