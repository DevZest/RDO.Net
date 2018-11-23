namespace DevZest.Data
{
    public static class CandidateKeyExtensions
    {
        public static KeyMapping Match<T>(this IKey<T> source, T target)
            where T : CandidateKey
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(source.PrimaryKey, target);
        }

        public static KeyMapping Match<T>(this IKey<T> source, IKey<T> target)
            where T : CandidateKey
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(source.PrimaryKey, target.PrimaryKey);
        }

        public static KeyMapping Join<T>(this T sourceKey, IKey<T> target)
            where T : CandidateKey
        {
            return new KeyMapping(sourceKey, target.PrimaryKey);
        }
    }
}
