namespace DevZest.Data.CodeAnalysis
{
    internal interface IInitializable<T>
    {
        void Initialize(T owner);
    }
}
