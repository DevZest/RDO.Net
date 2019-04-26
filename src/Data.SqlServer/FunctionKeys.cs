using DevZest.Data.Primitives;

namespace DevZest.Data.SqlServer
{
    internal static class FunctionKeys
    {
        public static readonly FunctionKey XmlValue = new FunctionKey(typeof(Functions), nameof(XmlValue));

        public static readonly FunctionKey Like = new FunctionKey(typeof(Functions), nameof(Like));

        public static readonly FunctionKey NotLike = new FunctionKey(typeof(Functions), nameof(NotLike));
    }
}
