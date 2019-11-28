using DevZest.Data.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.DbInit
{
    internal class DbGenRunner : Runner
    {
        public static int Execute(DbGenOptions options)
        {
            return new DbGenRunner(options.CancellationPipeName).Run(options.DbSessionProviderType, options.DbInitializerType, options.ProjectPath, options.Verbose);
        }

        public static int Execute(Type dbSessionProviderType, Type dbSessionType, Type dbInitializerType, string projectPath, bool showLog)
        {
            return new DbGenRunner().Run(dbSessionProviderType, dbSessionType, dbInitializerType, projectPath, showLog);
        }

        private DbGenRunner(string pipeName = null)
            : base(pipeName)
        {
        }

        private int Run(string dbSessionProviderTypeFullName, string dbInitializerTypeFullName, string projectPath, bool showLog)
        {
            var dbSessionProviderType = dbSessionProviderTypeFullName.ResoveType();
            var dbSessionType = dbSessionProviderType.SafeGetDbSessionType();
            var dbInitializerType = GetDbInitializerType(dbInitializerTypeFullName);
            return Run(dbSessionProviderType, dbSessionType, dbInitializerType, projectPath, showLog);
        }

        private int Run(Type dbSessionProviderType, Type dbSessionType, Type dbInitializerType, string projectPath, bool showLog)
        {
            var task = RunAsync(dbSessionProviderType, dbSessionType, dbInitializerType, projectPath, showLog);
            return Run(task);
        }

        private async Task RunAsync(Type dbSessionProviderType, Type dbSessionType, Type dbInitializerType, string projectPath, bool showLog)
        {
            dbInitializerType = EnsureDbInitializerType(dbInitializerType, dbSessionType);
            var genericDbInitializerType = dbInitializerType.SafeGetGenericDbInitializerType();

            var methodInfo = GetMethod(genericDbInitializerType, nameof(DbInitializer<DbSession>.GenerateAsync), dbSessionType, typeof(IProgress<DbInitProgress>), typeof(CancellationToken));
            var dbInitializer = Instantiate(dbInitializerType);
            using (var dbSession = CreateDbSession(dbSessionProviderType, projectPath, showLog))
            {
                await (Task)Invoke(methodInfo, dbInitializer, dbSession, dbInitializer, CancellationToken);
            }
        }

        private Type GetDbInitializerType(string dbInitializerTypeFullName)
        {
            return string.IsNullOrEmpty(dbInitializerTypeFullName) || dbInitializerTypeFullName == "-" ? null : dbInitializerTypeFullName.ResoveType();
        }

        private Type EnsureDbInitializerType(Type dbInitializerType, Type dbSessionType)
        {
            return dbInitializerType ?? typeof(DbGenerator<>).MakeGenericType(dbSessionType);
        }
    }
}
