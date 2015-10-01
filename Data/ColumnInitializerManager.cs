using System;
using System.Runtime.CompilerServices;

namespace DevZest.Data
{
    internal static class ColumnInitializerManager<T>
        where T : Column
    {
        static ConditionalWeakTable<T, Action<T>> s_initializers = new ConditionalWeakTable<T, Action<T>>();

        public static void SetInitializer(T column, Action<T> initializer)
        {
            if (initializer != null)
                s_initializers.Add(column, initializer);
        }

        public static Action<T> GetInitializer(T column)
        {
            Action<T> result;
            if (s_initializers.TryGetValue(column, out result))
                return result;
            return null;
        }
    }
}
