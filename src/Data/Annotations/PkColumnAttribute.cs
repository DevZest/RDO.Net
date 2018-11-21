namespace DevZest.Data.Annotations
{
    public sealed class PkColumnAttribute
    {
        public PkColumnAttribute(int index = 0)
        {
            Index = index;
        }

        public int Index { get; }
    }
}
