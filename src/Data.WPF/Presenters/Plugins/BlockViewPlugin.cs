using System;
using DevZest.Data.Views;

namespace DevZest.Data.Presenters.Plugins
{
    public abstract class BlockViewPlugin : IBlockViewPlugin
    {
        protected abstract void Setup(BlockView blockView);

        protected abstract void Refresh(BlockView blockView);

        protected abstract void Cleanup(BlockView blockView);

        void IBlockViewPlugin.Setup(BlockView blockView)
        {
            Setup(blockView);
        }

        void IBlockViewPlugin.Refresh(BlockView blockView)
        {
            Refresh(blockView);
        }

        void IBlockViewPlugin.Cleanup(BlockView blockView)
        {
            Cleanup(blockView);
        }
    }
}
