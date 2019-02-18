using System.Diagnostics;

namespace DevZest.Data
{
    internal static class BinaryExtensions
    {
        private static readonly char[] _hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        internal static string ToHexLitera(this Binary value)
        {
            Debug.Assert(value != null);
            if (value.Length == 0)
                return string.Empty;

            var result = new char[2 * (value.Length + 1)];
            int index = 0;
            result[index++] = '0';
            result[index++] = 'x';
            for (var i = 0; i < value.Length; i++)
            {
                result[index++] = _hexDigits[(value[i] & 0xF0) >> 4];
                result[index++] = _hexDigits[value[i] & 0x0F];
            }
            return new string(result);
        }
    }
}
