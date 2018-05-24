using System.Collections.ObjectModel;

namespace DevZest.Data.Primitives
{
    public static class ExtensibleObjectExtensions
    {
        public static ReadOnlyCollection<T> GetExtensions<T>(this ExtensibleObject extensibleObject)
            where T : class, IExtension
        {
            return extensibleObject.GetExtensions<T>();
        }

        public static T GetExtension<T>(this ExtensibleObject extensibleObject)
            where T : class, IExtension
        {
            return extensibleObject.GetExtension<T>();
        }

        public static IExtension GetExtension(this ExtensibleObject extensibleObject, object extensionKey)
        {
            return extensibleObject.GetExtension(extensionKey);
        }

        public static void AddExtension(this ExtensibleObject extensibileObject, IExtension extension)
        {
            extensibileObject.AddExtension(extension);
        }

        public static void AddOrUpdateExtension(this ExtensibleObject extensibleObject, IExtension extension)
        {
            extensibleObject.AddOrUpdateExtension(extension);
        }

        public static bool ContainsExtension(this ExtensibleObject extensibleObject, object extensionKey)
        {
            return extensibleObject.ContainsExtension(extensionKey);
        }

        public static void RemoveExtension(this ExtensibleObject extensibleObject, object extensionKey)
        {
            extensibleObject.RemoveExtension(extensionKey);
        }
    }
}
