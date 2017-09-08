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

        public static ScalarBinding<T> AddPlugin<T, TPlugin>(this ScalarBinding<T> scalarBinding, ScalarBindingPlugin<TPlugin> plugin)
            where TPlugin : UIElement, new()
            where T : TPlugin, new()
        {
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));
            scalarBinding.InternalAddPlugin(plugin);
            return scalarBinding;
        }

        public static BlockBinding<T> AddPlugin<T, TPlugin>(this BlockBinding<T> blockBinding, BlockBindingPlugin<TPlugin> plugin)
            where TPlugin : UIElement, new()
            where T : TPlugin, new()
        {
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));
            blockBinding.InternalAddPlugin(plugin);
            return blockBinding;
        }
    }
}
