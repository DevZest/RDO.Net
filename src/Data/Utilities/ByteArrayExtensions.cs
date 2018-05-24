using System;

namespace DevZest.Data.Utilities
{
    internal static class ByteArrayExtensions
    {
        public static string ToHexString(this byte[] bytes)
        {
            char[] result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                byte x = ((byte)(bytes[i] >> 4));
                result[i * 2] = (char)(x > 9 ? x + 0x37 : x + 0x30);
                x = ((byte)(bytes[i] & 0xF));
                result[i * 2 + 1] = (char)(x > 9 ? x + 0x37 : x + 0x30);
            }
            return new string(result);
        }
    }
}
