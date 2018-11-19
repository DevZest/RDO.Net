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

        public static KeyMapping Join<T>(this T sourceKey, T targetKey)
            where T : PrimaryKey, new()
        {
            return new KeyMapping(sourceKey, targetKey);
        }

        public static KeyMapping UnsafeJoin(this PrimaryKey sourceKey, PrimaryKey targetKey)
        {
            return new KeyMapping(sourceKey, targetKey);
        }
    }
}
