using CommandLine;
using System;
using System.Reflection;

namespace DevZest.Data.DbInit
{
    /// <summary>
    /// Provides extension methods for DbInit tasks.
    /// </summary>
    public static class DbInitExtensions
    {
        /// <summary>
        /// Adds specified number of <see cref="DataRow"/> into <see cref="DataSet"/>.
        /// </summary>
        /// <typeparam name="T">Type of the dataset model reference.</typeparam>
        /// <param name="dataSet">The dataset to add rows.</param>
        /// <param name="count">Number of rows to add.</param>
        /// <returns>The same dataset for fluent coding.</returns>
        public static DataSet<T> AddRows<T>(this DataSet<T> dataSet, int count)
            where T : Model, new()
        {
            for (int i = 0; i < count; i++)
                dataSet.AddRow();

            return dataSet;
        }

        /// <summary>
        /// Executes DbInit tasks.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Error code of the execution.</returns>
        public static int RunDbInit(this string[] args)
        {
            Console.WriteLine(Messages.ExecutingDbInitProgramRun);

            if (args == null || args.Length == 0)
                return RunDefault();

            return Parser.Default.ParseArguments(args, typeof(DbGenOptions), typeof(DataSetGenOptions))
                .MapResult(
                  (DbGenOptions opts) => DbGenRunner.Execute(opts),
                  (DataSetGenOptions opts) => DataSetGenRunner.Execute(opts),
                  errs => -1);
        }

        private static int RunDefault()
        {
            var defaultRunAttribute = GetDefaultRunAttribute();
            if (defaultRunAttribute == null)
            {
                Console.WriteLine(Messages.NoDefaultDbGenAttribute);
                return -1;
            }
            else
                return defaultRunAttribute.Run();
        }

        private static DefaultDbGenAttribute GetDefaultRunAttribute()
        {
#if DEPLOY
            return Assembly.GetEntryAssembly().GetCustomAttribute<DefaultDbGenAttribute>();
#else
            var result = Assembly.GetEntryAssembly().GetCustomAttribute<DefaultDbGenAttribute>();
            if (result != null)
                return result;

            // Resolve from assemblies, for unit testing purpose.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                result = assembly.GetCustomAttribute<DefaultDbGenAttribute>();
                if (result != null)
                    return result;
            }

            return null;
#endif
        }
    }
}
