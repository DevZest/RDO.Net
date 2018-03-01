using DevZest.Data;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace FileExplorer
{
    public sealed class CurrentDirectoryBar : DirectoryPresenter<CurrentDirectoryBar.Item>,
        InPlaceEditor.ICommandService, InPlaceEditor.ISwitcher
        DataView.ICommandService
    {
        public sealed class Item : Model
        {
        }

        public CurrentDirectoryBar(DataView dataView, DirectoryTree directoryTree)
            : base(directoryTree)
        {
            _currentDirectory = NewScalar<string>();
            CurrentDirectory = DirectoryTree.CurrentPath;

            var dataSet = DataSet<Item>.New();
            Show(dataView, dataSet);
        }

        private readonly Scalar<string> _currentDirectory;
        protected sealed override string CurrentDirectory
        {
            get { return _currentDirectory.GetValue(true); }
            set { _currentDirectory.SetValue(value, true); }
        }

        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder.GridColumns("*")
                .GridRows("Auto", "0")
                .RowRange(0, 1, 0, 1)
                .AddBinding(0, 0, _currentDirectory.BindToTextBox().Input.AddToInPlaceEditor(_currentDirectory.BindToTextBlock()));
        }

        IEnumerable<CommandEntry> InPlaceEditor.ICommandService.GetCommandEntries(InPlaceEditor inPlaceEditor)
        {
            yield return RowView.Commands.BeginEdit.Bind(BeginEdit, CanBeginEdit, new MouseGesture(MouseAction.LeftClick));
        }

        private void CanBeginEdit(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !ScalarContainer.IsEditing;
            if (!e.CanExecute)
                e.ContinueRouting = true;
        }

        private void BeginEdit(object sender, ExecutedRoutedEventArgs e)
        {
            ScalarContainer.BeginEdit();
        }

        bool InPlaceEditor.ISwitcher.AffectsIsEditing(InPlaceEditor inPlaceEditor, DependencyProperty dp)
        {
            return dp == InPlaceEditor.IsScalarEditingProperty;
        }

        bool InPlaceEditor.ISwitcher.GetIsEditing(InPlaceEditor inPlaceEditor)
        {
            return inPlaceEditor.IsScalarEditing;
        }

        bool InPlaceEditor.ISwitcher.ShouldFocusToEditorElement(InPlaceEditor inPlaceEditor)
        {
            return true;
        }

        IEnumerable<CommandEntry> DataView.ICommandService.GetCommandEntries(DataView dataView)
        {
            var baseService = ServiceManager.GetService<DataView.ICommandService>(this);
            foreach (var entry in baseService.GetCommandEntries(dataView))
            {
                if (entry.Command == DataView.Commands.CancelEditScalars)
                    yield return entry.ReplaceWith(new KeyGesture(Key.Escape));
                else if (entry.Command == DataView.Commands.EndEditScalars)
                    yield return entry.ReplaceWith(new KeyGesture(Key.Enter));
                else
                    yield return entry;
            }
        }
    }
}
