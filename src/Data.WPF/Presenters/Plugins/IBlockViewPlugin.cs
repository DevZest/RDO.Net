using DevZest.Data.Views;

namespace DevZest.Data.Presenters.Plugins
{
    public interface IBlockViewPlugin
    {
        void Setup(BlockView blockView);
        void Refresh(BlockView blockView);
        void Cleanup(BlockView blockView);
    }
}
