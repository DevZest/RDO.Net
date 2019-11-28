using DevZest.Data.CodeAnalysis;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;

namespace DevZest.Data.Tools
{
    internal sealed class DbInitContextMenuHandler
    {
        private static readonly Guid CommandSetGuid = new Guid("A7148E7D-4058-4B22-B4A9-F4A7598DEEAB");
        private const int CommandId_DbGen = 0x0010;
        private const int CommandId_DataSetGen = 0x0020;


        public static DbInitContextMenuHandler Initialize(IServiceProvider serviceProvider, OleMenuCommandService commandService)
        {
            return new DbInitContextMenuHandler(serviceProvider, commandService);
        }

        private DbInitContextMenuHandler(IServiceProvider serviceProvider, OleMenuCommandService commandService)
        {
            ServiceProvider = serviceProvider;
            InitializeCommands(commandService);
        }

        private IServiceProvider ServiceProvider { get; }

        private void InitializeCommands(OleMenuCommandService commandService)
        {
            InitDbGenCommand(commandService);
            InitDataSetGenCommand(commandService);
        }

        private void InitDbGenCommand(OleMenuCommandService commandService)
        {
            var command = new OleMenuCommand(DbGenCommand_Execute, new CommandID(CommandSetGuid, CommandId_DbGen));
            command.BeforeQueryStatus += DbGenCommand_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private void DbGenCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            ((OleMenuCommand)sender).Visible = IsDbInitializer || IsDbGenSessionProvider;
        }

        private bool IsDbInitializer
        {
            get { return GetCodeContext().GetDbInitializerType() != null; }
        }

        private bool IsDbGenSessionProvider
        {
            get { return GetCodeContext().GetDbGenSessionProviderType() != null; }
        }

        private void DbGenCommand_Execute(object sender, EventArgs e)
        {
            Debug.Assert(IsDbInitializer || IsDbGenSessionProvider);

            var codeContext = GetCodeContext();

            if (IsDbInitializer)
            {
                var dbInitializerType = codeContext.GetDbInitializerType();
                DbInitWindow.Show(codeContext.Project, dbInitializerType, GetDTE());
            }
            else
            {
                var dbSessionProviderType = codeContext.GetDbGenSessionProviderType();
                DbGenWindow.Show(codeContext.Project, dbSessionProviderType, dbSessionProviderType.GetDbInitInput(codeContext.Compilation), GetDTE());
            }
        }

        private void InitDataSetGenCommand(OleMenuCommandService commandService)
        {
            var command = new OleMenuCommand(DataSetGenCommand_Execute, new CommandID(CommandSetGuid, CommandId_DataSetGen));
            command.BeforeQueryStatus += DataSetGenCommand_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private void DataSetGenCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            ((OleMenuCommand)sender).Visible = IsDbInitializer && HasDataSetEntry;
        }

        private bool HasDataSetEntry
        {
            get { return GetCodeContext().HasDataSetEntry(); }
        }

        private void DataSetGenCommand_Execute(object sender, EventArgs e)
        {
            Debug.Assert(IsDbInitializer && HasDataSetEntry);
            var codeContext = GetCodeContext();
            DataSetGenWindow.Show(codeContext, GetDTE());
        }

        private CodeContext GetCodeContext()
        {
            var document = ServiceProvider.GetCurrentDocument();
            if (document == null || !document.Project.TryGetCompilation(out _) || !CodeContext.IsSupported(document))
                return default(CodeContext);
            return CodeContext.Create(document, ServiceProvider.GetCurrentSelectionSpan());
        }

        private EnvDTE.DTE GetDTE()
        {
            return ServiceProvider.GetDTE();
        }
    }
}
