using System.Collections.Generic;
using System.Windows;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Windows.Input;

namespace FileExplorer
{
    public abstract class DirectoryList<T> : DataPresenter<T>, InPlaceEditor.ICommandService, InPlaceEditor.ISwitcher
        where T : DirectoryItem, new()
    {
        protected sealed override void BuildTemplate(TemplateBuilder builder)
        {
            builder.WithRowViewBeginEditGestures(new KeyGesture(Key.F2))
                .WithRowViewCancelEditGestures(new KeyGesture(Key.Escape))
                .WithRowViewEndEditGestures(new KeyGesture(Key.Enter));
            OverrideBuildTemplate(builder);
        }

        protected abstract void OverrideBuildTemplate(TemplateBuilder builder);

        IEnumerable<CommandEntry> InPlaceEditor.ICommandService.GetCommandEntries(InPlaceEditor inPlaceEditor)
        {
            yield return RowView.Commands.BeginEdit.Bind(BeginEdit, CanBeginEdit, new MouseGesture(MouseAction.LeftClick));
        }

        private void CanBeginEdit(object sender, CanExecuteRoutedEventArgs e)
        {
            var rowView = RowView.GetCurrent((InPlaceEditor)sender);
            var rowPresenter = rowView.RowPresenter;
            e.CanExecute = rowPresenter.IsCurrent && !rowPresenter.IsEditing;
            if (!e.CanExecute)
                e.ContinueRouting = true;
        }

        private void BeginEdit(object sender, ExecutedRoutedEventArgs e)
        {
            var rowView = RowView.GetCurrent((InPlaceEditor)sender);
            var rowPresenter = rowView.RowPresenter;
            rowPresenter.BeginEdit();
        }

        bool InPlaceEditor.ISwitcher.AffectsIsEditing(InPlaceEditor inPlaceEditor, DependencyProperty dp)
        {
            return dp == InPlaceEditor.IsRowEditingProperty;
        }

        bool InPlaceEditor.ISwitcher.GetIsEditing(InPlaceEditor inPlaceEditor)
        {
            return inPlaceEditor.IsRowEditing;
        }

        bool InPlaceEditor.ISwitcher.ShouldFocusToEditorElement(InPlaceEditor inPlaceEditor)
        {
            return true;
        }
    }
}
