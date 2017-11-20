using System;

namespace DevZest.Data.Primitives
{
    internal sealed class NotNull : IInterceptor
    {
        public static readonly NotNull Singleton = new NotNull();

        private NotNull()
        {
        }

        public string FullName
        {
            get { return this.GetType().FullName; }
        }
    }
}
