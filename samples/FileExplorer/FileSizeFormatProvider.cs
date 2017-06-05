using System;

namespace FileExplorer
{
    /// <summary>
    /// Format a size in bytes to KB, MB or GB.
    /// </summary>
    /// <example><code>
    ///Debug.WriteLine(string.Format(new FileSizeFormatProvider(), "{0:2}", 10240)); //display 10.00 KB
    ///Debug.WriteLine(string.Format(new FileSizeFormatProvider(), "{0:KB}", 1024)); //display 1 KB
    ///Debug.WriteLine(string.Format(new FileSizeFormatProvider(), "{0:KB1}", 16245)); //display 15.9 KB
    ///Debug.WriteLine(string.Format(new FileSizeFormatProvider(), "{0:MB1}", 102450)); //display 0.1 MB
    ///Debug.WriteLine(string.Format(new FileSizeFormatProvider(), "{0:GB1}", 1073741824)); //display 1.0 GB
    /// </code></example>
    /// http://martinwilley.com/net/code/forms/filesizeformatprovider.html
    public class FileSizeFormatProvider : IFormatProvider, ICustomFormatter
    {
        public static readonly FileSizeFormatProvider Singleton = new FileSizeFormatProvider();

        private FileSizeFormatProvider()
        {
        }

        #region IFormatProvider Members
        public object GetFormat(Type formatType)
        {
            return (formatType == typeof(ICustomFormatter)) ? this : null;
        }
        #endregion

        #region ICustomFormatter Members
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            string s = TryFormat(format, arg);
            if (!string.IsNullOrEmpty(s))
                return s; //we successfully formated
            //fall through to default formating http://msdn.microsoft.com/en-us/library/system.icustomformatter.format.aspx
            if (arg is IFormattable)
                s = ((IFormattable)arg).ToString(format, formatProvider);
            else if (arg != null)
                s = arg.ToString();
            return s;
        }
        #endregion

        private static string TryFormat(string format, object arg)
        {
            if (string.IsNullOrEmpty(format)) return null;
            if (!(format.StartsWith("KB", StringComparison.OrdinalIgnoreCase) ||
                format.StartsWith("MB", StringComparison.OrdinalIgnoreCase) ||
                format.StartsWith("GB", StringComparison.OrdinalIgnoreCase) ||
                format.StartsWith("FS", StringComparison.OrdinalIgnoreCase))) return null;
            //only on numeric types (FileInfo.Length is a long)
            if (!(arg is long || arg is decimal || arg is int)) return null;

            //try to convert to decimal
            decimal size;
            try
            {
                size = Convert.ToDecimal(arg);
            }
            catch (InvalidCastException)
            {
                size = 0;
            }
            //try to parse the decimal points
            int dps;
            if (format.Length > 2)
                int.TryParse(format.Substring(2), out dps);
            else
                int.TryParse(format, out dps);

            if (format.Length > 1)                //standardize
                format = format.Substring(0, 2).ToUpper(); //get first two chars

            const int KB = 1024;
            const int MB = KB * 1024;
            const int GB = MB * 1024;
            string suffix = "B";

            //if one of KB, MB or GB, divide, round and stringify
            switch (format)
            {
                case "KB":
                    size = size / KB;
                    suffix = "KB";
                    break;
                case "MB":
                    size = size / MB;
                    suffix = "MB";
                    break;
                case "GB":
                    size = size / GB;
                    suffix = "GB";
                    break;
                case "FS":
                    if (size >= GB)
                    {
                        size = size / GB;
                        suffix = "GB";
                    }
                    else if (size >= MB)
                    {
                        size = size / MB;
                        suffix = "MB";
                    }
                    else if (size >= KB)
                    {
                        size = size / KB;
                        suffix = "KB";
                    }
                    break;
            }
            return string.Format("{0:N" + dps + "} " + suffix, size);
        }
    }
}
