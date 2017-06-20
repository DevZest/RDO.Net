using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public class StyleKey
    {
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
            get { return _style ?? (_style = (Style)(LoadResourceDictionary(_uriString)[this])); }
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

            var defaultNamespace = DefaultNamespaceAttribute.GetDefaultNamespace(type.Assembly);

            if (result.StartsWith(defaultNamespace))
                result = result.Substring(defaultNamespace.Length, result.Length - defaultNamespace.Length);

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
