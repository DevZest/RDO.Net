
using DevZest.Data.Primitives;

namespace DevZest.Data.SqlServer
{
    internal static class FunctionKeys
    {
        public static FunctionKey XmlValue = new FunctionKey(typeof(Functions), nameof(XmlValue));
    }
}
