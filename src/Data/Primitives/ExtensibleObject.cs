using System.Collections.ObjectModel;
using System.Linq;

namespace DevZest.Data.Primitives
{
    public abstract partial class ExtensibleObject
    {
        protected ExtensibleObject()
        {
            _extensions = new ExtensionCollection(this);
        }

        private ExtensionCollection _extensions;

        internal ReadOnlyCollection<T> GetExtensions<T>()
            where T : class, IExtension
        {
            return _extensions.Filter<T>();
        }

        internal T GetExtension<T>()
            where T : class, IExtension
        {
            return GetExtensions<T>().SingleOrDefault();
        }

        internal IExtension GetExtension(object extensionKey)
        {
            if (!ContainsExtension(extensionKey))
                return null;
            return _extensions[extensionKey];
        }

        internal void AddExtension(IExtension extension)
        {
            extension.VerifyNotNull(nameof(extension));

            _extensions.Add(extension);
        }

        internal void AddOrUpdateExtension(IExtension extension)
        {
            extension.VerifyNotNull(nameof(extension));

            var extensionKey = extension.Key;
            if (_extensions.Contains(extensionKey))
                _extensions.Remove(extensionKey);
            _extensions.Add(extension);
        }

        internal bool ContainsExtension(object extensionKey)
        {
            extensionKey.VerifyNotNull(nameof(extensionKey));

            return _extensions.Contains(extensionKey);
        }

        internal void RemoveExtension(object extensionKey)
        {
            extensionKey.VerifyNotNull(nameof(extensionKey));

            _extensions.Remove(extensionKey);
        }

        internal void BrutalRemoveExtension(object extensionKey)
        {
            _extensions.AllowFrozenChange(true);
            RemoveExtension(extensionKey);
            _extensions.AllowFrozenChange(false);
        }
    }
}
