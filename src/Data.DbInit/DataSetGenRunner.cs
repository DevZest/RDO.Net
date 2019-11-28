using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DevZest.Data.DbInit
{
    internal sealed class DataSetGenRunner : Runner
    {
        public static int Execute(DataSetGenOptions options)
        {
            return new DataSetGenRunner(options.CancellationPipeName).Run(options.DbSessionProviderType, options.ProjectPath, options.Verbose, options.Tables, options.Language, options.OutputDirectory);
        }

        private DataSetGenRunner(string pipeName)
            : base(pipeName)
        {
        }

        private int Run(string dbSessionProviderTypeFullName, string projectPath, bool showLog, IEnumerable<string> tableNames, string language, string outputDirectory)
        {
            var dbSessionProviderType = dbSessionProviderTypeFullName.ResoveType();
            using (var db = CreateDbSession(dbSessionProviderType, projectPath, showLog))
            {
                var task = RunAsync(db, tableNames.ToArray(), language, outputDirectory);
                return Run(task);
            }
        }

        private static IEnumerable<string> GetTableNames(string fileTables)
        {
            using (var file = new StreamReader(fileTables))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                    yield return line;
            }
        }

        private async Task RunAsync(DbSession db, string[] tableNames, string language, string directory)
        {
            for (int i = 0; i < tableNames.Length; i++)
            {
                var tableName = tableNames[i];
                Console.WriteLine(string.Format(Messages.FetchingTableData, tableName));
                CancellationToken.ThrowIfCancellationRequested();
                var dataSet = await GetDataSetAsync(db, tableName);
                using (var g = new DataSetGenerator(dataSet, language))
                {
                    Directory.CreateDirectory(directory);
                    File.WriteAllLines(Path.Combine(directory, tableName + ".types"), g.GetReferencedTypes().ToArray());
                    File.WriteAllLines(Path.Combine(directory, tableName + ".statements"), g.GetStatements().ToArray());
                }
            }
        }

        private IDbTable GetDbTable(DbSession db, string tableName)
        {
            var propertyInfo = db.GetType().GetProperty(tableName);
            return (IDbTable)propertyInfo.GetValue(db);
        }

        private Task<DataSet> GetDataSetAsync(DbSession db, string tableName)
        {
            var dbTable = GetDbTable(db, tableName);
            return dbTable.ToDataSetAsync(CancellationToken);
        }
    }
}
