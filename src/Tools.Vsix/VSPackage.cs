using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(VSPackage.PackageGuidString)]
    [ProvideAutoLoad(LoadContext, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideUIContextRule(LoadContext, "RightFileTypeOpen", "(CSharpFileOpen | VBFileOpen)", new[] { "CSharpFileOpen", "VBFileOpen" },
        new[] { "ActiveEditorContentType:CSharp", "ActiveEditorContentType:Basic" })]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(ModelVisualizer))]
    [ProvideToolWindow(typeof(DbVisualizer))]
    public sealed class VSPackage : AsyncPackage
    {
        /// <summary>
        /// VSPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "69fd4743-5823-4911-bc25-f05519fdc383";
        public const string LoadContext = "01783248-9BC2-4AD0-BA1D-F5E17EF7AB72";

        /// <summary>
        /// Initializes a new instance of the <see cref="VSPackage"/> class.
        /// </summary>
        public VSPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        private DbInitContextMenuHandler _dbInitContextMenuHandler;
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override async System.Threading.Tasks.Task InitializeAsync(System.Threading.CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            // When initialized asynchronously, we *may* be on a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            // Otherwise, remove the switch to the UI thread if you don't need it.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                commandService.AddCommand(new MenuCommand(ShowModelVisualizer, new CommandID(ModelVisualizer.CommandSetGuid, ModelVisualizer.CommandId_Show)));
                commandService.AddCommand(new MenuCommand(ShowDbVisualizer, new CommandID(DbVisualizer.CommandSetGuid, DbVisualizer.CommandId_Show)));
                Debug.Assert(_dbInitContextMenuHandler == null);
                _dbInitContextMenuHandler = DbInitContextMenuHandler.Initialize(this, commandService);
            }
        }

        private IServiceProvider ServiceProvider
        {
            get { return this; }
        }

        private void ShowModelVisualizer(object sender, EventArgs e)
        {
            ShowToolWindow<ModelVisualizer>();
        }

        private void ShowDbVisualizer(object sender, EventArgs e)
        {
            ShowToolWindow<DbVisualizer>();
        }

        private void ShowToolWindow<T>()
            where T : ToolWindowPane
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = FindToolWindow(typeof(T), 0, true);
            if ((null == window) || (null == window.Frame))
                throw new NotSupportedException("Cannot create tool window");

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        #endregion
    }
}
