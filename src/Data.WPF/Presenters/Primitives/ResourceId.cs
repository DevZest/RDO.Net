using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class ResourceId<T>
        where T : class
    {
        static ResourceId()
        {
            // Workaround UriFormatException: Invalid URI: Invalid port specified
            // https://stackoverflow.com/questions/6005398/uriformatexception-invalid-uri-invalid-port-specified
            var currentApp = Application.Current;
        }

        protected ResourceId(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            _uriString = GetUriString(type);
            Trace.WriteLine(string.Format("_uriString={0}", _uriString));
        }

        private readonly string _uriString;
        private T _loadedResource;
        public T GetOrLoad(bool throwsExceptionIfFailed = true)
        {
            return _loadedResource ?? (_loadedResource = LoadResource(throwsExceptionIfFailed));
        }

        private T LoadResource(bool throwsExceptionIfFailed)
        {
            var result = LoadResourceDictionary(_uriString)[this] as T;
            if (throwsExceptionIfFailed && result == null)
                throw new InvalidOperationException(DiagnosticMessages.ResourceId_ResourceNotFound);
            return result;
        }

        private string GetUriString(Type type)
        {
            return string.Format(CultureInfo.InvariantCulture, "pack://application:,,,/{0};component/{1}{2}.{3}.xaml", type.Assembly.GetName().Name, GetPath(type), type.Name, UriSuffix);
        }

        protected abstract string UriSuffix { get; }

        private static string GetPath(Type type)
        {
            var result = type.Namespace;
            if (result.Length == 0)
                return result;

            var relativeTo = ResourceIdRelativeToAttribute.GetResourceIdRelativeTo(type.Assembly);

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
