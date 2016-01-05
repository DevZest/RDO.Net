using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    internal static class IModelSetExtensions
    {
        public static IModelSet Union(this IModelSet modelSet, IModelSet other)
        {
            if (other == null || other.Count == 0)
                return modelSet;

            if (modelSet == null || modelSet.Count == 0)
                return other;

            return (modelSet.Count <= other.Count) ? QuickUnion(modelSet, other) : QuickUnion(other, modelSet);
        }

        private static IModelSet QuickUnion(IModelSet leftSet, IModelSet rightSet)
        {
            Debug.Assert(leftSet.Count <= rightSet.Count);

            ModelSet result = null;
            foreach (var left in leftSet)
            {
                if (!rightSet.Contains(left))
                {
                    if (result == null)
                        result = new ModelSet(rightSet);
                    result.Add(left);
                }
            }

            return result ?? leftSet;
        }

        public static bool ContainsAny(this IModelSet modelSet, IModelSet other)
        {
            foreach (var model in other)
            {
                if (modelSet.Contains(model))
                    return true;
            }

            return false;
        }

    }
}
