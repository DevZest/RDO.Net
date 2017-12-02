
using System.Collections.ObjectModel;

namespace DevZest.Data.Primitives
{
    public static class ResourceContainerExtensions
    {
        public static ReadOnlyCollection<T> GetResources<T>(this ResourceContainer resourceContainer)
            where T : class, IResource
        {
            return resourceContainer.GetResources<T>();
        }

        public static T GetResource<T>(this ResourceContainer resourceContainer)
            where T : class, IResource
        {
            return resourceContainer.GetResource<T>();
        }

        public static IResource GetResource(this ResourceContainer resourceContainer, object resourceKey)
        {
            return resourceContainer.GetResource(resourceKey);
        }

        public static void AddResource(this ResourceContainer resourceContainer, IResource interceptor)
        {
            resourceContainer.AddResource(interceptor);
        }

        public static void AddOrUpdateResource(this ResourceContainer resourceContainer, IResource resource)
        {
            resourceContainer.AddOrUpdateResource(resource);
        }

        public static bool ContainsResource(this ResourceContainer resourceContainer, object resourceKey)
        {
            return resourceContainer.ContainsResource(resourceKey);
        }

        public static void RemoveResource(this ResourceContainer resourceContainer, object resourceKey)
        {
            resourceContainer.RemoveResource(resourceKey);
        }
    }
}
