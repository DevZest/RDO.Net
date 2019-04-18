namespace DevZest.Data
{
    public static class CandidateKeyExtensions
    {
        public static KeyMapping Match<T>(this IEntity<T> source, T target)
            where T : CandidateKey
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(source.Model.PrimaryKey, target);
        }

        public static KeyMapping Match<T>(this IEntity<T> source, IEntity<T> target)
            where T : CandidateKey
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(source.Model.PrimaryKey, target.Model.PrimaryKey);
        }

        public static KeyMapping Join<T>(this T sourceKey, IEntity<T> target)
            where T : CandidateKey
        {
            return new KeyMapping(sourceKey, target.Model.PrimaryKey);
        }
    }
}
