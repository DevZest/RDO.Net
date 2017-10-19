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
                    v.ForeignKeyExtension = extension;
                },
                onRefresh: (v, p) => refreshAction((TView)v.Content, extension, p),
                onCleanup: (v, p) =>
                {
                    v.Content = null;
                });
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
                    v.ForeignKeyExtension = extension;
                },
                onRefresh: (v, p) => refreshAction((TView)v.Content, key, extension, p),
                onCleanup: (v, p) =>
                {
                    v.Content = null;
                });
        }
    }
}
