namespace DevZest.Data
{
    public static class PrimaryKeyExtensions
    {
        public static KeyMapping Match<T>(this IKey<T> source, T target)
            where T : PrimaryKey
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(source.PrimaryKey, target);
        }

        public static KeyMapping Match<T>(this IKey<T> source, IKey<T> target)
            where T : PrimaryKey
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(source.PrimaryKey, target.PrimaryKey);
        }
    }
}
