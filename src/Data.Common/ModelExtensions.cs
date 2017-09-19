using System;

namespace DevZest.Data
{
    public static class ModelExtensions
    {
        public static Relationship Join<TSource, TTarget, TKey>(this TSource source, TTarget target, Func<TSource, TKey> sourceKeyGetter, Func<TTarget, TKey> targetKeyGetter)
            where TSource : Model, new()
            where TTarget : Model, new()
            where TKey : KeyBase
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (sourceKeyGetter == null)
                throw new ArgumentNullException(nameof(sourceKeyGetter));
            if (targetKeyGetter == null)
                throw new ArgumentNullException(nameof(targetKeyGetter));
            var sourceKey = sourceKeyGetter(source);
            var targetKey = targetKeyGetter(target);
            return Data.Relationship.Create(sourceKey, targetKey);
        }
    }
}
