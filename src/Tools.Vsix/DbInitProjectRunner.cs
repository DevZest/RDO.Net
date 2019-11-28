using CommandLine;
using DevZest.Data.CodeAnalysis;
using DevZest.Data.DbInit;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Tools
{
    internal abstract class DbInitProjectRunner
    {
        private sealed class Runner<T> : DbInitProjectRunner
            where T : OptionsBase
        {
            public Runner(Project project, T options, DataSet<DbInitInput> input, EnvDTE.DTE dte)
                : base(project, dte, input)
            {
                Options = options;
            }

            T Options { get; }

            protected override string[] GetArgs(string pipeName)
            {
                Options.CancellationPipeName = pipeName;
                return Parser.Default.FormatCommandLine(Options).Split(' ');
            }
        }

        private abstract class Launcher
        {
            private enum OutputType
            {
                WinExe = 0,
                Exe = 1,
                Library = 2
            }

            private static OutputType GetOutputType(EnvDTE.Project vsProject)
            {
                return (OutputType)vsProject.GetOutputType();
            }

            private static bool IsExe(EnvDTE.Project vsProject)
            {
                return GetOutputType(vsProject) == OutputType.Exe;
            }

            private static Launcher[] s_launchers = new Launcher[] { NetCoreAppLauncher.Singleton, NetFramewokLauncher.Singleton };

            public static Launcher Get(EnvDTE.Project vsProject)
            {
                foreach (var launcher in s_launchers)
                {
                    if (launcher.IsSupported(vsProject))
                        return launcher;
                }

                return null;
            }

            public static void ReportProjectNotSupported(string projectName, IProgress<string> progress)
            {
                var supportedProjects = string.Join(", ", s_launchers.Select(x => x.Description));
                progress.ReportLine(string.Format(UserMessages.DbInitProjectRunner_ProjectNotSupported, projectName, supportedProjects));
            }

            private sealed class NetCoreAppLauncher : Launcher
            {
                public static readonly NetCoreAppLauncher Singleton = new NetCoreAppLauncher();

                private NetCoreAppLauncher()
                {
                }

                protected override string GetFileName(EnvDTE.Project vsProject)
                {
                    return "dotnet";
                }

                protected override string[] GetArguments(EnvDTE.Project vsProject, string[] args)
                {
                    return Concat(vsProject.GetAssemblyPath(), args);
                }

                protected override bool IsSupported(EnvDTE.Project vsProject)
                {
                    return IsExe(vsProject) && vsProject.GetTargetFrameworkMoniker().StartsWith(".NETCoreApp");
                }

                protected override string Description
                {
                    get { return UserMessages.DbInitProjectRunner_ConsoleAppNetCore; }
                }
            }

            private sealed class NetFramewokLauncher : Launcher
            {
                public static readonly NetFramewokLauncher Singleton = new NetFramewokLauncher();

                private NetFramewokLauncher()
                {
                }

                protected override string GetFileName(EnvDTE.Project vsProject)
                {
                    return vsProject.GetAssemblyPath();
                }

                protected override string[] GetArguments(EnvDTE.Project vsProject, string[] args)
                {
                    return args;
                }

                protected override bool IsSupported(EnvDTE.Project vsProject)
                {
                    var tfm = vsProject.GetTargetFrameworkMoniker();
                    return IsExe(vsProject) && tfm.StartsWith("NET") && char.IsDigit(tfm[3]);
                }

                protected override string Description
                {
                    get { return UserMessages.DbInitProjectRunner_ConsoleAppNetFramework; }
                }
            }

            protected abstract string GetFileName(EnvDTE.Project vsProject);

            protected abstract string[] GetArguments(EnvDTE.Project vsProject, string[] args);

            protected abstract bool IsSupported(EnvDTE.Project vsProject);

            protected abstract string Description { get; }

            public Task<int> LaunchAsync(EnvDTE.Project vsProject, string[] args, DataSet<DbInitInput> input, IProgress<string> progress)
            {
                var process = new Process();
                var startInfo = process.StartInfo;
                startInfo.FileName = GetFileName(vsProject).AddQuotesIfRequired();
                startInfo.Arguments = string.Join(" ", GetArguments(vsProject, args).AddQuotesIfRequired());

                startInfo.CreateNoWindow = true;
                startInfo.ErrorDialog = false;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;
                if (input != null && input.Count > 0)
                {
                    var _ = input._;
                    for (int i = 0; i < input.Count; i++)
                        startInfo.EnvironmentVariables[_.EnvironmentVariableName[i]] = _.Value[i];
                }

                process.ErrorDataReceived += OnDataReceived;
                process.OutputDataReceived += OnDataReceived;

                progress.ReportLine(string.Format(UserMessages.DbInitProjectRunner_StartingProcess, process.StartInfo.FileName, process.StartInfo.Arguments));
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                return System.Threading.Tasks.Task.Run(() =>
                {
                    process.WaitForExit();
                    return process.ExitCode;
                });

                void OnDataReceived(object sender, DataReceivedEventArgs e)
                {
                    if (e.Data != null)
                        progress.ReportLine(e.Data);
                }
            }
        }

        public static DbInitProjectRunner CreateDbInit(Project project, INamedTypeSymbol dbSessionProviderType, DataSet<DbInitInput> input, INamedTypeSymbol dbInitializerType, bool showLog, EnvDTE.DTE dte)
        {
            var options = new DbGenOptions()
            {
                DbSessionProviderType = dbSessionProviderType.GetFullyQualifiedMetadataName(),
                DbInitializerType = dbInitializerType?.GetFullyQualifiedMetadataName(),
                ProjectPath = Path.GetDirectoryName(project.FilePath),
                Verbose = showLog
            };
            return new Runner<DbGenOptions>(project, options, input, dte);
        }

        public static DbInitProjectRunner CreateDataSetGen(Project project, INamedTypeSymbol designTimeDbType, DataSet<DbInitInput> input, bool showLog, IEnumerable<string> tables, string outputDir, EnvDTE.DTE dte)
        {
            var options = new DataSetGenOptions()
            {
                DbSessionProviderType = designTimeDbType.GetFullyQualifiedMetadataName(),
                ProjectPath = Path.GetDirectoryName(project.FilePath),
                Language = project.Language,
                Tables = tables,
                OutputDirectory = outputDir,
                Verbose = showLog
            };
            return new Runner<DataSetGenOptions>(project, options, input, dte);
        }

        private DbInitProjectRunner(Project project, EnvDTE.DTE dte, DataSet<DbInitInput> input)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Project = project;
            DTE = dte;
            VsProject = GetVsProject(project);
            Input = input;
        }

        private EnvDTE.Project GetVsProject(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var vsProjects = DTE.Solution.Projects.OfType<EnvDTE.Project>();
            var projectFilePath = project.FilePath.ToLower();
            foreach (var vsProject in vsProjects)
            {
                var result = FindVsProject(vsProject, projectFilePath);
                if (result != null)
                    return result;
            }
            return null;
        }

        private static EnvDTE.Project FindVsProject(EnvDTE.Project vsProject, string projectFilePath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (vsProject == null)
                return null;

            if (vsProject.Kind == EnvDTE80.ProjectKinds.vsProjectKindSolutionFolder)
            {
                var projectItems = vsProject.ProjectItems;
                for (var i = 1; i <= projectItems.Count; i++)
                {
                    var subProject = projectItems.Item(i).SubProject;
                    var result = FindVsProject(subProject, projectFilePath);
                    if (result != null)
                        return result;
                }
                return null;
            }
            else if (vsProject.FullName.ToLower() == projectFilePath)
                return vsProject;
            else
                return null;
        }

        private Project Project { get; }

        private EnvDTE.DTE DTE { get; }

        private DataSet<DbInitInput> Input { get; }

        private EnvDTE.Project VsProject { get; }

        public async Task<int> RunAsync(IProgress<string> progress, CancellationToken ct)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(ct);

            progress.ReportLine(string.Format(UserMessages.DbInitProjectRunner_VerifyingProject, VsProject.Name));
            var launcher = Launcher.Get(VsProject);
            if (launcher == null)
            {
                Launcher.ReportProjectNotSupported(VsProject.Name, progress);
                return -1;
            }

            progress.ReportLine(string.Format(UserMessages.DbInitProjectRunner_BuildingProject, VsProject.Name));
            await BuildProjectAsync(ct);

            if (DTE.Solution.SolutionBuild.LastBuildInfo != 0)
            {
                progress.ReportLine(string.Format(UserMessages.DbInitProjectRunner_BuildingProjectFailed, VsProject.Name));
                return -1;
            }
            else
                progress.ReportLine(string.Format(UserMessages.DbInitProjectRunner_BuildingProjectSucceeded, VsProject.Name));

            if (ct.IsCancellationRequested)
            {
                progress.ReportLine(UserMessages.ConsoleWindow_OperationCancelled);
                return ExitCodes.Cancelled;
            }

            if (Input != null && Input.Count > 0)
            {
                if (DbInitInputWindow.Show(Input) != true)
                {
                    progress.ReportLine(UserMessages.ConsoleWindow_OperationCancelled);
                    return ExitCodes.Cancelled;
                }
            }

            var pipeName = Guid.NewGuid().ToString();
            var args = GetArgs(pipeName);
            var task = launcher.LaunchAsync(VsProject, args, Input, progress);
            Thread cancellationThread = null;
            var cancellationTask = System.Threading.Tasks.Task.Run(() =>
            {
                cancellationThread = Thread.CurrentThread;
                WaitHandle.WaitAny(new[] { ct.WaitHandle });
                RequestCancel(pipeName);
                cancellationThread = null;
            });

            await System.Threading.Tasks.Task.WhenAny(task, cancellationTask);
            if (cancellationThread != null)
                cancellationThread.Abort();
            var result = await task;

            if (result == ExitCodes.Succeeded)
                progress.ReportLine(UserMessages.DbInitProjectRunner_OperationSucceeded);
            else if (result != ExitCodes.Cancelled)
                progress.ReportLine(string.Format(UserMessages.DbInitProjectRunner_OperationFailed, result));
            return result;
        }

        protected abstract string[] GetArgs(string pipeName);

        private static string[] Concat(string arg, string[] args)
        {
            var result = new string[args.Length + 1];
            result[0] = arg;
            Array.Copy(args, 0, result, 1, args.Length);
            return result;
        }

        private void RequestCancel(string pipeName)
        {
            using (var stream = new NamedPipeClientStream(".", pipeName, PipeDirection.In))
            {
                stream.Connect();
            }
        }

        private TaskCompletionSource<object> _tcsBuildProject;
        private EnvDTE.BuildEvents _buildEvents;
        private async System.Threading.Tasks.Task BuildProjectAsync(CancellationToken ct)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(ct);

            _tcsBuildProject = new TaskCompletionSource<object>();

            _buildEvents = DTE.Events.BuildEvents;  // This is a COM wrapper and has to be declared at class level, or it will be garbage collected. 
            _buildEvents.OnBuildDone += OnBuildDone;
            var solutionBuild = DTE.Solution.SolutionBuild;
            solutionBuild.BuildProject(solutionBuild.ActiveConfiguration.Name, VsProject.UniqueName);
            await _tcsBuildProject.Task;
        }

        private void OnBuildDone(EnvDTE.vsBuildScope Scope, EnvDTE.vsBuildAction Action)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _buildEvents.OnBuildDone -= OnBuildDone;
            _tcsBuildProject.SetResult(null);
        }
    }
}
