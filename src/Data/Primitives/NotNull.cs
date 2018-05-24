using System;

namespace DevZest.Data.Primitives
{
    internal sealed class NotNull : IExtension
    {
        public static readonly NotNull Singleton = new NotNull();

        private NotNull()
        {
        }

        public object Key
        {
            get { return this.GetType(); }
        }
    }
}
