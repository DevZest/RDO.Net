using DevZest.Data.Primitives;
using System;
using System.IO.Pipes;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.DbInit
{
    internal abstract class Runner
    {
        protected Runner(string pipeName)
        {
            if (!string.IsNullOrEmpty(pipeName))
            {
                CancellationTokenSource = new CancellationTokenSource();
                CancellationTask = ListenCancellationAsync(pipeName);
            }
        }

        private CancellationTokenSource CancellationTokenSource { get; }

        protected CancellationToken CancellationToken
        {
            get { return CancellationTokenSource == null ? default(CancellationToken) : CancellationTokenSource.Token; }
        }

        private void RequestCancel()
        {
            CancellationTokenSource.Cancel();
        }

        protected object Instantiate(Type type)
        {
            return Activator.CreateInstance(type);
        }

        private object Instantiate(string typeFullName)
        {
            return Instantiate(typeFullName.ResoveType());
        }

        protected DbSession CreateDbSession(Type dbSessionProviderType, string projectPath, bool showLog)
        {
            var genericDbSessionProviderType = dbSessionProviderType.SafeGetGenericDbSessionProviderType();
            var createMethodInfo = GetMethod(genericDbSessionProviderType, nameof(DbSessionProvider<DbSession>.Create), typeof(string));
            var dbSessionProvider = Instantiate(dbSessionProviderType);
            InitInputValues(dbSessionProvider);
            var result = (DbSession)Invoke(createMethodInfo, dbSessionProvider, projectPath);
            if (showLog)
            {
                var showLogMethodInfo = GetMethod(result.GetType(), nameof(DbSession.SetLogger), typeof(Action<string>));
                Invoke(showLogMethodInfo, result, LogAction);
            }

            return result;
        }

        private static void InitInputValues(object dbSessionProvider)
        {
            var inputAttributes = InputAttribute.Resolve(dbSessionProvider.GetType());
            if (inputAttributes == null || inputAttributes.Length == 0)
                return;

            for (int i = 0; i < inputAttributes.Length; i++)
                InitInputValue(dbSessionProvider, inputAttributes[i]);
        }

        private static void InitInputValue(object dbSessionProvider, InputAttribute inputAttribute)
        {
            var value = GetInputValue(inputAttribute);
            inputAttribute.SetValue(dbSessionProvider, value);
        }

        private static string GetInputValue(InputAttribute inputAttribute)
        {
            var result = Environment.GetEnvironmentVariable(inputAttribute.EnvironmentVariableName);
            return result ?? inputAttribute.Title.ReadFromConsole(inputAttribute.IsPassword);
        }

        protected static MethodInfo GetMethod(Type type, string name, params Type[] parameters)
        {
            return type.GetMethod(name, parameters);
        }

        protected static object Invoke(MethodInfo methodInfo, object obj, params object[] arguments)
        {
            return methodInfo.Invoke(obj, arguments);
        }

        private Action<string> LogAction
        {
            get { return WriteLog; }
        }

        private static void WriteLog(string value)
        {
            Console.Write(value);
        }

        private Task CancellationTask { get; }

        private NamedPipeServerStream _pipeStream;
        private async Task ListenCancellationAsync(string pipeName)
        {
            using (var stream = _pipeStream = new NamedPipeServerStream(pipeName, PipeDirection.Out))
            {
                try
                {
                    await stream.WaitForConnectionAsync(CancellationToken);
                    RequestCancel();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine(Messages.OperationCancelled);
                }
                finally
                {
                    _pipeStream = null;
                }
            }
        }

        protected int Run(Task task)
        {
            return RunAsync(task).Result;
        }

        private async Task<int> RunAsync(Task task)
        {
            try
            {
                await task;
                return ExitCodes.Succeeded;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine(Messages.OperationCancelled);
                return ExitCodes.Cancelled;
            }
            finally
            {
                if (_pipeStream != null)
                    _pipeStream.Close();
            }
        }
    }
}
