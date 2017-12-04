namespace DevZest.Data.Primitives
{
    public abstract class DbTableElement
    {
        public abstract bool IsMemberOfTable { get; }

        public abstract bool IsMemberOfTempTable { get; }
    }
}
