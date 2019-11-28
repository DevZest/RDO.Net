using DevZest.Data.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;

namespace DevZest.Data.Tools
{
    public abstract class CodeVisualizer : ToolWindowPane, IVsRunningDocTableEvents
    {
        private const string CSharpContentType = "CSharp";
        private const string VisualBasicContentType = "Basic";
        private readonly TimeSpan RefreshTimerTimeout = TimeSpan.FromMilliseconds(300);

        private DispatcherTimer _refreshTimer;
        private IWpfTextView _wpfTextView;

        protected override void Initialize()
        {
            base.Initialize();
            Content = new CodeVisualizerRoot();
            RootElement.Initialize(OnGotFocus, OnLostFocus, OnPropertyChanged);
            _refreshTimer = new DispatcherTimer
            {
                Interval = RefreshTimerTimeout
            };
            _refreshTimer.Tick += HandleRefreshTimerTimeout;
        }

        private CodeVisualizerRoot RootElement
        {
            get { return (CodeVisualizerRoot)Content; }
        }

        private UIElement View
        {
            get { return RootElement.Content; }
            set { RootElement.Content = value; }
        }

        protected override void Dispose(bool disposing)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            UnadviseRunningDocTableEvents();
            base.Dispose(disposing);
        }

        protected TServiceInterface GetService<TServiceInterface, TService>()
            where TServiceInterface : class
            where TService : class
        {
            return (TServiceInterface)(GetService(typeof(TService)));
        }

        int IVsRunningDocTableEvents.OnBeforeDocumentWindowShow(uint docCookie, int isFirstShow, IVsWindowFrame vsWindowFrame)
        {
            if (isFirstShow == 0)
                WpfTextView = vsWindowFrame.ToWpfTextView();
            return 0;
        }

