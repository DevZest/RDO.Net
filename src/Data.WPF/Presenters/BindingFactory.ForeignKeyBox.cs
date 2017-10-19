using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    partial class BindingFactory
    {
        public static RowBinding<ForeignKeyBox> AsForeignKeyBox<TKey, TExtension, TView>(this TKey key, TExtension extension, Action<TView, TExtension, RowPresenter> refreshAction)
            where TKey : KeyBase
            where TExtension : ModelExtension
            where TView : UIElement, new()
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));
            if (refreshAction == null)
                throw new ArgumentNullException(nameof(refreshAction));

            return new RowBinding<ForeignKeyBox>(
                onSetup: (v, p) =>
                {
                    v.Content = new TView();
                    v.ForeignKey = key;
                    v.Extension = extension;
                },
                onRefresh: (v, p) => refreshAction((TView)v.Content, extension, p),
                onCleanup: (v, p) =>
                {
                    v.Content = null;
                }).WithInput(key, extension);
        }

        public static RowBinding<ForeignKeyBox> AsForeignKeyBox<TKey, TExtension, TView>(this TKey key, TExtension extension, Action<TView, TKey, TExtension, RowPresenter> refreshAction)
            where TKey : KeyBase
            where TExtension : ModelExtension
            where TView : UIElement, new()
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));
            if (refreshAction == null)
                throw new ArgumentNullException(nameof(refreshAction));

            return new RowBinding<ForeignKeyBox>(
                onSetup: (v, p) =>
                {
                    v.Content = new TView();
                    v.ForeignKey = key;
                    v.Extension = extension;
                },
                onRefresh: (v, p) => refreshAction((TView)v.Content, key, extension, p),
                onCleanup: (v, p) =>
                {
                    v.Content = null;
                }).WithInput(key, extension);
        }

        private static RowBinding<ForeignKeyBox> WithInput(this RowBinding<ForeignKeyBox> rowBinding, KeyBase foreignKey, ModelExtension extension)
        {
            var rowInput = rowBinding.BeginInput(new PropertyChangedTrigger<ForeignKeyBox>(ForeignKeyBox.ValueBagProperty));
            foreach (var columnSort in foreignKey)
                rowInput.WithFlush(columnSort.Column, v => v.ValueBag);
            if (extension != null)
            {
                foreach (var column in extension.Columns)
                {
                    if (column.IsExpression)
                        continue;
                    rowInput.WithFlush(column, v => v.ValueBag);
                }
            }
            return rowInput.EndInput();
        }
    }
}
