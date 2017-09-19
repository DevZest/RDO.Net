using System;

namespace DevZest.Data
{
    public static class JoinExtensions
    {
        public static Join Join<T>(this Model<T> source, Model<T> target)
            where T : KeyBase
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            return Data.Join.Create(source.PrimaryKey, target.PrimaryKey);
        }

        public static Join JoinTo<TSource, TTargetKey>(this TSource source, Model<TTargetKey> target, Func<TSource, TTargetKey> keyGetter)
            where TSource : Model, new()
            where TTargetKey : KeyBase
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (keyGetter == null)
                throw new ArgumentNullException(nameof(keyGetter));

            var sourceKey = keyGetter(source);
            var targetKey = target.PrimaryKey;
            return Data.Join.Create(sourceKey, targetKey);
        }

        public static Join JoinFrom<TSourceKey, TTarget>(this Model<TSourceKey> source, TTarget target, Func<TTarget, TSourceKey> keyGetter)
            where TSourceKey : KeyBase
            where TTarget : Model, new()
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (keyGetter == null)
                throw new ArgumentNullException(nameof(keyGetter));

            var sourceKey = source.PrimaryKey;
            var targetKey = keyGetter(target);
            return Data.Join.Create(sourceKey, targetKey);
        }

        public static Join Join<TSource, TTarget, TKey>(this TSource source, TTarget target, Func<TSource, TKey> sourceKeyGetter, Func<TTarget, TKey> targetKeyGetter)
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
            return Data.Join.Create(sourceKey, targetKey);
        }
    }
}
