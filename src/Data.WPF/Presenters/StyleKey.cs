using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public class StyleKey
    {
        static StyleKey()
        {
            // Workaround UriFormatException: Invalid URI: Invalid port specified
            // https://stackoverflow.com/questions/6005398/uriformatexception-invalid-uri-invalid-port-specified
            var currentApp = Application.Current;
        }

        public StyleKey(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            _uriString = GetUriString(type);
        }

        private readonly string _uriString;
        private Style _style;
        public Style Style
        {
            get { return _style ?? (_style = LoadStyle()); }
        }

        private Style LoadStyle()
        {
            var result = (Style)(LoadResourceDictionary(_uriString)[this]);
            if (result == null)
                throw new InvalidOperationException(Strings.StyleKey_StyleNotFound);
            return result;
        }

        private static string GetUriString(Type type)
        {
            return string.Format(CultureInfo.InvariantCulture, "pack://application:,,,/{0};component/{1}{2}.Styles.xaml", type.Assembly.GetName().Name, GetPath(type), type.Name);
        }

        private static string GetPath(Type type)
        {
            var result = type.Namespace;
            if (result.Length == 0)
                return result;

            var relativeTo = StyleKeyRelativeToAttribute.GetStyleKeyRelativeTo(type.Assembly);

            if (result.StartsWith(relativeTo))
                result = result.Substring(relativeTo.Length, result.Length - relativeTo.Length);

            if (result.StartsWith("."))
                result = result.Substring(1, result.Length - 1);
            return result.Replace('.', '/') + "/";
        }

        private static ResourceDictionary LoadResourceDictionary(string uriString)
        {
            return new ResourceDictionary
            {
                Source = new Uri(uriString, UriKind.RelativeOrAbsolute)
            };
        }
    }
}