        int IVsRunningDocTableEvents.OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame vsWindowFrame)
        {
            WpfTextView = null;
            return VSConstants.S_OK;
        }

        private IWpfTextView WpfTextView
        {
            get { return _wpfTextView; }
            set
            {
                if (value != null)
                {
                    IContentType contentType = value.TextBuffer.ContentType;
                    if (!contentType.IsOfType(CSharpContentType) && !contentType.IsOfType(VisualBasicContentType))
                        value = null;
                }

                if (_wpfTextView == value)
                    return;

                if (_wpfTextView != null)
                {
                    TextSelection.SelectionChanged -= OnSelectionChanged;
                    _wpfTextView.TextBuffer.Changed -= OnTextBufferChanged;
                }

                _wpfTextView = value;

                if (_wpfTextView != null)
                {
                    TextSelection.SelectionChanged += OnSelectionChanged;
                    _wpfTextView.TextBuffer.Changed += OnTextBufferChanged;
                }

                StartRefresh();
            }
        }

        protected bool CanNavigateToSource(INavigatable navigatable)
        {
            if (navigatable == null)
                return false;

            var location = navigatable.Location;
            if (location == null || location.SourceTree == null)
                return false;

            return true;
        }

        public INavigatableMarker NavigatableMarker { get; protected set; }

        protected void NavigateToSource(INavigatable navigatable, bool activate)
        {
            Debug.Assert(CanNavigateToSource(navigatable));

            ThreadHelper.ThrowIfNotOnUIThread();

            if (NavigateToLocalSource(navigatable))
            {
                if (activate)
                    EnsureDocumentActivated();
                return;
            }

            var location = navigatable.Location;
            if (location == null || location.SourceTree == null)
                return;

            NavigatableMarker = navigatable.CreateMarker();
            var documentId = Document.Project.GetDocumentId(location.SourceTree);
            var workspace = Document.Project.Solution.Workspace;
            workspace.OpenDocument(documentId, true);
        }

        private bool NavigateToLocalSource(INavigatable navigatable)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var localLocation = navigatable.LocalLocation;
            if (localLocation != null && IsLocal(localLocation))
            {
                NavigateToSource(localLocation.SourceSpan);
                return true;
            }

            return false;
        }

        private bool IsLocal(Location location)
        {
            var document = Document;
            if (document == null || !document.TryGetSyntaxTree(out var syntaxTree))
                return false;
            return location.SourceTree == syntaxTree;
        }

        protected SnapshotSpan NavigateToSource(TextSpan span)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var textView = WpfTextView;
            SnapshotSpan result = span.ToSnapshotSpan(textView.TextBuffer.CurrentSnapshot);
            NavigateToSource(textView, result);
            EnsureExpanded(textView, result);
            return result;
        }

        private void NavigateToSource(IWpfTextView textView, SnapshotSpan span)
        {
            textView.Selection.Select(span, false);
            textView.Caret.MoveTo(span.Start);
            textView.ViewScroller.EnsureSpanVisible(span);
        }

        private void EnsureExpanded(IWpfTextView textView, SnapshotSpan span)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            while (textView.Caret.Position.BufferPosition != span.Start || textView.Selection.StreamSelectionSpan.SnapshotSpan != span)
            {
                EnsureDocumentActivated();
                DTE.ExecuteCommand("Edit.ExpandCurrentRegion", string.Empty);
                NavigateToSource(textView, span);
            }
        }

        private ITextSelection TextSelection
        {
            get { return WpfTextView.Selection; }
        }

        public TextSpan SelectionSpan
        {
            get { return TextSelection.GetSelectionSpan(); }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            if (!RefreshSelectionChanged(SelectionSpan))
                StartRefresh();
        }

        private void OnTextBufferChanged(object sender, EventArgs e)
        {
            StartRefresh();
        }

        protected bool CanRefresh
        {
            get { return IsVisible && IsDocumentSupported; }
        }

        protected void StartRefresh()
        {
            IsBusy = false;

            if (!CanRefresh)
            {
                UpdateView();
                Workspace = null;
                return;
            }

            IsBusy = true;
        }

        private bool IsDocumentSupported
        {
            get
            {
                var document = Document;
                return document != null && CodeContext.IsSupported(document);
            }
        }

        public Document Document
        {
            get { return WpfTextView.GetDocument(); }
        }

        private bool IsBusy
        {
            get { return RootElement.IsBusy; }
            set
            {
                if (IsBusy == value)
                    return;

                RootElement.IsBusy = value;
                if (value)
                    _refreshTimer.Start();
                else
                    _refreshTimer.Stop();
            }
        }

        private void HandleRefreshTimerTimeout(object sender, EventArgs e)
        {
            Debug.Assert(IsBusy);
            var refreshed = PerformRefresh();
            if (refreshed)
                IsBusy = false;
        }

        private Workspace _workspace;
        private Workspace Workspace
        {
            get { return _workspace; }
            set
            {
                if (_workspace == value)
                    return;

                if (_workspace != null)
                    _workspace.WorkspaceChanged -= OnWorkspaceChanged;
                _workspace = value;
                if (_workspace != null) // Refresh may fail due to project is not built, listen for workspace change to refresh.
                    _workspace.WorkspaceChanged += OnWorkspaceChanged;
            }
        }

        private void OnWorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            Debug.Assert(View == null);
            StartRefresh();
        }

        private bool PerformRefresh()
        {
            Debug.Assert(IsDocumentSupported);

            var document = Document;
            var project = document.Project;
            if (!project.TryGetCompilation(out _))
                return false;

            UpdateView();
            Workspace = View == null ? project.Solution.Workspace : null;
            return true;
        }

        private void UpdateView()
        {
            View = Refresh();
            NavigatableMarker = null;
        }

        private EnvDTE.WindowEvents WindowEvents { get; set; }

        protected EnvDTE.DTE DTE
        {
            get { return this.GetDTE(); }
        }

        public override void OnToolWindowCreated()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var dte = DTE;
            EnvDTE80.Events2 events = (EnvDTE80.Events2)dte.Events;
            WindowEvents = events.get_WindowEvents(null);
            WindowEvents.WindowActivated += new EnvDTE._dispWindowEvents_WindowActivatedEventHandler(WindowEvents_WindowActivated);
        }

        public bool IsActive { get; private set; }

        void WindowEvents_WindowActivated(EnvDTE.Window gotFocus, EnvDTE.Window lostFocus)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IsActive = gotFocus.Kind == "Tool" // gotFocus.ObjectKind may throw an InvalidCastException when gotFocus is not a tool window.
                &&  new Guid(gotFocus.ObjectKind) == this.GetType().GUID;
        }

        protected abstract UIElement Refresh();

        protected abstract CodeMapper CodeMapper { get; }

        private bool RefreshSelectionChanged(TextSpan selectionSpan)
        {
            if (IsBusy)
                return false;

            var codeMapper = CodeMapper;
            return codeMapper != null && codeMapper.Document == Document && codeMapper.RefreshSelectionChanged(selectionSpan);
        }

        int IVsRunningDocTableEvents.OnBeforeLastDocumentUnlock(uint docCookie, uint lockType, uint readLocksRemaining, uint editLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterFirstDocumentLock(uint docCookie, uint lockType, uint readLocksRemaining, uint editLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterSave(uint docCookie)
        {
            return VSConstants.S_OK;
        }

        private void OnGotFocus()
        {
            if (_wpfTextView != null && !_wpfTextView.Properties.ContainsProperty("BackupOpacity"))
            {
                IAdornmentLayer adornmentLayer = _wpfTextView.GetAdornmentLayer("SelectionAndProvisionHighlight");
                _wpfTextView.Properties.AddProperty("BackupOpacity", adornmentLayer.Opacity);
                adornmentLayer.Opacity = 1.0;
            }
        }

        private void OnLostFocus()
        {
            if (_wpfTextView != null && _wpfTextView.Properties.ContainsProperty("BackupOpacity"))
            {
                _wpfTextView.GetAdornmentLayer("SelectionAndProvisionHighlight").Opacity = (double)this._wpfTextView.Properties.GetProperty("BackupOpacity");
                _wpfTextView.Properties.RemoveProperty("BackupOpacity");
            }
        }

        private IVsWindowFrame VsWindowFrame
        {
            get { ThreadHelper.ThrowIfNotOnUIThread(); return (IVsWindowFrame)Frame; }
        }

        private bool IsVsWindowFrameVisible
        {
            get { ThreadHelper.ThrowIfNotOnUIThread(); return VsWindowFrame.IsVisible() == VSConstants.S_OK; }
        }

        private uint _runningDocumentTableCookie;

        private void AdviseRunningDocTableEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_runningDocumentTableCookie == 0u)
            {
                var runningDocumentTable = GetService<IVsRunningDocumentTable, SVsRunningDocumentTable>();
                runningDocumentTable.AdviseRunningDocTableEvents(this, out _runningDocumentTableCookie);
            }
        }

        private void UnadviseRunningDocTableEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_runningDocumentTableCookie != 0u)
            {
                var runningDocumentTable = GetService<IVsRunningDocumentTable, SVsRunningDocumentTable>();
                runningDocumentTable.UnadviseRunningDocTableEvents(_runningDocumentTableCookie);
                _runningDocumentTableCookie = 0u;
            }
        }

        private bool _isVisible;
        private bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (_isVisible == value)
                    return;

                _isVisible = value;
                if (_isVisible)
                {
                    AdviseRunningDocTableEvents();
                    WpfTextView = this.GetCurrentWpfTextView();
                }
                else
                {
                    UnadviseRunningDocTableEvents();
                    WpfTextView = null;
                }
            }
        }

        private void RefreshIsVisible()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IsVisible = IsVsWindowFrameVisible;
        }

        private void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (e.Property != UIElement.IsVisibleProperty)
                return;

            RefreshIsVisible();

            if (IsVisible == (View == null))
                StartRefresh();
        }

        public void EnsureDocumentActivated()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE.ActiveDocument.Activate();
        }

        protected void InsertCodeSnippet(string shortcut, TextSpan span)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            EnsureDocumentActivated();
            var vsSpan = NavigateToSource(span);
            WpfTextView.TextBuffer.Replace(vsSpan, shortcut);
            DTE.ExecuteCommand(VsCommands.Edit.InsertTab, string.Empty);
        }

        protected void ShowMessageBox(string message, string title)
        {
            VsShellUtilities.ShowMessageBox(
                this,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        protected static void TryApplyChanges(Document changedDocument)
        {
            var solution = changedDocument.Project.Solution;
            var workspace = solution.Workspace;
            workspace.TryApplyChanges(solution);
        }
    }
}
