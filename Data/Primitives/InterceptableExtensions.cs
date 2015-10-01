
using System.Collections.ObjectModel;

namespace DevZest.Data.Primitives
{
    public static class InterceptableExtensions
    {
        public static ReadOnlyCollection<T> GetInterceptors<T>(this Interceptable interceptable)
            where T : class, IInterceptor
        {
            return interceptable.GetInterceptors<T>();
        }

        public static T GetInterceptor<T>(this Interceptable interceptable)
            where T : class, IInterceptor
        {
            return interceptable.GetInterceptor<T>();
        }

        public static IInterceptor GetInterceptor(this Interceptable interceptable, string interceptorName)
        {
            return interceptable.GetInterceptor(interceptorName);
        }

        public static void AddInterceptor(this Interceptable interceptable, IInterceptor interceptor)
        {
            interceptable.AddInterceptor(interceptor);
        }

        public static void AddOrUpdateInterceptor(this Interceptable interceptable, IInterceptor interceptor)
        {
            interceptable.AddOrUpdateInterceptor(interceptor);
        }

        public static bool ContainsInterceptor(this Interceptable interceptable, string interceptorName)
        {
            return interceptable.ContainsInterceptor(interceptorName);
        }

        public static void RemoveInterceptor(this Interceptable interceptable, string interceptorName)
        {
            interceptable.RemoveInterceptor(interceptorName);
        }
    }
}
