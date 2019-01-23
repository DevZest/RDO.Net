using System;

namespace DevZest.Data.MySql
{
    public struct MySqlVersion
    {
        private readonly string _srcString;

        public MySqlVersion(string s, int major, int minor, int build)
        {
            Major = major;
            Minor = minor;
            Build = build;
            _srcString = s;
        }

        public int Major { get; }

        public int Minor { get; }

        public int Build { get; }

        public static MySqlVersion Parse(string versionString)
        {
            int start = 0;
            int index = versionString.IndexOf('.', start);
            if (index == -1)
                throw new ArgumentException(DiagnosticMessages.BadVersionFormat, nameof(versionString));
            string val = versionString.Substring(start, index - start).Trim();
            int major = Convert.ToInt32(val, System.Globalization.NumberFormatInfo.InvariantInfo);

            start = index + 1;
            index = versionString.IndexOf('.', start);
            if (index == -1)
                throw new ArgumentException(DiagnosticMessages.BadVersionFormat, nameof(versionString));
            val = versionString.Substring(start, index - start).Trim();
            int minor = Convert.ToInt32(val, System.Globalization.NumberFormatInfo.InvariantInfo);

            start = index + 1;
            int i = start;
            while (i < versionString.Length && Char.IsDigit(versionString, i))
                i++;
            val = versionString.Substring(start, i - start).Trim();
            int build = Convert.ToInt32(val, System.Globalization.NumberFormatInfo.InvariantInfo);

            return new MySqlVersion(versionString, major, minor, build);
        }

        public bool IsAtLeast(int majorNum, int minorNum, int buildNum)
        {
            if (Major > majorNum) return true;
            if (Major == majorNum && Minor > minorNum) return true;
            if (Major == majorNum && Minor == minorNum && Build >= buildNum) return true;
            return false;
        }

        public override string ToString()
        {
            return _srcString;
        }
    }
}
