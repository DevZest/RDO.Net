namespace DevZest.Data.Presenters.Primitives
{
    public interface IBlockElement
    {
        void Setup(BlockPresenter blockPresenter);
        void Refresh(BlockPresenter blockPresenter);
        void Cleanup(BlockPresenter blockPresenter);
    }
}
