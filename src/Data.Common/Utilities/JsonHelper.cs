using System.Text;

namespace DevZest.Data.Utilities
{
    internal static class JsonHelper
    {
        internal static void WriteObjectName(StringBuilder stringBuilder, string name)
        {
            stringBuilder.Append("\"");
            stringBuilder.Append(name);
            stringBuilder.Append("\"");
            stringBuilder.Append(":");
        }
    }
}
