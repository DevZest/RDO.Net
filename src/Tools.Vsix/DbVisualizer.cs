using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DevZest.Data.Tools
{
    [Guid("5B0D702D-D53D-421C-881B-4EDD7B851BA2")]
    public partial class DbVisualizer : CodeVisualizer
    {
        public static readonly Guid CommandSetGuid = new Guid("810E8186-6D2A-471C-9F0F-0644919459BF");
        public const int CommandId_Show = 0x110;
        private const int CommandId_Toolbar = 0x0100;
        private const int CommandId_ContextMenu = 0x0105;
        private const int CommandId_AddTable = 0x210;
        private const int CommandId_AddRelationship = 0x220;
        private const int CommandId_GotoSource = 0x230;
        private const int CommandId_Refresh = 0x240;

        public DbVisualizer()
        {
            Caption = UserMessages.DbVisualizer_Caption;
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
            InitAddTableCommand(commandService);
            InitAddRelationshipCommand(commandService);
            InitGotoSourceCommand(commandService);
            InitRefreshCommand(commandService);
        }

        private void InitAddTableCommand(OleMenuCommandService commandService)
        {
            var command = new OleMenuCommand(AddTableCommand_Execute, new CommandID(CommandSetGuid, CommandId_AddTable));
            command.BeforeQueryStatus += AddTableCommand_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private void AddTableCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            ((OleMenuCommand)sender).Enabled = CanAddTable;
        }

        private bool CanAddTable
        {
            get { return DbMapper != null && DbMapper.CodeSnippetInsertionSpan.HasValue; }
        }

        private void AddTableCommand_Execute(object sender, EventArgs e)
        {
            Debug.Assert(CanAddTable);
            DbTableWindow.Show(DbMapper, AddDbTable);
        }

        private void AddDbTable(INamedTypeSymbol modelType, string name, string dbName, string description)
        {
            var document = DbMapper.AddDbTable(modelType, name, dbName, description);
            TryApplyChanges(document);
            NavigatableMarker = DbMapper.CreateTableMarker(name);
        }

        private void InitAddRelationshipCommand(OleMenuCommandService commandService)
        {
            var command = new OleMenuCommand(AddRelationshipCommand_Execute, new CommandID(CommandSetGuid, CommandId_AddRelationship));
            command.BeforeQueryStatus += AddRelationshipCommand_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private void AddRelationshipCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            ((OleMenuCommand)sender).Enabled = CanAddRelationship;
        }

        private bool CanAddRelationship
        {
            get { return CurrentNode != null && CurrentNode.CanAddRelationship; }
        }

        private void AddRelationshipCommand_Execute(object sender, EventArgs e)
        {
            Debug.Assert(CanAddRelationship);
            RelationshipWindow.Show(DbMapper, (IPropertySymbol)CurrentNode.Symbol, AddRelationship);
        }

        private void AddRelationship(string name, IPropertySymbol foreignKey, IPropertySymbol refTable, string description, ForeignKeyRule deleteRule, ForeignKeyRule updateRule)
        {
            var document = CurrentNode.AddRelationship(name, foreignKey, refTable, description, deleteRule, updateRule);
            TryApplyChanges(document);
            NavigatableMarker = DbMapper.CreateTableMarker(CurrentNode.Name);
        }

        private OleMenuCommand _gotoSourceCommand;
        private void InitGotoSourceCommand(OleMenuCommandService commandService)
        {
            _gotoSourceCommand = new OleMenuCommand(GotoSourceCommand_Execute, new CommandID(CommandSetGuid, CommandId_GotoSource));
            _gotoSourceCommand.BeforeQueryStatus += GotoSourceCommand_BeforeQueryStatus;
            commandService.AddCommand(_gotoSourceCommand);
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

        private void OnCurrentRowChanged(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RefreshGotoSourceCommand();
            var currentNode = CurrentNode;
            if (currentNode != null && currentNode.HasLocalLocation && _presenter.NavigationSuggested)
                NavigateToSource(currentNode, false);
        }

        private RowPresenter CurrentRow
        {
            get { return _presenter?.CurrentRow; }
        }

        private DbMapper.Node CurrentNode
        {
            get { return CurrentRow?.GetValue(_presenter._.Node); }
        }

        private DbMapper DbMapper
        {
            get { return _presenter?.DbMapper; }
        }

        protected override CodeMapper CodeMapper
        {
            get { return DbMapper; }
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
                var dbMapper = DbMapper.Refresh(null, Document, SelectionSpan);
                return dbMapper == null ? null : new Presenter(dbMapper, this,
                    new CommandBinding(Presenter.GotoSourceCommand, ExecuteGotoSourceCommand, CanExecuteGotoSourceCommand),
                    new CommandBinding(TreeItemView.ShowContextMenuCommand, ExecuteShowContextMenu, CanExecuteShowContextMenu));
            }
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

        private void ExecuteGotoSourceCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            GotoSource();
        }

        private void CanExecuteGotoSourceCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanGotoSource;
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

        private void ExecuteShowContextMenu(object sender, ExecutedRoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Debug.Assert(CanAddRelationship);

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
            e.CanExecute = CanAddRelationship;
        }
    }
}
