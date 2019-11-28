using DevZest.Data.DbInit;
using System.IO;
using System.Reflection;

namespace DevZest.Samples.AdventureWorksLT
{
    [EmptyDb]
    public sealed class _DbProvider : DbSessionProvider<Db>
    {
        private static bool s_shouldStopSqlServerLocalDb = false;

        public override Db Create(string projectPath)
        {
            var dbFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string attachDbFilename = Path.Combine(dbFolder, "AdventureWorksLT.Design.mdf");
            if (s_shouldStopSqlServerLocalDb)
                StopSqlServerLocalDb();
            File.Copy(Path.Combine(dbFolder, "EmptyDb.mdf"), attachDbFilename, true);
            File.Copy(Path.Combine(dbFolder, "EmptyDb_log.ldf"), Path.Combine(dbFolder, "AdventureWorksLT.Design_log.ldf"), true);
            s_shouldStopSqlServerLocalDb = true;
            var connectionString = string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
            return new Db(connectionString);
        }

        private static void StopSqlServerLocalDb()
        {
            ExecuteCommand("sqllocaldb p MSSQLLocalDB");
        }

        private static void ExecuteCommand(string command)
        {
            // create the ProcessStartInfo using "cmd" as the program to be run,
            // and "/c " as the parameters.
            // Incidentally, /c tells cmd that we want it to execute the command that follows,
            // and then exit.
            System.Diagnostics.ProcessStartInfo procStartInfo =
                new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);

            // The following commands are needed to redirect the standard output.
            // This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            // Do not create the black window.
            procStartInfo.CreateNoWindow = true;
            // Now we create a process, assign its ProcessStartInfo and start it
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            // Get the output into a string
            string result = proc.StandardOutput.ReadToEnd();
            // Display the command output.
            // Console.WriteLine(result);
        }
    }
}
