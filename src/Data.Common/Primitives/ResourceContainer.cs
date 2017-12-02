using DevZest.Data.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DevZest.Data.Primitives
{
    public abstract partial class ResourceContainer
    {
        protected ResourceContainer()
        {
            _resources = new ResourceCollection(this);
        }

        private ResourceCollection _resources;

        internal ReadOnlyCollection<T> GetResources<T>()
            where T : class, IResource
        {
            return _resources.Filter<T>();
        }

        internal T GetResource<T>()
            where T : class, IResource
        {
            return GetResources<T>().SingleOrDefault();
        }

        internal IResource GetResource(object resourceKey)
        {
            if (!ContainsResource(resourceKey))
                return null;
            return _resources[resourceKey];
        }

        internal void AddResource(IResource resource)
        {
            Check.NotNull(resource, nameof(resource));

            _resources.Add(resource);
        }

        internal void AddOrUpdateResource(IResource resource)
        {
            Check.NotNull(resource, nameof(resource));

            var resourceKey = resource.Key;
            if (_resources.Contains(resourceKey))
                _resources.Remove(resourceKey);
            _resources.Add(resource);
        }

        internal bool ContainsResource(object resourceKey)
        {
            Check.NotNull(resourceKey, nameof(resourceKey));

            return _resources.Contains(resourceKey);
        }

        internal void RemoveResource(object resourceKey)
        {
            Check.NotNull(resourceKey, nameof(resourceKey));

            _resources.Remove(resourceKey);
        }

        internal void BrutalRemoveResource(object resourceKey)
        {
            _resources.AllowFrozenChange(true);
            RemoveResource(resourceKey);
            _resources.AllowFrozenChange(false);
        }
    }
}
