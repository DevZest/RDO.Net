using System;

namespace DevZest.Data.MySql
{
    /// <summary>
    /// Specifies MySQL version.
    /// </summary>
    public struct MySqlVersion
    {
        private readonly string _srcString;

        /// <summary>
        /// Gets the lowest version (8.0.4) which is supported.
        /// </summary>
        public static MySqlVersion LowestSupported
        {
            get { return new MySqlVersion("8.0.4", 8, 0, 4); }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MySqlVersion"/>.
        /// </summary>
        /// <param name="s">The leading string.</param>
        /// <param name="major">The major number of the version.</param>
        /// <param name="minor">The minor number of the version.</param>
        /// <param name="build">The build number of the version.</param>
        public MySqlVersion(string s, int major, int minor, int build)
        {
            Major = major;
            Minor = minor;
            Build = build;
            _srcString = s;
        }

        /// <summary>
        /// Gets the major number of the version.
        /// </summary>
        public int Major { get; }

        /// <summary>
        /// Gets the Minor number of the version.
        /// </summary>
        public int Minor { get; }

        /// <summary>
        /// Gets the build number of the version.
        /// </summary>
        public int Build { get; }

        /// <summary>
        /// Parses string into <see cref="MySqlVersion"/>.
        /// </summary>
        /// <param name="versionString">The input string.</param>
        /// <returns>The result.</returns>
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

        /// <summary>
        /// Determines whether current version is equal or greater than specified version.
        /// </summary>
        /// <param name="major">The major number of the specified version.</param>
        /// <param name="minor">The minor number of the specified version.</param>
        /// <param name="build">The build number of the specified version.</param>
        /// <returns><see langword="true"/> if current version is equal or greater than specified version.</returns>
        public bool IsAtLeast(int major, int minor, int build)
        {
            if (Major > major) return true;
            if (Major == major && Minor > minor) return true;
            if (Major == major && Minor == minor && Build >= build) return true;
            return false;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _srcString;
        }
    }
}
