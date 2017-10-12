using DevZest.Data.Views;
using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    partial class BindingFactory
    {
        public static RowBinding<ForeignKeyBox> AsForeignKeyBox<TKey, TView>(this TKey key, Action<TView, TKey, RowPresenter> refreshAction)
            where TKey : KeyBase
            where TView : UIElement, new()
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (refreshAction == null)
                throw new ArgumentNullException(nameof(refreshAction));

            return new RowBinding<ForeignKeyBox>(
                onSetup: (v, p) => v.Content = new TView(),
                onRefresh: (v, p) => refreshAction((TView)v.Content, key, p),
                onCleanup: (v, p) => v.Content = null);
        }
    }
}
