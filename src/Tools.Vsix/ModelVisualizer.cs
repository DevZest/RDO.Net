namespace DevZest.Data.Tools
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using DevZest.Data.Annotations;
    using DevZest.Data.CodeAnalysis;
    using DevZest.Data.Presenters;
    using Microsoft.CodeAnalysis;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("9b2fac6a-ccc5-46ef-aaec-79df7374f237")]
    public partial class ModelVisualizer : CodeVisualizer
    {
        public static readonly Guid CommandSetGuid = new Guid("60d853cd-eccf-48c0-aa8a-ec7f7c5e030c");
        private const int CommandId_Toolbar = 0x0100;
        private const int CommandId_ContextMenu = 0x0105;
        public const int CommandId_Show = 0x110;
        private const int CommandId_AddRegistableProperty = 0x0210;
        private const int CommandId_AddPrimaryKey = 0x0220;
        private const int CommandId_AddKey = 0x230;
        private const int CommandId_AddRef = 0x240;
        private const int CommandId_AddComputation = 0x245;
        private const int CommandId_AddForeignKey = 0x0250;
        private const int CommandId_AddCheckConstraint = 0x0260;
        private const int CommandId_AddUniqueConstraint = 0x265;
        private const int CommandId_AddCustomValidator = 0x268;
        private const int CommandId_AddIndex = 0x0270;
        private const int CommandId_AddProjection = 0x0280;
        private const int CommandId_GotoSource = 0x310;
        private const int CommandId_Refresh = 0x0320;
        private const int CommandId_ContextMenuStart = 0x9000;

        public ModelVisualizer()
        {
            Caption = UserMessages.ModelVisualizer_Caption;
            ToolBar = new CommandID(CommandSetGuid, CommandId_Toolbar);
        }

        protected override void Initialize()
        {
            base.Initialize();
            if (GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
                InitializeCommands(commandService);
        }

        private void InitializeCommands(OleMenuCommandService commandService)
        {
            InitContextMenuCommand(commandService);
            InitAddRegistablePropertyCommand(commandService);
            InitGotoSourceCommand(commandService);
            InitRefreshCommand(commandService);
            InitAddPrimaryKeyCommand(commandService);
            InitAddKeyCommand(commandService);
            InitAddRefCommand(commandService);
            InitAddForeignKeyCommand(commandService);
            InitAddComputationCommand(commandService);
            InitAddCheckConstraintCommand(commandService);
            InitAddUniqueConstraintCommand(commandService);
            InitAddCustomValidatorCommand(commandService);
            InitAddIndexCommand(commandService);
            InitAddProjectionCommand(commandService);
        }

        private void InitAddRegistablePropertyCommand(OleMenuCommandService commandService)
        {
            var command = new OleMenuCommand(AddRegistablePropertyCommand_Execute, new CommandID(CommandSetGuid, CommandId_AddRegistableProperty));
            command.BeforeQueryStatus += AddCodeSnippetCommand_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private void InitAddForeignKeyCommand(OleMenuCommandService commandService)
        {
            var command = new OleMenuCommand(AddForeignKeyCommand_Execute, new CommandID(CommandSetGuid, CommandId_AddForeignKey));
            command.BeforeQueryStatus += AddCodeSnippetCommand_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private void InitAddUniqueConstraintCommand(OleMenuCommandService commandService)
        {
            var command = new OleMenuCommand(AddUniqueConstraintCommand_Execute, new CommandID(CommandSetGuid, CommandId_AddUniqueConstraint));
            command.BeforeQueryStatus += AddCodeSnippetCommand_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private void InitAddComputationCommand(OleMenuCommandService commandService)
        {
            var command = new OleMenuCommand(AddComputationCommand_Execute, new CommandID(CommandSetGuid, CommandId_AddComputation));
            command.BeforeQueryStatus += AddCodeSnippetCommand_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private void InitAddCustomValidatorCommand(OleMenuCommandService commandService)
        {
            var command = new OleMenuCommand(AddCustomValidatorCommand_Execute, new CommandID(CommandSetGuid, CommandId_AddCustomValidator));
            command.BeforeQueryStatus += AddCodeSnippetCommand_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private void InitAddIndexCommand(OleMenuCommandService commandService)
        {
            var command = new OleMenuCommand(AddIndexCommand_Execute, new CommandID(CommandSetGuid, CommandId_AddIndex));
            command.BeforeQueryStatus += AddCodeSnippetCommand_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private void InitAddCheckConstraintCommand(OleMenuCommandService commandService)
        {
            var command = new OleMenuCommand(AddCheckConstraintCommand_Execute, new CommandID(CommandSetGuid, CommandId_AddCheckConstraint));
            command.BeforeQueryStatus += AddCodeSnippetCommand_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private void InitAddPrimaryKeyCommand(OleMenuCommandService commandService)
        {
            var command = new OleMenuCommand(AddPrimaryKeyCommand_Execute, new CommandID(CommandSetGuid, CommandId_AddPrimaryKey));
            command.BeforeQueryStatus += AddPrimaryKeyCommand_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private void InitAddKeyCommand(OleMenuCommandService commandService)
        {
            var command = new OleMenuCommand(AddKeyCommand_Execute, new CommandID(CommandSetGuid, CommandId_AddKey));
            command.BeforeQueryStatus += AddKeyCommand_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private void InitAddRefCommand(OleMenuCommandService commandService)
        {
            var command = new OleMenuCommand(AddRefCommand_Execute, new CommandID(CommandSetGuid, CommandId_AddRef));
            command.BeforeQueryStatus += AddRefCommand_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private void InitAddProjectionCommand(OleMenuCommandService commandService)
        {
            var command = new OleMenuCommand(AddProjectionCommand_Execute, new CommandID(CommandSetGuid, CommandId_AddProjection));
            command.BeforeQueryStatus += AddProjectionCommand_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private OleMenuCommand _gotoSourceCommand;
        private void InitGotoSourceCommand(OleMenuCommandService commandService)
        {
            _gotoSourceCommand = new OleMenuCommand(GotoSourceCommand_Execute, new CommandID(CommandSetGuid, CommandId_GotoSource));
            _gotoSourceCommand.BeforeQueryStatus += GotoSourceCommand_BeforeQueryStatus;
            commandService.AddCommand(_gotoSourceCommand);
        }

        private void InitContextMenuCommand(OleMenuCommandService commandService)
        {
            var command = new DynamicItemMenuCommand(new CommandID(CommandSetGuid, CommandId_ContextMenuStart), IsValidContextMenuCommand,
                ContextMenuCommand_Execute, ContextMenuCommand_BeforeQueryStatus);
            commandService.AddCommand(command);
        }

        private int ContextMenuItemsCount
        {
            get
            {
                var attributes = CurrentNode.Attributes;
                return attributes == null ? 0 : attributes.Count;
            }
        }

        private bool IsValidContextMenuCommand(int commandId)
        {
            return commandId >= CommandId_ContextMenuStart && commandId < CommandId_ContextMenuStart + ContextMenuItemsCount;
        }

        private void ContextMenuCommand_Execute(object sender, EventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var command = (OleMenuCommand)sender;
            var isRoot = command.MatchedCommandId == 0;
            var index = isRoot ? 0 : command.MatchedCommandId - CommandId_ContextMenuStart;

            var attribute = CurrentNode.Attributes[index];
            if (attribute.IsChecked)
                TryApplyChanges(CurrentNode.Remove(attribute));
            else
            {
                TryApplyChanges(CurrentNode.Add(attribute, out var textSpan));
                if (textSpan.HasValue)
#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
                    Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                    {
                        NavigateToSource(textSpan.Value);
                        EnsureDocumentActivated();
                    }));
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs
            }

            NavigatableMarker = CurrentNode.CreateMarker(navigationSuggested: false);
        }

        private void ContextMenuCommand_BeforeQueryStatus(object sender, EventArgs args)
        {
            var command = (OleMenuCommand)sender;

            command.Visible = true;
            var isRoot = command.MatchedCommandId == 0;
            int index = isRoot ? 0 : command.MatchedCommandId - CommandId_ContextMenuStart;

            var memberAttribute = CurrentNode.Attributes[index];
            command.Text = memberAttribute.DisplayName;
            command.Checked = memberAttribute.IsChecked;
            command.Enabled = memberAttribute.IsEnabled;

            // Must clear MatchedCommandId otherwise clicking first command will get MatchedCommandId of the last menu item.
            // what a fxxking ridiculous framework design!!!
            command.MatchedCommandId = 0;
        }

        private void RefreshGotoSourceCommand()
        {
            _gotoSourceCommand.Enabled = CanGotoSource;
        }

        private void GotoSourceCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            RefreshGotoSourceCommand();
        }

        private void GotoSourceCommand_Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            GotoSource();
        }

        private void InitRefreshCommand(OleMenuCommandService commandService)
        {
            var command = new OleMenuCommand(RefreshCommand_Execute, new CommandID(CommandSetGuid, CommandId_Refresh));
            command.BeforeQueryStatus += RefreshCommand_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private void RefreshCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            ((OleMenuCommand)sender).Enabled = CanRefresh;
        }

        private void RefreshCommand_Execute(object sender, EventArgs e)
        {
            Debug.Assert(CanRefresh);
            StartRefresh();
        }

        private void AddCodeSnippetCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            ((OleMenuCommand)sender).Enabled = CanAddCodeSnippet;
        }

        private bool CanAddCodeSnippet
        {
            get { return ModelMapper != null && ModelMapper.CodeSnippetInsertionSpan.HasValue; }
        }

        private void AddRegistablePropertyCommand_Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            InsertCodeSnippet("rdopropr", ModelMapper.CodeSnippetInsertionSpan.Value);
        }

        private void AddPrimaryKeyCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            ((OleMenuCommand)sender).Enabled = CanAddPrimaryKey;
        }

        private bool CanAddPrimaryKey
        {
            get { return ModelMapper != null && ModelMapper.CanAddPrimaryKey; }
        }

        private void AddPrimaryKeyCommand_Execute(object sender, EventArgs e)
        {
            Debug.Assert(CanAddPrimaryKey);
            PrimaryKeyWindow.Show(ModelMapper, AddPrimaryKey);
        }

        private void AddPrimaryKey(string pkTypeName, DataSet<ModelMapper.PrimaryKeyEntry> entries, string keyTypeName, string refTypeName)
        {
            var document = ModelMapper.AddPrimaryKey(pkTypeName, entries, keyTypeName, refTypeName);
            TryApplyChanges(document);
            NavigatableMarker = ModelMapper.CreateNavigatableMarker(ModelMapper.NodeKind.PrimaryKey, ModelMapper.ModelType, pkTypeName);
        }

        private void AddForeignKeyCommand_Execute(object sender, EventArgs e)
        {
            Debug.Assert(CanAddCodeSnippet);
            ForeignKeyWindow.Show(ModelMapper, AddForeignKey);
        }

        private void AddForeignKey(INamedTypeSymbol fkType, string fkName, DataSet<ModelMapper.ForeignKeyEntry> entries)
        {
            var document = ModelMapper.AddForeignKey(fkType, fkName, entries);
            TryApplyChanges(document);
            NavigatableMarker = ModelMapper.CreateNavigatableMarker(ModelMapper.NodeKind.ForeignKey, ModelMapper.ModelType, fkName);
        }

        private void AddUniqueConstraintCommand_Execute(object sender, EventArgs e)
        {
            Debug.Assert(CanAddCodeSnippet);
            UniqueConstraintWindow.Show(ModelMapper, AddUniqueConstraint);
        }

        private void AddUniqueConstraint(string name, string description, string dbName, INamedTypeSymbol resourceType, IPropertySymbol resourceProperty, string message, DataSet<ModelMapper.IndexEntry> entries)
        {
            var document = ModelMapper.AddUniqueConstraint(name, description, dbName, resourceType, resourceProperty, message, entries);
            TryApplyChanges(document);
            NavigatableMarker = ModelMapper.CreateNavigatableMarker(ModelMapper.NodeKind.UniqueConstraint, ModelMapper.ModelType, name);
        }

        private void AddComputationCommand_Execute(object sender, EventArgs e)
        {
            Debug.Assert(CanAddCodeSnippet);
            ComputationWindow.Show(ModelMapper, AddComputation);
        }

        private void AddComputation(string name, string description, ComputationMode? mode)
        {
            var document = ModelMapper.AddComputation(name, description, mode);
            TryApplyChanges(document);
            NavigatableMarker = ModelMapper.CreateNavigatableMarker(ModelMapper.NodeKind.Computation, ModelMapper.ModelType, name);
        }

        private void AddCustomValidatorCommand_Execute(object sender, EventArgs e)
        {
            Debug.Assert(CanAddCodeSnippet);
            CustomValidatorWindow.Show(ModelMapper, AddCustomValidator);
        }

        private void AddCustomValidator(string name, string description)
        {
            var document = ModelMapper.AddCustomValidator(name, description);
            TryApplyChanges(document);
            NavigatableMarker = ModelMapper.CreateNavigatableMarker(ModelMapper.NodeKind.CustomValidator, ModelMapper.ModelType, name);
        }

        private void AddIndexCommand_Execute(object sender, EventArgs e)
        {
            Debug.Assert(CanAddCodeSnippet);
            IndexWindow.Show(ModelMapper, AddIndex);
        }

        private void AddIndex(string name, string description, string dbName, bool isUnique, bool isValidOnTable, bool isValidOnTempTable, DataSet<ModelMapper.IndexEntry> entries)
        {
            var document = ModelMapper.AddIndex(name, description, dbName, isUnique, isValidOnTable, isValidOnTempTable, entries);
            TryApplyChanges(document);
            NavigatableMarker = ModelMapper.CreateNavigatableMarker(ModelMapper.NodeKind.Index, ModelMapper.ModelType, name);
        }

        private void AddCheckConstraintCommand_Execute(object sender, EventArgs e)
        {
            Debug.Assert(CanAddCodeSnippet);
            CheckConstraintWindow.Show(ModelMapper, AddCheckConstraint);
        }

        private void AddCheckConstraint(string name, string description, INamedTypeSymbol resourceType, IPropertySymbol resourceProperty, string message)
        {
            var document = ModelMapper.AddCheckConstraint(name, description, resourceType, resourceProperty, message);
            TryApplyChanges(document);
            NavigatableMarker = ModelMapper.CreateNavigatableMarker(ModelMapper.NodeKind.CheckConstraint, ModelMapper.ModelType, name);
        }

        private ModelMapper.TreeItem _
        {
            get { return _presenter?._; }
        }

        private RowPresenter PkRow
        {
            get { return _presenter == null ? null : _presenter.Rows.Where(x => x.GetValue(_.Node).Kind == ModelMapper.NodeKind.PrimaryKey).SingleOrDefault(); }
        }

        private bool CanAddKeyOrRef(ModelMapper.NodeKind kind)
        {
            var pkRow = PkRow;
            if (pkRow == null || !pkRow.GetValue(_.Node).HasLocalLocation)
                return false;
            return !pkRow.Children.Any(x => x.GetValue(_.Node).Kind == kind);
        }

        private void AddKeyCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            ((OleMenuCommand)sender).Enabled = CanAddKeyOrRef(ModelMapper.NodeKind.Key);
        }

        private void AddKeyCommand_Execute(object sender, EventArgs e)
        {
            Debug.Assert(CanAddKeyOrRef(ModelMapper.NodeKind.Key));
            KeyOrRefWindow.Show(UserMessages.ModelVisualizer_AddKey, nameof(ModelMapper.NodeKind.Key), ModelMapper, AddKey);
        }

        private void AddKey(string typeName, DataSet<ModelMapper.PrimaryKeyEntry> entries)
        {
            var document = ModelMapper.AddKey(typeName, entries);
            TryApplyChanges(document);
            NavigatableMarker = ModelMapper.CreateNavigatableMarker(ModelMapper.NodeKind.Key, ModelMapper.ModelType, typeName);
        }

        private void AddRefCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            ((OleMenuCommand)sender).Enabled = CanAddKeyOrRef(ModelMapper.NodeKind.Ref);
        }

        private void AddRefCommand_Execute(object sender, EventArgs e)
        {
            Debug.Assert(CanAddKeyOrRef(ModelMapper.NodeKind.Ref));
            KeyOrRefWindow.Show(UserMessages.ModelVisualizer_AddRef, nameof(ModelMapper.NodeKind.Ref), ModelMapper, AddRef);
        }

        private void AddRef(string typeName, DataSet<ModelMapper.PrimaryKeyEntry> entries)
        {
            var document = ModelMapper.AddRef(typeName, entries);
            TryApplyChanges(document);
            NavigatableMarker = ModelMapper.CreateNavigatableMarker(ModelMapper.NodeKind.Ref, ModelMapper.ModelType, typeName);
        }

        private void AddProjectionCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            ((OleMenuCommand)sender).Enabled = CanAddProjection;
        }

        private bool CanAddProjection
        {
            get { return ModelMapper == null ? false : ModelMapper.GetColumns().Any(); }
        }

        private void AddProjectionCommand_Execute(object sender, EventArgs e)
        {
            Debug.Assert(CanAddProjection);
            ProjectionWindow.Show(GetDefaultProjectionTypeName(), ModelMapper, AddProjection);
        }

        private const string LOOKUP_PROJECTION = "Lookup";
        private string GetDefaultProjectionTypeName()
        {
            return _presenter.Rows.Any(AnyLookupProjection) ? null : LOOKUP_PROJECTION;
        }

        private bool AnyLookupProjection(RowPresenter row)
        {
            var node = row.GetValue(_.Node);
            if (node.Kind == ModelMapper.NodeKind.Projection && node.Name == LOOKUP_PROJECTION)
                return true;

            return row.IsExpanded ? false : row.Children.Any(AnyLookupProjection);
        }

        private void AddProjection(string typeName, DataSet<ModelMapper.ProjectionEntry> entries)
        {
            var document = ModelMapper.AddProjection(typeName, entries);
            TryApplyChanges(document);
            NavigatableMarker = ModelMapper.CreateNavigatableMarker(ModelMapper.NodeKind.Projection, ModelMapper.ModelType, typeName);
        }

        private Presenter _presenter;
        private void SetPresenter(Presenter value)
        {
            if (_presenter == value)
                return;

            if (_presenter != null)
                _presenter.CurrentRowChanged -= OnCurrentRowChanged;
            _presenter = value;
            if (_presenter != null)
                _presenter.CurrentRowChanged += OnCurrentRowChanged;
        }

        private RowPresenter CurrentRow
        {
            get { return _presenter?.CurrentRow; }
        }

        private ModelMapper.Node CurrentNode
        {
            get { return CurrentRow?.GetValue(_presenter._.Node); }
        }

        private void OnCurrentRowChanged(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RefreshGotoSourceCommand();
            var currentNode = CurrentNode;
            if (currentNode != null && currentNode.HasLocalLocation && _presenter.NavigationSuggested)
                NavigateToSource(currentNode, false);
        }

        private ModelMapper ModelMapper
        {
            get { return _presenter?.ModelMapper; }
        }

        protected override CodeMapper CodeMapper
        {
            get { return ModelMapper; }
        }

        protected override UIElement Refresh()
        {
            SetPresenter(RefreshPresenter());
            RefreshGotoSourceCommand();
            return _presenter?.View;
        }

        private Presenter RefreshPresenter()
        {
            if (!CanRefresh)
                return null;
            else if (_presenter != null)
                return _presenter.Refresh();
            else
            {
                var modelMapper = ModelMapper.Refresh(null, Document, SelectionSpan);
                return modelMapper == null ? null : new Presenter(modelMapper, this,
                    new CommandBinding(Presenter.GotoSourceCommand, ExecuteGotoSourceCommand, CanExecuteGotoSourceCommand),
                    new CommandBinding(TreeItemView.ShowContextMenuCommand, ExecuteShowContextMenu, CanExecuteShowContextMenu));
            }
        }

        private void ExecuteGotoSourceCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            GotoSource();
        }

        private void CanExecuteGotoSourceCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanGotoSource;
        }

        private void ExecuteShowContextMenu(object sender, ExecutedRoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Debug.Assert(CanShowContextMenu);

            var attributes = CurrentNode.Attributes;
            if (attributes == null || attributes.Count == 0)
                return;

            var uiShell = GetService<IVsUIShell, SVsUIShell>();

            var view = _presenter.View;
            var relativePoint = Mouse.GetPosition(view);
            var screenPoint = view.PointToScreen(relativePoint);

            var point = new POINTS();
            point.x = (short)screenPoint.X;
            point.y = (short)screenPoint.Y;

            var points = new[] { point };

            var commandSetGuid = CommandSetGuid;
            uiShell.ShowContextMenu(0, ref commandSetGuid, CommandId_ContextMenu, points, null);
        }

        private void CanExecuteShowContextMenu(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanShowContextMenu;
        }

        private bool CanShowContextMenu
        {
            get { return CurrentNode != null && CurrentNode.CanGetAttributes; }
        }

        private bool CanGotoSource
        {
            get
            {
                var currentNodeInfo = CurrentNode;
                return currentNodeInfo == null ? false : CanNavigateToSource(currentNodeInfo);
            }
        }

        private void GotoSource()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            NavigateToSource(CurrentNode, true);
        }
    }
}
