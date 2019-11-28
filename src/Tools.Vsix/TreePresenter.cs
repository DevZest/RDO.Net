using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace DevZest.Data.Tools
{
    public abstract class TreePresenter<T> : DataPresenter<T>
        where T : Model, ITreeItem, new()
    {
        protected TreePresenter(CodeVisualizer codeVisualizer, params CommandBinding[] commandBindings)
        {
            _codeVisualizer = codeVisualizer;
            ViewToShow = CreateDataView(commandBindings);
        }

        private readonly CodeVisualizer _codeVisualizer;

        protected Document Document
        {
            get { return _codeVisualizer.Document; }
        }

        protected TextSpan SelectionSpan
        {
            get { return _codeVisualizer.SelectionSpan; }
        }

        private bool ShouldHaveFocus
        {
            get { return _codeVisualizer.IsActive; }
        }

        private DataView ViewToShow { get; }

        private DataView CreateDataView(CommandBinding[] commandBindings)
        {
            var result = new VsDataView();
            if (commandBindings != null)
            {
                for (int i = 0; i < commandBindings.Length; i++)
                {
                    var commandBinding = commandBindings[i];
                    if (commandBinding != null)
                        result.CommandBindings.Add(commandBindings[i]);
                }
            }
            return result;
        }

        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder
                .MakeRecursive(_.GetChild())
                .GridRows("Auto")
                .GridColumns("*")
                .Layout(Orientation.Vertical)
                .WithSelectionMode(SelectionMode.Single)
                .WithInitialFocus(ShouldHaveFocus ? InitialFocus.First : InitialFocus.None)
                .AddBinding(0, 0, BindToTreeItemView());
        }

        protected abstract RowBinding<TreeItemView> BindToTreeItemView();

        public EventHandler CurrentRowChanged = delegate { };
        protected override void OnCurrentRowChanged(RowPresenter oldValue)
        {
            base.OnCurrentRowChanged(oldValue);
            CurrentRowChanged(this, EventArgs.Empty);
        }

        protected void RefreshView(DataSet<T> nodes)
        {
            _marker = _codeVisualizer.NavigatableMarker;
            Show(ViewToShow, nodes);
            var root = Rows[0];
            if (!root.IsExpanded)
                root.ToggleExpandState();
        }

        public bool NavigationSuggested
        {
            get { return _marker == null || _marker.NavigationSuggested; }
        }

        private INavigatableMarker _marker;
        protected override void OnViewRefreshed()
        {
            base.OnViewRefreshed();
            if (_marker != null)
            {
                Match(_marker.ShouldExpand);
                _marker = null;
            }
        }

        private void Match(bool shouldExpand)
        {
            Debug.Assert(_marker != null);

            var row = GetMatchedRow();
            if (row == null)
                return;

            for (var parent = row.Parent; parent != null; parent = parent.Parent)
            {
                if (!parent.IsExpanded)
                    parent.ToggleExpandState();
            }

            Select(row, SelectionMode.Single);
            if (shouldExpand && !row.IsExpanded && row.HasChildren)
                row.ToggleExpandState();
        }

        private RowPresenter GetMatchedRow()
        {
            for (int i = 0; i < Rows.Count; i++)
            {
                var row = Rows[i];
                var result = GetMatchedRow(row, !row.IsExpanded);
                if (result != null)
                    return result;
            }

            return null;
        }

        private RowPresenter GetMatchedRow(RowPresenter row, bool matchChildren)
        {
            if (IsMatched(row))
                return row;

            if (row.HasChildren && matchChildren)
            {
                var children = row.Children;
                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];
                    var result = GetMatchedRow(child, true);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        private bool IsMatched(RowPresenter row)
        {
            var nodeInfo = row.GetValue(_.TreeNode);
            return _marker.Matches(nodeInfo);
        }
    }
}
