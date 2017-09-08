using System;
using DevZest.Data.Views;

namespace DevZest.Data.Presenters.Plugins
{
    public abstract class BlockViewPlugin
    {
        protected internal abstract void Setup(BlockView blockView);

        protected internal abstract void Refresh(BlockView blockView);

        protected internal abstract void Cleanup(BlockView blockView);
    }
}
