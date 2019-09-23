using System;
using System.Globalization;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Represents Id used to load resource.
    /// </summary>
    /// <typeparam name="T">The type of the resource.</typeparam>
    public abstract class ResourceId<T>
        where T : class
    {
        static ResourceId()
        {
            // Workaround UriFormatException: Invalid URI: Invalid port specified
            // https://stackoverflow.com/questions/6005398/uriformatexception-invalid-uri-invalid-port-specified
            var currentApp = Application.Current;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ResourceId{T}"/> class.
        /// </summary>
        /// <param name="type">The type that will be used to resolve the resource URI.</param>
        protected ResourceId(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            _uriString = GetUriString(type);
        }

        private readonly string _uriString;
        private T _loadedResource;

        /// <summary>
        /// Gets or loads the resource.
        /// </summary>
        /// <param name="throwsExceptionIfFailed">Indicates whether throws exception when loading resource failed.</param>
        /// <returns>The resource.</returns>
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

        /// <summary>
        /// Gets the URI suffix.
        /// </summary>
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
