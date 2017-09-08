using System;
using System.Windows;

namespace DevZest.Data.Presenters.Plugins
{
    public static class Extensions
    {
        public static RowBinding<T> AddPlugin<T, TPlugin>(this RowBinding<T> rowBinding, RowBindingPlugin<TPlugin> plugin)
            where TPlugin : UIElement, new()
            where T : TPlugin, new()
        {
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));
            rowBinding.InternalAddPlugin(plugin);
            return rowBinding;
        }
    }
}
